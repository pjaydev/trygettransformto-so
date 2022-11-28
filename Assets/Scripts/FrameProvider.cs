using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

#if !UNITY_EDITOR
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Windows.Media.Devices.Core;
using Windows.Perception.Spatial; // for Native SpatialCoordinateSystem
#else
using EditorHelper;
using Microsoft.Windows.Perception.Spatial; // for Unity-compatible SpatialCoordinateSystem
                                            // (to avoid compiler errors, not used on HL)
#endif

public class FrameProvider : MonoBehaviour
{
    #region unity references

    [SerializeField] private TextMeshProUGUI statusTextUi;

    #endregion unity references
    
    #region private fields

    enum AppState
    {
        Idle,
        MatrixAvailable,
        MatrixUnavailable,
        Starting,
        Restoring,
        Error
    }
    
    private MediaCapture _mediaCapture;
    private MediaFrameReader _mediaFrameReader;
    private SpatialCoordinateSystem _unityReferenceCoordinateSystem;
    private AppState _currentState = AppState.Idle;
    private AppState _lastState = AppState.Idle;
    private bool _isFrameReaderActive;
    
    #endregion private fields
    
    #region Frame Acquisition related methods
    
    private void ProcessFrame(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        var currentFrameReference = sender.TryAcquireLatestFrame();

        if (currentFrameReference == null)
        {
            Debug.LogWarning("Frame was null");
            return;
        }

        var frameCoordinateSystem = currentFrameReference.CoordinateSystem;
        
        System.Numerics.Matrix4x4? cameraToUnityRaw = frameCoordinateSystem.TryGetTransformTo(_unityReferenceCoordinateSystem);

        if (cameraToUnityRaw.HasValue)
        {
            Debug.Log("TryGetTransformTo returned a transformation matrix.");
            _currentState = AppState.MatrixAvailable;
            
        }
        else
        {
            Debug.LogError("TryGetTransformTo DID NOT RETURN A TRANSFORMATION MATRIX");
            _currentState = AppState.MatrixUnavailable;
        }
    }

    private async void StartFrameReading()
    {
        if (_isFrameReaderActive)
            return;
        
        _isFrameReaderActive = true;

        try
        {
            await InitFrameReader();

            _mediaFrameReader.FrameArrived += ProcessFrame;

            await _mediaFrameReader.StartAsync();
        }
        catch (Exception e)
        {
            if(_mediaFrameReader != null)
                _mediaFrameReader.FrameArrived -= ProcessFrame;
            
            _isFrameReaderActive = false;
            _currentState = AppState.Error;
            Debug.LogError($"Error initializing frame reader: {e}\n{e.StackTrace}");
        }

    }

    private async void StopFrameReading()
    {
        if (_mediaFrameReader == null || !_isFrameReaderActive)
            return;
        
        _isFrameReaderActive = false;
        
        _mediaFrameReader.FrameArrived -= ProcessFrame;

        await _mediaFrameReader.StopAsync();
        
        _mediaFrameReader.Dispose();
        _mediaCapture.Dispose();

    }


    private async Task InitFrameReader()
    {
        _mediaCapture = new MediaCapture();

        // Find the sources
        IReadOnlyList<MediaFrameSourceGroup> allGroups = await MediaFrameSourceGroup.FindAllAsync();

        var sourceGroups = allGroups.Select(g => new
        {
            Group = g,
            SourceInfo = g.SourceInfos.FirstOrDefault(
                i =>
                    i.SourceKind == MediaFrameSourceKind.Color &&
                    i.MediaStreamType == MediaStreamType.VideoRecord)
        }).Where(g => g.SourceInfo != null).ToList();

        var selectedSource = sourceGroups.FirstOrDefault();

        if (selectedSource == default)
        {
            // No camera sources found
            throw new Exception("No video sources found");
        }

        string deviceId = selectedSource.SourceInfo.DeviceInformation.Id;
        MediaFrameSourceGroup sourceGroup = selectedSource.Group;

        // HL2 High Resolution Frame
        int width = 2272;
        int height = 1278;
        int frameRate = 15;

        IReadOnlyList<MediaCaptureVideoProfile> profiles = MediaCapture.FindAllVideoProfiles(deviceId);

        var profileMatch = (
            from profile in profiles
            from desc in profile.SupportedRecordMediaDescription
            where desc.Width == width && desc.Height == height && desc.FrameRate == frameRate
            select new { profile, desc }
        ).FirstOrDefault(); // Select the Profile with the required resolution from all available profiles.

        if (profileMatch == null)
        {
            throw new Exception("No supported video profile found.");
        }

        var settings = new MediaCaptureInitializationSettings()
        {
            SourceGroup = sourceGroup,
            SharingMode = MediaCaptureSharingMode.ExclusiveControl,
            StreamingCaptureMode = StreamingCaptureMode.Video,
            MemoryPreference = MediaCaptureMemoryPreference.Cpu,
            VideoProfile = profileMatch.profile,
            RecordMediaDescription = profileMatch.desc
        };

        try
        {
            await _mediaCapture.InitializeAsync(settings);
        }
        catch (Exception ex)
        {
            Debug.LogError("*FrameProvider: MediaCapture initialization failed: " + ex.Message);
            throw;
        }

        // Create the frame reader
        MediaFrameSource frameSource = _mediaCapture.FrameSources[selectedSource.SourceInfo.Id];

        IReadOnlyList<MediaFrameFormat> formats = frameSource.SupportedFormats;
        MediaFrameFormat myformat = formats.FirstOrDefault(format =>
            (format.FrameRate.Numerator == frameRate &&
             format.VideoFormat.Width == width &&
             format.VideoFormat.Height == height));


        await frameSource.SetFormatAsync(myformat);

        _mediaFrameReader =
            await _mediaCapture.CreateFrameReaderAsync(frameSource,
                MediaEncodingSubtypes.Yuy2);
        
        _unityReferenceCoordinateSystem =
            PerceptionInterop.GetSceneCoordinateSystem(Pose.identity) as SpatialCoordinateSystem;
                // see https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10082#issuecomment-905865993
    }
    
    #endregion Frame Acquisition related methods
    
    #region Unity Event Handlers

    private void Start()
    {
        _currentState = AppState.Starting;
        StartFrameReading();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            _currentState = AppState.Restoring;
            StartFrameReading();
        }
        else
        {
            StopFrameReading();
        }
    }

    private void Update()
    {
        if (_lastState == _currentState)
        {
            return;
        }

        switch (_currentState)
        {
            case AppState.Starting:
                statusTextUi.text = "Initial Start. Loading frame reader...";
                break;
            case AppState.Restoring:
                statusTextUi.text = "Restoring from Stand-by...";
                break;
            case AppState.MatrixAvailable:
                statusTextUi.text = "Matrix available";
                break;
            case AppState.MatrixUnavailable:
                statusTextUi.text = "Matrix UNAVAILABLE";
                break;
            case AppState.Error:
                statusTextUi.text = "Error on initialization.\nSee Log for details.";
                break;
            
        }

        _lastState = _currentState;

    }

    #endregion Unity Event Handlers
}
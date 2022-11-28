
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Windows.Perception.Spatial;

#pragma warning disable 1998

#if UNITY_EDITOR

//This is a collection of several dummy classes that are needed for scripts in the editor to compile
namespace EditorHelper
{
    public class MediaCapture
    {
        public VideoDeviceController VideoDeviceController;
        public IReadOnlyDictionary<string, MediaFrameSource> FrameSources { get; }

        public async Task InitializeAsync(MediaCaptureInitializationSettings settings)
        {
            throw new System.NotImplementedException();
        }

        public async Task<MediaFrameReader> CreateFrameReaderAsync(MediaFrameSource frameSource,
            MediaEncodingSubtypes mediaEncodingSubtype)
        {
            throw new NotImplementedException();
        }

        internal static IReadOnlyList<MediaCaptureVideoProfile> FindAllVideoProfiles(string deviceid)
        {
            throw new NotImplementedException();
        }

        internal static bool IsVideoProfileSupported(string deviceid)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public enum MediaEncodingSubtypes
    {
        Bgra8,
        Yuy2
    }

    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);

    public class MediaFrameReader
    {
        public event TypedEventHandler<MediaFrameReader, MediaFrameArrivedEventArgs> FrameArrived;

        public async Task StartAsync()
        {
            throw new NotImplementedException();
        }

        public MediaFrameReference TryAcquireLatestFrame()
        {
            throw new NotImplementedException();
        }

        public async Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class MediaFrameReference
    {
        public VideoMediaFrame VideoMediaFrame;

        public TimeSpan? SystemRelativeTime { get; internal set; }
        public SpatialCoordinateSystem CoordinateSystem { get; internal set; }
    }

    public class VideoMediaFrame
    {
        public SoftwareBitmap SoftwareBitmap { get; }
        public CameraIntrinsics CameraIntrinsics { get; internal set; }
    }

    public class SoftwareBitmap : IDisposable
    {
        public int PixelHeight { get; internal set; }
        public int PixelWidth { get; internal set; }

        public static SoftwareBitmap Convert(SoftwareBitmap inputBitmap, BitmapPixelFormat bgra8,
            BitmapAlphaMode premultiplied)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class MediaFrameArrivedEventArgs
    {
    }

    public enum BitmapPixelFormat
    {
        Bgra8,
        Rgba8
    }

    public enum BitmapAlphaMode
    {
        BitmapAlphaMode,
        Premultiplied,
        Ignore
    }

    public class MediaFrameSourceGroup
    {
        public static async Task<IReadOnlyList<MediaFrameSourceGroup>> FindAllAsync()
        {
            return new List<MediaFrameSourceGroup>();
        }

        public IReadOnlyList<MediaFrameSourceInfo> SourceInfos { get; }
    }

    public enum MediaFrameSourceKind
    {
        Color
    }

    public class MediaFrameSourceInfo
    {
        public MediaFrameSourceKind SourceKind;
        public string Id;

        public MediaStreamType MediaStreamType { get; internal set; }
        public DeviceInformation DeviceInformation { get; internal set; }
    }

    public class DeviceInformation
    {
        public string Id { get; internal set; }
    }

    public enum MediaCaptureSharingMode
    {
        ExclusiveControl
    }

    public enum StreamingCaptureMode
    {
        Video
    }

    public enum MediaCaptureMemoryPreference
    {
        Cpu
    }

    public class MediaCaptureInitializationSettings
    {
        public MediaFrameSourceGroup SourceGroup;
        public MediaCaptureSharingMode SharingMode;
        public StreamingCaptureMode StreamingCaptureMode;
        public MediaCaptureMemoryPreference MemoryPreference;
        public MediaCaptureVideoProfile VideoProfile;
        public MediaCaptureVideoProfileMediaDescription RecordMediaDescription;
        public String VideoDeviceId;
    }

    public class MediaCaptureVideoProfile
    {
        public IReadOnlyList<MediaCaptureVideoProfileMediaDescription> SupportedRecordMediaDescription
        {
            get;
            internal set;
        }
    }

    public class MediaCaptureVideoProfileMediaDescription
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int FrameRate { get; internal set; }
    }

    public enum MediaStreamType
    {
        VideoRecord
    }

    public interface IMediaEncodingProperties
    {
        string Subtype { get; set; }
    }

    public class VideoDeviceController
    {
        public ExposureControl ExposureControl { get; internal set; }

        public IReadOnlyList<IMediaEncodingProperties> GetAvailableMediaStreamProperties(
            MediaStreamType mediaStreamType)
        {
            throw new NotImplementedException();
        }

        public async Task SetMediaStreamPropertiesAsync(MediaStreamType videoRecord,
            IMediaEncodingProperties encodingProperties)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaFrameSource
    {
        public IReadOnlyList<MediaFrameFormat> SupportedFormats { get; internal set; }

        internal Task SetFormatAsync(object myformat)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaFrameFormat
    {
        public FrameRate FrameRate { get; internal set; }
        public VideoFormat VideoFormat { get; internal set; }
    }

    public class VideoFormat
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    public class FrameRate
    {
        public int Numerator { get; internal set; }
    }

    public class ImageEncodingProperties : IMediaEncodingProperties
    {
        public uint Height { get; set; }
        public uint Width { get; set; }

        public string Subtype { get; set; }
    }

    public class MediaRatio
    {
        public uint Denominator { get; set; }
        public uint Numerator { get; set; }
    }

    public class VideoEncodingProperties : IMediaEncodingProperties
    {
        public MediaRatio FrameRate { get; }

        public uint Height { get; set; }
        public uint Width { get; set; }
        public string Subtype { get; set; }
    }

    public class CameraIntrinsics
    {
        public FocalLength FocalLength { get; internal set; }
        public PrincipalPoint PrincipalPoint { get; internal set; }
        public RadialDistortion RadialDistortion { get; internal set; }
        public TangentialDistortion TangentialDistortion { get; internal set; }
        
    }

    public class RadialDistortion
    {
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Z { get; internal set; }
    }

    public class PrincipalPoint
    {
        public float X { get; internal set; }
        public float Y { get; internal set; }
    }

    public class TangentialDistortion
    {
        public float Y { get; internal set; }
        public float X { get; internal set; }
    }

    public class FocalLength
    {
        public float X { get; internal set; }
        public float Y { get; internal set; }
    }

    public class ExposureControl
    {
        public TimeSpan Value { get; internal set; }
        public TimeSpan Min { get; internal set; }
        public bool Supported { get; internal set; }

        internal Task SetValueAsync(object min)
        {
            throw new NotImplementedException();
        }
    }
}


#endif
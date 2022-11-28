# MCVE for SpatialCoordinateSystem.TryGetTransformTo() issue

## Summary

This is a minimal reproducible example to demonstrate the problem when calling `Windows.Perception.Spatial.SpatialCoordinateSystem.TryGetTransformTo()` with having the HoloLens 2 in a separated spatial environment after restoring from Stand-by, which differs from the one when the HL2 app was initially started.

The detailed problem description is to find in the related StackOverflow post:

https://stackoverflow.com/questions/74545055/spatialcoordinatesystem-trygettransformto-from-unity-to-webcam-space-fails-in

## How To Use

To reproduce the problem, open the project in Unity v2021.3.8f, switch platform to UWP and set the appropriate build settings for HL2 (see https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/new-openxr-project-with-mrtk#set-your-build-target). Follow the MRTK Setup assistant chosing the OpenXR Plug-In. 

Then build, and deploy the `Scenes/SampleScene` to HL2 and perform the following steps:

1. Start the app on HL2
2. App acquires, initializes and starts the `MediaFrameReader` and tries to acquire the `cameraToUnityRaw` matrix for each frame --\> works fine (Status text shows "Matrix available" after a short loading time)
3. Set the HL to stand-by. `MediaFrameReader` will be stopped and disposed.
4. Go to another room, for which the HL's spatial awareness does not know the connection between these two rooms
5. Wake up HL and open the (still running) app. The app will re-initialize the MediaFrameReader and start frame processing.
6. Acquire the `cameraToUnityRaw` matrix.  =\> **FAILS:**  `cameraToUnityRaw.HasValue` returns false. (Status text shows "Matrix UNAVAILABLE" after a short loading time)
7. Set the HL to stand-by again. `MediaFrameReader` will be stopped and disposed.
8. Go back to the initial environment where the app was originaly started
9. Wake up HL and open the (still running) app. The app will re-initialize the MediaFrameReader and start frame processing.
10. App will reinitialise `cameraToUnityRaw` matrix as described =\> works fine again! (`cameraToUnityRaw` has valid value again)

**IMPORTANT**: 
To successfully reproduce the problem you need to make sure the HL does not know the spatial *connection* between the two spatial environments. To verify this, in the HL go to the Settings-\>System-\>Holograms and press the "Remove all holograms" button before doing the test. Make sure to always have the HL in Stand-By when bringing it from the one to the other spatial environment.

## Debugging

All the relevant code you find in the `Assets\Scripts\FrameProvider.cs` which should be self-explanatory. 

If you have any questions or remarks, please state an issue or add a comment to my StackOverflow post.
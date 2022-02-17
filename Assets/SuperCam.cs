/* 
*   NatCorder-NatDevice Integration
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Examples {

    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using Devices;
    using Devices.Outputs;
    using Recorders;
    using Recorders.Clocks;
    using Recorders.Inputs;

    public sealed class SuperCam : MonoBehaviour {

        #region --Inspector--
        [Header(@"Camera Preview")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        [Header(@"Video Recording")]
        public int videoWidth = 720;
        public int videoHeight = 1280;
        public Camera recordingCamera;
        public bool recordAudio;
        #endregion


        #region --Operations--
        CameraDevice cameraDevice;
        AudioDevice audioDevice;
        TextureOutput previewOutput;
        IMediaRecorder recorder;
        CameraInput cameraInput;

        async void Start () {
            // Request permissions
            if (await MediaDeviceQuery.RequestPermissions<CameraDevice>() != PermissionStatus.Authorized) {
                Debug.LogError("User did not grant camera permissions");
                return;
            }
            if (await MediaDeviceQuery.RequestPermissions<AudioDevice>() != PermissionStatus.Authorized) {
                Debug.LogWarning("User did not grant microphone permissions");
                recordAudio = false;
            }
            // Get media devices
            var query = new MediaDeviceQuery();
            cameraDevice = query.FirstOrDefault(device => device is CameraDevice) as CameraDevice;
            audioDevice = query.FirstOrDefault(device => device is AudioDevice) as AudioDevice;
            // Start the camera preview
            cameraDevice.previewResolution = (1280, 720);
            previewOutput = new TextureOutput();
            cameraDevice.StartRunning(previewOutput);
            // Display preview
            var previewTexture = await previewOutput;
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
        }

        void OnDestroy () {
            // Dispose the preview output
            previewOutput?.Dispose();
        }
        #endregion


        #region --UI Delegates--

        public void StartRecording () {
            // Create recorder
            var sampleRate = recordAudio ? audioDevice.sampleRate : 0;
            var channelCount = recordAudio ? audioDevice.channelCount : 0;
            var clock = new RealtimeClock();
            recorder = new MP4Recorder(videoWidth, videoHeight, 30, sampleRate, channelCount);
            // Record the main camera
            cameraInput = new CameraInput(recorder, clock, recordingCamera);
            // Record the microphone
            if (recordAudio)
                audioDevice.StartRunning(audioBuffer => recorder.CommitSamples(audioBuffer.sampleBuffer.ToArray(), clock.timestamp));
        }

        public async void StopRecording () {
            // Stop the microphone stream
            if (audioDevice.running)
                audioDevice.StopRunning();
            // Stop the camera recording
            cameraInput.Dispose();
            // Finish writing video
            var recordingPath = await recorder.FinishWriting();
            Debug.Log($"Saved recording to: {recordingPath}");
            // Playback recording
            Handheld.PlayFullScreenMovie($"file://{recordingPath}");
        }
        #endregion
    }
}
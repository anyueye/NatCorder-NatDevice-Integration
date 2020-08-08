/* 
*   NatSuite Integrations
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Integrations {

    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using Devices;
    using Recorders;
    using Recorders.Clocks;
    using Recorders.Inputs;

    public sealed partial class SuperCam : MonoBehaviour {

        [Header(@"Camera Preview")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        [Header(@"Video Recording")]
        public int videoWidth = 720;
        public int videoHeight = 1280;
        public Camera recordingCamera;

        [Header(@"Buttons")]
        public Button muteAudioButton;
        public Button saveImageButton;

        CameraDevice cameraDevice;
        AudioDevice audioDevice;
        IMediaRecorder recorder;
        CameraInput cameraInput;
        
        bool recordAudio;


        #region --Operations--

        async void Start () {
            // Request permissions
            if (!await MediaDeviceQuery.RequestPermissions<CameraDevice>()) {
                Debug.LogError("User failed to grant camera permissions");
                return;
            }
            if (!await MediaDeviceQuery.RequestPermissions<AudioDevice>()) {
                Debug.LogError("User failed to grant microphone permissions");
                return;
            }
            // Get media devices
            var query = new MediaDeviceQuery();
            cameraDevice = query.FirstOrDefault(device => device is CameraDevice) as CameraDevice; // will be `null` on standalone platforms
            audioDevice = query.FirstOrDefault(device => device is AudioDevice) as AudioDevice;
            // Start the camera preview
            cameraDevice.previewResolution = (1280, 720);
            var previewTexture = await cameraDevice.StartRunning();
            // Display preview
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
        }
        #endregion


        #region --UI Delegates--

        void StartRecording () {
            // Create recorder
            var clock = new RealtimeClock();
            recorder = recordAudio ?
                new MP4Recorder(videoWidth, videoHeight, 30, audioDevice.sampleRate, audioDevice.channelCount) :
                new MP4Recorder(videoWidth, videoHeight, 30);
            // Stream media samples to the recorder
            cameraInput = new CameraInput(recorder, clock, recordingCamera);
            if (recordAudio)
                audioDevice.StartRunning((sampleBuffer, _) => recorder.CommitSamples(sampleBuffer, clock.timestamp));
        }

        async void StopRecording () {
            // Stop committing media samples
            if (audioDevice.running)
                audioDevice.StopRunning();
            cameraInput.Dispose();
            // Finish writing video
            var recordingPath = await recorder.FinishWriting();
            Debug.Log($"Saved recording to: {recordingPath}");
            // Playback recording
            Handheld.PlayFullScreenMovie($"file://{recordingPath}");
        }

        async void CapturePhoto () {
            // Capture photo
            var photoTexture = await cameraDevice.CapturePhoto();
        }
        #endregion
    }
}
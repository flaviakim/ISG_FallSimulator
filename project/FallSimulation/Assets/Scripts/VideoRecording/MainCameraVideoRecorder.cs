#if UNITY_EDITOR
using System;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace VideoRecording {
    /// <summary>
    /// Wraps Unity Recorder's <see cref="RecorderController"/> to capture a .mp4 video
    /// from the MainCamera. The output file path should be provided without an extension;
    /// Unity Recorder appends ".mp4" automatically.
    /// </summary>
    public class MainCameraVideoRecorder : IDisposable {
        private readonly RecorderController         _controller;
        private readonly RecorderControllerSettings _controllerSettings;
        private readonly MovieRecorderSettings      _movieSettings;

        /// <param name="outputFilePath">Full path without extension, e.g. "PoseRecordings/2024-01-01_00-00-00/fall_recording_..._main_camera"</param>
        /// <param name="frameRate">Target frame rate (frames per second).</param>
        /// <param name="flipOutput">When true, each captured frame is flipped vertically to correct Y-axis inversion (OpenGL/Metal).</param>
        public MainCameraVideoRecorder(string outputFilePath, int frameRate, bool flipOutput = false) {
            _movieSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            _movieSettings.name                   = "MainCameraVideo";
            _movieSettings.Enabled                = true;
            _movieSettings.OutputFile             = outputFilePath;
            _movieSettings.FrameRate              = frameRate;
            _movieSettings.CaptureAlpha           = false;

            var cameraInput = new CameraInputSettings {
                Source          = ImageSource.MainCamera,
                OutputWidth     = 1280,
                OutputHeight    = 720,
                FlipFinalOutput = flipOutput,
            };
            _movieSettings.ImageInputSettings = cameraInput;

            _controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _controllerSettings.AddRecorderSettings(_movieSettings);
            _controllerSettings.SetRecordModeToManual();
            _controllerSettings.FrameRate = frameRate;

            _controller = new RecorderController(_controllerSettings);
        }

        /// <summary>Prepares and starts the video recording session.</summary>
        public void StartRecording() {
            _controller.PrepareRecording();
            _controller.StartRecording();
        }

        /// <summary>Stops recording and destroys the ScriptableObject instances.</summary>
        public void Dispose() {
            if (_controller.IsRecording()) {
                _controller.StopRecording();
            }

            if (_movieSettings != null) {
                UnityEngine.Object.DestroyImmediate(_movieSettings);
            }
            if (_controllerSettings != null) {
                UnityEngine.Object.DestroyImmediate(_controllerSettings);
            }
        }
    }
}
#endif

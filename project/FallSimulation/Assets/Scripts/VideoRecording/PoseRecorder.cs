using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VideoRecording {
    /// <summary>
    /// MonoBehaviour that spawns per-camera recorders and drives frame capture.
    /// Subscribes to <see cref="GameManager"/> events for start / stop.
    /// </summary>
    public class PoseRecorder : MonoBehaviour {
        [Header("Positions")]
        [SerializeField] private Transform fallCenterPosition;

        [Header("Camera Settings")]
        [SerializeField] private int   numberOfCameras        = 10;
        [SerializeField] private float fovMin                 = 60f;
        [SerializeField] private float fovMax                 = 60f;
        [SerializeField] private float distanceFromCenterMin  = 5f;
        [SerializeField] private float distanceFromCenterMax  = 10f;
        [SerializeField] private float angleVariance          = 15f;
        [SerializeField] private float heightMin              = 1f;
        [SerializeField] private float heightMax              = 3f;
        [SerializeField] private float aboveHeadHeightThreshold = 1.8f;
        [SerializeField] private float aspectRatio             = 16f / 9f;
        [SerializeField] private float nearClip                = 0.1f;
        [SerializeField] private float farClip                 = 100f;

        [Header("Recording Settings")]
        [SerializeField] private RecordingMode mode             = RecordingMode.Json;
        [SerializeField] private string        rootOutputFolder = "PoseRecordings";
        [SerializeField] private string        fileName         = "fall_recording";
        [SerializeField] private int           frameRate        = 30;
        [SerializeField] private int           numberOfPosePoints    = 33;
        [SerializeField] private bool          flipYAxisOutput  = false;
        

        public bool IsRecording { get; private set; }

        private float _recordingStartTime;

        private readonly List<PosePoint>           _posePoints      = new();
        private readonly List<SingleCameraRecorder> _cameraRecorders = new();

#if UNITY_EDITOR
        private MainCameraVideoRecorder _videoRecorder;
#endif

        // ---- Unity lifecycle ----------------------------------------------

        private void Start() {
            if (fallCenterPosition == null) {
                Debug.LogError("PoseRecorder: Fall Center Position is not set.");
                return;
            }
            
            var posePoints = FindObjectsByType<PosePoint>(FindObjectsSortMode.None)
                             .OrderBy(pp => pp.PoseID);
            _posePoints.AddRange(posePoints);

            // Wire GameManager events
            if (GameManager.Instance != null) {
                GameManager.Instance.OnStartRecording             += StartRecording;
                GameManager.Instance.OnEndRecordingAndWriteToFile += StopRecording;
            }
        }

        private void FixedUpdate() {
            if (IsRecording) RecordFrame();
        }

        private void OnDestroy() {
            if (GameManager.Instance != null) {
                GameManager.Instance.OnStartRecording             -= StartRecording;
                GameManager.Instance.OnEndRecordingAndWriteToFile -= StopRecording;
            }

            StopRecording(); // safety net
        }

        // ---- Public API ---------------------------------------------------

        /// <summary>Starts recording; opens all camera output resources.</summary>
        public void StartRecording() {
            if (IsRecording) return;
            BuildCameras();
            foreach (var recorder in _cameraRecorders) {
                recorder.Open();
            }
#if UNITY_EDITOR
            _videoRecorder?.StartRecording();
#endif
            _recordingStartTime = Time.time;
            IsRecording = true;
        }

        /// <summary>Stops recording and flushes all output files.</summary>
        public void StopRecording() {
            if (!IsRecording) return;
            IsRecording = false;
            foreach (var recorder in _cameraRecorders) {
                recorder.Dispose();
            }
            _cameraRecorders.Clear();
#if UNITY_EDITOR
            _videoRecorder?.Dispose();
            _videoRecorder = null;
#endif
        }

        // ---- Gizmos -------------------------------------------------------

        private void OnDrawGizmos() {
            if (fallCenterPosition != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(fallCenterPosition.position, 0.2f);
            }
        }

        // ---- Private helpers ----------------------------------------------

        private void RecordFrame() {
            int   frameNumber = Time.frameCount;
            float timeStamp   = Time.time - _recordingStartTime;

            foreach (var recorder in _cameraRecorders) {
                recorder.CaptureFrame(frameNumber, timeStamp, _posePoints);
            }
        }

        private void BuildCameras() {
            _cameraRecorders.Clear();  // no GameObjects to destroy anymore

            if (fallCenterPosition == null) return;

            CameraPosition[] positions = GetCameraPositions();
            DateTime         startTime = DateTime.Now;

            var outputFolder = Path.Combine(rootOutputFolder, $"{startTime:yyyy-MM-dd_HH-mm-ss}");
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

#if UNITY_EDITOR
            string videoFilePath = Path.Combine(outputFolder, $"{fileName}_{startTime:yyyy-MM-dd_HH-mm-ss}_main_camera");
            _videoRecorder = new MainCameraVideoRecorder(videoFilePath, frameRate, flipYAxisOutput);
#endif

            float subjectY = fallCenterPosition.position.y;

            for (int i = 0; i < numberOfCameras; i++) {
                var virtualCam = new VirtualCamera(
                    positions[i].Position,
                    positions[i].Rotation,
                    positions[i].FOV,
                    aspectRatio,
                    nearClip,
                    farClip);

                string heightLabel  = CameraHeightClassifier.Classify(positions[i].Position.y, subjectY, aboveHeadHeightThreshold);
                string baseFilePath = Path.Combine(outputFolder, $"{fileName}_{startTime:yyyy-MM-dd_HH-mm-ss}_{i}");
                IRecordingStrategy strategy = RecordingStrategyFactory.Create(mode, baseFilePath, numberOfPosePoints, heightLabel);

                _cameraRecorders.Add(new SingleCameraRecorder(virtualCam, strategy, numberOfPosePoints, flipYAxisOutput));
            }
        }

        private CameraPosition[] GetCameraPositions() {
            var positions = new CameraPosition[numberOfCameras];

            for (int i = 0; i < numberOfCameras; i++) {
                float angle    = i * (360f / numberOfCameras);
                float distance = Random.Range(distanceFromCenterMin, distanceFromCenterMax);
                float fov      = Random.Range(fovMin, fovMax);

                Vector3 position =
                    fallCenterPosition.position
                    + Quaternion.Euler(0, angle, 0) * new Vector3(distance, 0, 0)
                    + new Vector3(0, Random.Range(heightMin, heightMax), 0);

                Quaternion rotation =
                    Quaternion.LookRotation(fallCenterPosition.position - position)
                    * Quaternion.Euler(
                        Random.Range(-angleVariance, angleVariance),
                        Random.Range(-angleVariance, angleVariance),
                        0);

                positions[i] = new CameraPosition(position, rotation, fov);
            }

            return positions;
        }
    }
}

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
        // [SerializeField] private int   numberOfCameras        = 10;
        [SerializeField] private VaryingValue cameraPositionsAroundCenterRanges = new(new[] { new Range(0, 360, 10) });
        [SerializeField] private VaryingValue fovRanges = new(new[] { new Range(60f) });
        [SerializeField] private VaryingValue distanceFromCenterRanges = new(new[] { new Range(2f, 7f, 1) });
        [SerializeField] private VaryingValue cameraHorizontalRotationAngleRanges     = new(new[] { new Range(-10f, +10f, 1) });
        [SerializeField] private VaryingValue cameraVerticalRotationAngleRanges     = new(new[] { new Range(-10f, +10f, 1) });
        [SerializeField] private VaryingValue heightRanges    = new(new[] { new Range(1f, 2.5f, 1) });
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
        private int _frameNumber = 0;

        private readonly List<PosePoint>           _posePoints      = new();
        private readonly List<ISingleCameraRecorder> _cameraRecorders = new();

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
            _frameNumber = 0;
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
            int   frameNumber = _frameNumber++;
            float timeStamp   = Time.time - _recordingStartTime;

            foreach (var recorder in _cameraRecorders) {
                recorder.CaptureFrame(frameNumber, timeStamp, _posePoints);
            }
        }

        private void BuildCameras() {
            _cameraRecorders.Clear();  // no GameObjects to destroy anymore

            if (fallCenterPosition == null) return;

            // CameraPositionInfo[] positions = GetCameraPositions();
            DateTime         startTime = DateTime.Now;

            var outputFolder = Path.Combine(rootOutputFolder, $"{startTime:yyyy-MM-dd_HH-mm-ss}");
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

#if UNITY_EDITOR
            string videoFilePath = Path.Combine(outputFolder, $"{fileName}_{startTime:yyyy-MM-dd_HH-mm-ss}_main_camera");
            _videoRecorder = new MainCameraVideoRecorder(videoFilePath, frameRate);
#endif

            float subjectY = fallCenterPosition.position.y;
            
            foreach (float cameraAngleAroundCenter in cameraPositionsAroundCenterRanges.GetAllRandomPositions()) {
                foreach (var fov in fovRanges.GetAllRandomPositions()) {
                    foreach (float distanceFromCenter in distanceFromCenterRanges.GetAllRandomPositions()) {
                        foreach (var heightRange in heightRanges.GetAllRandomPositions()) {
                            foreach (var verticalAngle in cameraVerticalRotationAngleRanges.GetAllRandomPositions()) {
                                foreach (float horizontalAngle in cameraHorizontalRotationAngleRanges.GetAllRandomPositions()) {
                                    var cameraPositionInfo = GetCameraPositionInfo(
                                        cameraAngleAroundCenter,
                                        fov,
                                        distanceFromCenter,
                                        heightRange,
                                        verticalAngle,
                                        horizontalAngle
                                    );
                                    var virtualCam = new VirtualCamera(
                                        cameraPositionInfo.Position,
                                        cameraPositionInfo.Rotation,
                                        cameraPositionInfo.FOV,
                                        aspectRatio,
                                        nearClip,
                                        farClip
                                    );
                                    
                                    string heightLabel = CameraHeightClassifier.Classify(
                                        cameraPositionInfo.Position.y,
                                        subjectY,
                                        aboveHeadHeightThreshold
                                    );
                                    string baseFilePath = Path.Combine(
                                        outputFolder,
                                        $"{fileName}_{startTime:yyyy-MM-dd_HH-mm-ss}_{_cameraRecorders.Count}"
                                    );
                                    IRecordingStrategy strategy = RecordingStrategyFactory.Create(
                                        mode,
                                        baseFilePath,
                                        numberOfPosePoints,
                                        heightLabel
                                    );
                                    _cameraRecorders.Add(
                                        new SingleCameraRecorder(virtualCam, strategy, numberOfPosePoints, flipYAxisOutput)
                                    );
                                }
                            }
                        }
                    }
                }
            }
            
            _cameraRecorders.Add(
                new FullPoseInformationRecorder(
                    RecordingStrategyFactory.Create(
                        RecordingMode.Full3DCSV,
                        Path.Combine(outputFolder, $"{fileName}_full3D_{startTime:yyyy-MM-dd_HH-mm-ss}"),
                        numberOfPosePoints,
                        "none"
                    ),
                    numberOfPosePoints
                )
            );
        }
        
        public CameraPositionInfo GetCameraPositionInfo(
            float cameraAngleAroundCenter,
            float fov,
            float distanceFromCenter,
            float heightRange,
            float verticalAngle,
            float horizontalAngle
        ) {
            Vector3 position =
                fallCenterPosition.position
                + (Quaternion.Euler(0, cameraAngleAroundCenter, 0) * new Vector3(distanceFromCenter, 0, 0))
                + new Vector3(0, heightRange, 0);
            
            Quaternion rotation =
                Quaternion.LookRotation(fallCenterPosition.position - position)
                * Quaternion.Euler(
                    horizontalAngle,
                    verticalAngle,
                    0
                );
            
            return new CameraPositionInfo(position, rotation, fov);
        }

        // private CameraPositionInfo[] GetCameraPositions() {
        //     var positions = new CameraPositionInfo[numberOfCameras];
        //
        //     for (int i = 0; i < numberOfCameras; i++) {
        //         float angle    = i * (360f / numberOfCameras);
        //         float distance = Random.Range(distanceFromCenterMin, distanceFromCenterMax);
        //         float fov      = Random.Range(fovMin, fovMax);
        //
        //         Vector3 position =
        //             fallCenterPosition.position
        //             + Quaternion.Euler(0, angle, 0) * new Vector3(distance, 0, 0)
        //             + new Vector3(0, Random.Range(heightMin, heightMax), 0);
        //
        //         Quaternion rotation =
        //             Quaternion.LookRotation(fallCenterPosition.position - position)
        //             * Quaternion.Euler(
        //                 Random.Range(-angleVariance, angleVariance),
        //                 Random.Range(-angleVariance, angleVariance),
        //                 0);
        //
        //         positions[i] = new CameraPositionInfo(position, rotation, fov);
        //     }
        //
        //     return positions;
        // }
    }
    
    [Serializable]
    public struct VaryingValue {
        [SerializeField] private Range[] ranges;
        
        public VaryingValue(Range[] ranges) {
            this.ranges = ranges;
        }
        
        public float[] GetAllRandomPositions() {
            List<float> values = new();
            foreach (var range in ranges) {
                for (int i = 0; i < range.RecordingCount; i++) {
                    values.Add(range.GetRandomValue());
                }
            }
            return values.ToArray();
        }
    }
    
    [Serializable]
    public struct Range {
        [SerializeField] private float min;
        [SerializeField] private float max;
        
        [SerializeField] private int recordingCount;
        public int RecordingCount => recordingCount;
        
        public Range(float min, float max, int recordingCount) {
            this.min = min;
            this.max = max;
            this.recordingCount = recordingCount;
        }
        
        public Range(float value, int recordingCount = 1) {
            this.min = value;
            this.max = value;
            this.recordingCount = recordingCount;
        }

        public float GetRandomValue() {
            return Random.Range(min, max);
        }
    }
}

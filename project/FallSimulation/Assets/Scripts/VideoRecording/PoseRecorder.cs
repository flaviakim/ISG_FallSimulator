using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VideoRecording {
    public class PoseRecorder : MonoBehaviour {
        [Header("Positions")]
        [SerializeField] private Transform fallCenterPosition;
        [SerializeField] private Transform personOriginPosition;

        [Header("Camera Settings")]
        [SerializeField] private int numberOfCameras = 5;
        [SerializeField] private float fovMin = 60f;
        [SerializeField] private float fovMax = 60f;
        [SerializeField] private float distanceFromCenterMin = 5f;
        [SerializeField] private float distanceFromCenterMax = 10f;
        [SerializeField] private float angleVariance = 15f;
        [SerializeField] private float heightMin = 1f;
        [SerializeField] private float heightMax = 3f;
        
        [Header("Recording Settings")]
        [SerializeField] private string outputFolder = "PoseRecordings";
        [SerializeField] private string fileName = "fall_recording";
        [SerializeField] private int frameRate = 30;
        [SerializeField] private int highestPoseID = 32;
        
        public bool IsRecording { get; private set; } = true;

        private readonly List<PosePoint> _posePoints = new();
        private readonly List<SingleCameraRecorder> _cameraRecorders = new();

        private void Awake() {
            
        }

        private void Start() {
            if (fallCenterPosition == null) {
                Debug.LogError("Fall Center Position is not set in the VideoRecorder.");
                return;
            }

            CameraPosition[] cameraPositions = GetCameraPositions();

            DateTime startTime = DateTime.Now;
            for (int i = 0; i < numberOfCameras; i++) {
                GameObject cameraObject = new GameObject($"Camera_{i + 1}");
                cameraObject.transform.SetParent(transform);
                cameraObject.transform.position = cameraPositions[i].Position;
                cameraObject.transform.rotation = cameraPositions[i].Rotation;

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.fieldOfView = Random.Range(fovMin, fovMax);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;

                // var primitiveDebug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                // primitiveDebug.transform.SetParent(cameraObject.transform);
                // primitiveDebug.transform.localPosition = Vector3.zero;
                // primitiveDebug.transform.localRotation = Quaternion.Euler(90, 0, 0);
                // primitiveDebug.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                // primitiveDebug.GetComponent<Renderer>().material.color = Color.red;
                // primitiveDebug.name = "DebugCylinder";

                
                if (!Directory.Exists(outputFolder)) {
                    Directory.CreateDirectory(outputFolder);
                }

                _cameraRecorders.Add(new SingleCameraRecorder(i, outputFolder, startTime, camera, highestPoseID));
            }

            var posePoints = FindObjectsByType<PosePoint>(FindObjectsSortMode.None).OrderBy(pp => pp.PoseID);
            _posePoints.AddRange(posePoints);
        }

        private void FixedUpdate() {
            if (IsRecording) {
                RecordFrame();
            }
        }

        private void OnDestroy() {
            foreach (SingleCameraRecorder recorder in _cameraRecorders) {
                recorder.Close();
            }
        }

        private void RecordFrame() {
            int frameNumber = Time.frameCount;
            float timeStamp = Time.time;

            foreach (SingleCameraRecorder recorder in _cameraRecorders) {
                recorder.AppendFrameData(frameNumber, timeStamp, _posePoints);
            }
        }

        /// <summary>
        /// Returns an array of camera positions and rotations to be used for video recording.
        ///
        /// The camera positions
        /// </summary>
        /// <returns></returns>
        private CameraPosition[] GetCameraPositions() {
            CameraPosition[] cameraPositions = new CameraPosition[numberOfCameras];

            for (int i = 0; i < numberOfCameras; i++) {
                float angle = i * (360f / numberOfCameras);
                float distance = Random.Range(distanceFromCenterMin, distanceFromCenterMax);
                float fov = Random.Range(fovMin, fovMax);

                Vector3 position = fallCenterPosition.position
                                   + Quaternion.Euler(0, angle, 0) * new Vector3(distance, 0, 0) // moving it outwards
                                   + new Vector3(0, Random.Range(heightMin, heightMax), 0); // adding height
                Quaternion rotation = Quaternion.LookRotation(fallCenterPosition.position - position) *
                                      Quaternion.Euler(
                                          Random.Range(-angleVariance, angleVariance),
                                          Random.Range(-angleVariance, angleVariance),
                                          0);

                cameraPositions[i] = new CameraPosition(position, rotation, fov);
            }

            return cameraPositions;
        }

        private void OnDrawGizmos() {
            if (fallCenterPosition != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(fallCenterPosition.position, 0.2f);
            }

            Gizmos.color = Color.cyan;
            // if (_cameras.Count > 0) {
            //     foreach (var cam in _cameras) {
            //         Gizmos.DrawSphere(cam.transform.position, 0.1f);
            //         Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward);
            //     }
            // }
        }
    }

    public class CameraPosition {
        public float FOV { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public CameraPosition(Vector3 pos, Quaternion rot, float fov) {
            FOV = fov;
            Position = pos;
            Rotation = rot;
        }
    }
}
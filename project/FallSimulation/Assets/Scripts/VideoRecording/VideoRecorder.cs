using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace VideoRecording {
    public class VideoRecorder : MonoBehaviour {

        [SerializeField] private Transform fallCenterPosition;

        [SerializeField] private int numberOfCameras = 5;
        [SerializeField] private float fovMin = 60f;
        [SerializeField] private float fovMax = 60f;
        [SerializeField] private float distanceFromCenterMin = 5f;
        [SerializeField] private float distanceFromCenterMax = 10f;


        private void Start() {
            if (fallCenterPosition == null) {
                Debug.LogError("Fall Center Position is not set in the VideoRecorder.");
                return;
            }

            CameraPosition[] cameraPositions = GetCameraPositions();

            for (int i = 0; i < numberOfCameras; i++) {
                GameObject cameraObject = new GameObject($"Camera_{i + 1}");
                cameraObject.transform.SetParent(transform);
                cameraObject.transform.position = cameraPositions[i].position;
                cameraObject.transform.rotation = cameraPositions[i].rotation;

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.fieldOfView = Random.Range(fovMin, fovMax);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;

                var primitiveDebug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                primitiveDebug.transform.SetParent(cameraObject.transform);
                primitiveDebug.transform.localPosition = Vector3.zero;
                primitiveDebug.transform.localRotation = Quaternion.Euler(90, 0, 0);
                primitiveDebug.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                primitiveDebug.GetComponent<Renderer>().material.color = Color.red;
                primitiveDebug.name = "DebugCylinder";
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

                Vector3 position = fallCenterPosition.position + Quaternion.Euler(0, angle, 0) * new Vector3(distance, 0, 0);
                Quaternion rotation = Quaternion.LookRotation(fallCenterPosition.position - position);

                cameraPositions[i] = new CameraPosition(position, rotation);
            }

            return cameraPositions;
        }

    }

    public class CameraPosition {
        public Vector3 position;
        public Quaternion rotation;

        public CameraPosition(Vector3 pos, Quaternion rot) {
            position = pos;
            rotation = rot;
        }
    }
}
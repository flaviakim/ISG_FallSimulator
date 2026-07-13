using UnityEngine;

namespace VideoRecording {
    /// <summary>
    /// Immutable snapshot of a camera's position, rotation and field of view.
    /// </summary>
    public readonly struct CameraPositionInfo {
        public Vector3    Position { get; }
        public Quaternion Rotation { get; }
        public float      FOV      { get; }

        public CameraPositionInfo(Vector3 position, Quaternion rotation, float fov) {
            Position = position;
            Rotation = rotation;
            FOV      = fov;
        }
    }
}

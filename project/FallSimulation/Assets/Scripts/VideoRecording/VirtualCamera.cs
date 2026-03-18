using UnityEngine;

namespace VideoRecording {
    /// <summary>
    /// Pure C# virtual camera. Pre-computes a world-to-clip matrix and replicates
    /// <c>Camera.WorldToViewportPoint</c> without registering with Unity's rendering pipeline.
    /// Supports any number of simultaneous instances with no per-frame rendering cost.
    /// </summary>
    public sealed class VirtualCamera {
        private readonly Matrix4x4 _worldToClip;

        /// <summary>Pre-computes the view and projection matrices.</summary>
        /// <param name="position">World-space camera position.</param>
        /// <param name="rotation">World-space camera rotation.</param>
        /// <param name="fovDegrees">Vertical field of view in degrees.</param>
        /// <param name="aspectRatio">Width / height (e.g. 16f/9f).</param>
        /// <param name="nearClip">Near clip plane distance.</param>
        /// <param name="farClip">Far clip plane distance.</param>
        public VirtualCamera(Vector3 position, Quaternion rotation,
                             float fovDegrees, float aspectRatio,
                             float nearClip, float farClip) {
            Vector3 right = rotation * Vector3.right;
            Vector3 up    = rotation * Vector3.up;
            Vector3 fwd   = rotation * Vector3.forward;

            // Unity worldToCameraMatrix convention: +X right, +Y up, -Z forward (right-handed view space)
            var view = new Matrix4x4();
            view.SetRow(0, new Vector4( right.x,  right.y,  right.z, -Vector3.Dot(right, position)));
            view.SetRow(1, new Vector4( up.x,      up.y,     up.z,   -Vector3.Dot(up,    position)));
            view.SetRow(2, new Vector4(-fwd.x,    -fwd.y,   -fwd.z,   Vector3.Dot(fwd,   position)));
            view.SetRow(3, new Vector4( 0f,        0f,       0f,       1f));

            Matrix4x4 proj = Matrix4x4.Perspective(fovDegrees, aspectRatio, nearClip, farClip);
            _worldToClip = proj * view;
        }

        /// <summary>
        /// Converts a world-space position to viewport space [0,1]×[0,1].
        /// The z component is the eye-space depth (same as <c>Camera.WorldToViewportPoint</c>).
        /// </summary>
        public Vector3 WorldToViewportPoint(Vector3 worldPos) {
            Vector4 clip = _worldToClip * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f);
            float invW = 1f / clip.w;
            return new Vector3(
                (clip.x * invW + 1f) * 0.5f,
                (clip.y * invW + 1f) * 0.5f,
                clip.w);                          // eye-space depth, matches Unity convention
        }
    }
}

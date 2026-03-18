using System;
using System.Collections.Generic;
using UnityEngine;

namespace VideoRecording {
    /// <summary>
    /// Owns a <see cref="VirtualCamera"/> and an <see cref="IRecordingStrategy"/>.
    /// Optionally applies a Y-flip before handing data to the strategy so all
    /// strategies receive consistent, corrected viewport coordinates.
    /// The joints buffer is pre-allocated and reused every frame to eliminate GC pressure.
    /// </summary>
    public sealed class SingleCameraRecorder : IDisposable {
        /// <summary>The virtual (non-rendering) camera used for projection math.</summary>
        public VirtualCamera VirtualCamera { get; }

        private readonly IRecordingStrategy _strategy;
        private readonly int                _highestPoseID;
        private readonly bool               _flipY;
        private readonly Vector3?[]         _joints;   // pre-allocated; reused every frame
        private bool                        _disposed;

        public SingleCameraRecorder(VirtualCamera virtualCamera, IRecordingStrategy strategy,
                                    int highestPoseID, bool flipY = false) {
            VirtualCamera  = virtualCamera;
            _strategy      = strategy;
            _highestPoseID = highestPoseID;
            _flipY         = flipY;
            _joints        = new Vector3?[highestPoseID];
        }

        /// <summary>Opens the output resource (writes header / preamble).</summary>
        public void Open() => _strategy.Initialize();

        /// <summary>
        /// Samples all pose points, applies Y-flip, and appends one frame to the strategy.
        /// The internal joints buffer is cleared and reused each call; strategies must
        /// consume frame data within <see cref="IRecordingStrategy.AppendFrame"/> and
        /// must not retain a reference to <see cref="FrameData.Joints"/> across calls.
        /// </summary>
        public void CaptureFrame(int frameIndex, float time, IReadOnlyList<PosePoint> posePoints) {
            Array.Clear(_joints, 0, _joints.Length);

            int currIndex = 0;
            for (int i = 0; i < _highestPoseID; i++) {
                if (currIndex < posePoints.Count && posePoints[currIndex].PoseID == i) {
                    Vector3 vp = VirtualCamera.WorldToViewportPoint(posePoints[currIndex].GetPosition());
                    if (_flipY) vp.y = 1f - vp.y;
                    _joints[i] = vp;
                    currIndex++;
                }
            }

            _strategy.AppendFrame(new FrameData(frameIndex, time, _joints));
        }

        public void Dispose() {
            if (_disposed) return;
            _disposed = true;
            _strategy.Dispose();
        }
    }
}

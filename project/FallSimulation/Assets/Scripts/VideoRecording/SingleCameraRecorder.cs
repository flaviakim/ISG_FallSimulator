using System;
using System.Collections.Generic;
using UnityEngine;

namespace VideoRecording {
    /// <summary>
    /// Owns a <see cref="Camera"/> and an <see cref="IRecordingStrategy"/>.
    /// Applies Y-flip before handing data to the strategy so all strategies
    /// receive consistent, corrected viewport coordinates.
    /// </summary>
    public sealed class SingleCameraRecorder : IDisposable {
        public Camera Camera { get; }

        private readonly IRecordingStrategy _strategy;
        private readonly int                _highestPoseID;
        private bool                        _disposed;

        public SingleCameraRecorder(Camera camera, IRecordingStrategy strategy, int highestPoseID) {
            Camera         = camera;
            _strategy      = strategy;
            _highestPoseID = highestPoseID;
        }

        /// <summary>Opens the output resource (writes header / preamble).</summary>
        public void Open() => _strategy.Initialize();

        /// <summary>
        /// Samples all pose points, applies Y-flip, and appends one frame to the strategy.
        /// </summary>
        public void CaptureFrame(int frameIndex, float time, IReadOnlyList<PosePoint> posePoints) {
            var joints = new Vector3?[_highestPoseID];

            int currIndex = 0;
            for (int i = 0; i < _highestPoseID; i++) {
                if (currIndex < posePoints.Count && posePoints[currIndex].PoseID == i) {
                    Vector3 vp = Camera.WorldToViewportPoint(posePoints[currIndex].GetPosition());
                    vp.y = 1f - vp.y; // Y-flip
                    joints[i] = vp;
                    currIndex++;
                } else {
                    joints[i] = null;
                }
            }

            _strategy.AppendFrame(new FrameData(frameIndex, time, joints));
        }

        public void Dispose() {
            if (_disposed) return;
            _disposed = true;
            _strategy.Dispose();
        }
    }
}

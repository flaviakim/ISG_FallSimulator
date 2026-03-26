using System;
using System.Collections.Generic;
using UnityEngine;

namespace VideoRecording {
    public sealed class FullPoseInformationRecorder : ISingleCameraRecorder {
        private readonly IRecordingStrategy _strategy;
        private readonly int _numberOfPosePoints;
        private readonly Vector3?[] _joints;
        private bool _disposed;
        
        public FullPoseInformationRecorder(IRecordingStrategy strategy, int numberOfPosePoints) {
            _strategy = strategy;
            _numberOfPosePoints = numberOfPosePoints;
            _joints = new Vector3?[numberOfPosePoints];
        }
        
        public void Open() => _strategy.Initialize();
        
        public void CaptureFrame(int frameIndex, float time, IReadOnlyList<PosePoint> posePoints) {
            Array.Clear(_joints, 0, _joints.Length);
            
            for (int i = 0; i < posePoints.Count; i++) {
                var posePoint = posePoints[i];
                if (posePoint.PoseID < _numberOfPosePoints) {
                    _joints[posePoint.PoseID] = posePoint.GetPosition();
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
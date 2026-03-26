using System;
using System.Collections.Generic;

namespace VideoRecording {
    public interface ISingleCameraRecorder : IDisposable {
        /// <summary>Opens the output resource (writes header / preamble).</summary>
        void Open();
        
        /// <summary>
        /// Samples all pose points, applies Y-flip, and appends one frame to the strategy.
        /// The internal joints buffer is cleared and reused each call; strategies must
        /// consume frame data within <see cref="IRecordingStrategy.AppendFrame"/> and
        /// must not retain a reference to <see cref="FrameData.Joints"/> across calls.
        /// </summary>
        void CaptureFrame(int frameIndex, float time, IReadOnlyList<PosePoint> posePoints);
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace VideoRecording {
    /// <summary>
    /// Immutable frame snapshot. Joints are in Y-flipped viewport space.
    /// A null entry means the joint was absent/not tracked for that slot.
    /// </summary>
    public sealed class FrameData {
        /// <summary>Unity frame counter at capture time.</summary>
        public int FrameIndex { get; }

        /// <summary>Time.time at capture time.</summary>
        public float Time { get; }

        /// <summary>
        /// Length == highestPoseID. Each element is the viewport-space position
        /// (x, y already Y-flipped, z = depth) or null if joint is missing.
        /// </summary>
        public IReadOnlyList<Vector3?> Joints { get; }

        public FrameData(int frameIndex, float time, IReadOnlyList<Vector3?> joints) {
            FrameIndex = frameIndex;
            Time       = time;
            Joints     = joints;
        }
    }
}

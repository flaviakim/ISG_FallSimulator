using System;

namespace VideoRecording {
    /// <summary>
    /// Pluggable output strategy for pose recording.
    /// </summary>
    public interface IRecordingStrategy : IDisposable {
        /// <summary>Opens the output resource and writes any preamble (e.g. CSV header).</summary>
        void Initialize();

        /// <summary>Appends a single frame of pose data to the output.</summary>
        void AppendFrame(FrameData frame);

        /// <summary>Flushes and closes the output resource.</summary>
        void Finalize();
    }
}

using System.IO;
using System.Text;

namespace VideoRecording {
    /// <summary>
    /// Writes one CSV row per frame. Columns: Frame, Time, Pose_N_X, Pose_N_Y, Pose_N_Z ...
    /// Missing joints are represented as empty cells (,,).
    /// </summary>
    public sealed class CsvRecordingStrategy : IRecordingStrategy {
        private readonly string _filePath;
        private readonly int    _highestPoseID;
        private StreamWriter    _writer;
        private bool            _finalized;

        public CsvRecordingStrategy(string filePath, int highestPoseID) {
            _filePath      = filePath + ".csv";
            _highestPoseID = highestPoseID;
        }

        public void Initialize() {
            _writer = new StreamWriter(_filePath, append: false);
            var header = new StringBuilder("Frame,Time");
            for (int i = 0; i < _highestPoseID; i++) {
                header.Append($",Pose_{i}_X,Pose_{i}_Y,Pose_{i}_Z");
            }
            _writer.WriteLine(header.ToString());
        }

        public void AppendFrame(FrameData frame) {
            var line = new StringBuilder($"{frame.FrameIndex},{frame.Time:F5}");
            for (int i = 0; i < frame.Joints.Count; i++) {
                var joint = frame.Joints[i];
                if (joint.HasValue) {
                    line.Append($",{joint.Value.x:F5},{joint.Value.y:F5},{joint.Value.z:F5}");
                } else {
                    line.Append(",,,");
                }
            }
            _writer.WriteLine(line.ToString());
        }

        public void Finalize() {
            if (_finalized) return;
            _finalized = true;
            _writer?.Flush();
            _writer?.Close();
        }

        public void Dispose() => Finalize();
    }
}

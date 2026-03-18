using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VideoRecording {
    /// <summary>
    /// Buffers all frames in memory, then serialises the full JSON array on Finalize().
    /// Output matches the schema in Format.md.
    /// </summary>
    public sealed class JsonRecordingStrategy : IRecordingStrategy {
        // ---- private DTOs -------------------------------------------------

        private sealed class SkeletonEntry {
            [JsonProperty("pose")]  public float[] Pose  { get; set; }
            [JsonProperty("score")] public float[] Score { get; set; }
        }

        private sealed class FrameEntry {
            [JsonProperty("frame_index")]            public int            FrameIndex           { get; set; }
            [JsonProperty("time")]                   public float          Time                 { get; set; }
            [JsonProperty("label")]                  public float          Label                { get; set; }
            [JsonProperty("label_str")]              public string         LabelStr             { get; set; }
            [JsonProperty("camera_height_position")] public string         CameraHeightPosition { get; set; }
            [JsonProperty("skeleton")]               public SkeletonEntry[] Skeleton            { get; set; }
        }

        // ---- fields -------------------------------------------------------

        private readonly string _filePath;
        private readonly float  _label;
        private readonly string _labelStr;
        private readonly string _cameraHeightPosition;
        private readonly List<FrameEntry> _frames = new();
        private bool _finalized;

        public JsonRecordingStrategy(string filePath, float label, string labelStr, string cameraHeightPosition) {
            _filePath             = filePath;
            _label                = label;
            _labelStr             = labelStr;
            _cameraHeightPosition = cameraHeightPosition;
        }

        public void Initialize() {
            // Nothing to open yet — we buffer in memory.
        }

        public void AppendFrame(FrameData frame) {
            int jointCount = frame.Joints.Count;
            var pose  = new float[jointCount * 2];
            var score = new float[jointCount];

            for (int i = 0; i < jointCount; i++) {
                var joint = frame.Joints[i];
                if (joint.HasValue) {
                    pose[i * 2]     = joint.Value.x;
                    pose[i * 2 + 1] = joint.Value.y;
                    score[i]        = 1f;
                } else {
                    pose[i * 2]     = 0f;
                    pose[i * 2 + 1] = 0f;
                    score[i]        = 0f;
                }
            }

            _frames.Add(new FrameEntry {
                FrameIndex           = frame.FrameIndex,
                Time                 = frame.Time,
                Label                = _label,
                LabelStr             = _labelStr,
                CameraHeightPosition = _cameraHeightPosition,
                Skeleton             = new[] {
                    new SkeletonEntry { Pose = pose, Score = score }
                }
            });
        }

        public void Finalize() {
            if (_finalized) return;
            _finalized = true;

            string json = JsonConvert.SerializeObject(_frames, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public void Dispose() => Finalize();
    }
}

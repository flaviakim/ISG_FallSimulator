using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VideoRecording {
    public class Full3DRecordingStrategy : IRecordingStrategy {
        private sealed class SkeletonEntry {
            [JsonProperty("pose_3d")]  public float[] Pose  { get; set; }
            [JsonProperty("score")] public float[] Score { get; set; }
        }
        
        private sealed class FrameEntry {
            [JsonProperty("frame_index")]            public int            FrameIndex           { get; set; }
            [JsonProperty("time")]                   public float          Time                 { get; set; }
            [JsonProperty("label")]                  public float          Label                { get; set; }
            [JsonProperty("label_str")]              public string         LabelStr             { get; set; }
            [JsonProperty("skeleton")]               public SkeletonEntry[] Skeleton            { get; set; }
        }
        
        private readonly string _filePath;
        private readonly List<FrameEntry> _frames = new();
        private bool _finalized;
        
        public Full3DRecordingStrategy(string filePath) {
            _filePath      = filePath + ".json";
        }
        
        public void Dispose() {
            Finalize();
        }
        public void Initialize() {
            // Nothing to open yet — we buffer in memory.
        }
        public void AppendFrame(FrameData frame) {
            int jointCount = frame.Joints.Count;
            var pose  = new float[jointCount * 3];
            var score = new float[jointCount];
            
            for (int i = 0; i < jointCount; i++) {
                var joint = frame.Joints[i];
                if (joint.HasValue) {
                    pose[i * 3]       = joint.Value.x;
                    pose[(i * 3) + 1] = joint.Value.y;
                    pose[(i * 3) + 2] = joint.Value.z;
                    score[i]          = 1f;
                } else {
                    pose[i * 3]       = 0f;
                    pose[(i * 3) + 1] = 0f;
                    pose[(i * 3) + 2] = 0f;
                    score[i]          = 0f;
                }
            }
            
            _frames.Add(new FrameEntry {
                FrameIndex           = frame.FrameIndex,
                Time                 = frame.Time,
                Label                = GameManager.Instance.Status.GetIDForStatus(),
                LabelStr             = GameManager.Instance.Status.GetLabelStrForStatus(),
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
    }
}
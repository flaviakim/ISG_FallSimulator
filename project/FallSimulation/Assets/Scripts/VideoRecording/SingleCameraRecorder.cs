using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VideoRecording {
    public class SingleCameraRecorder {
        private readonly int _highestPoseID;
        private readonly string _filePath;
        private readonly Camera _camera;
        private readonly StreamWriter _fileStream;
        
        public SingleCameraRecorder(int index, string outputFolder, DateTime startTime, Camera camera, int highestPoseID) {
            _camera = camera;
            _highestPoseID = highestPoseID;
            _filePath = Path.Combine(outputFolder, $"fall-recording_{startTime:yyyy-MM-dd_HH-mm-ss}_{index}.csv");
            _fileStream = File.AppendText(_filePath);
            InitializeFile();
        }
        
        private void InitializeFile() {
            var header = "Frame,Time";
            for (int i = 0; i < _highestPoseID; i++) {
                header += $",Pose_{i}_X,Pose_{i}_Y,Pose_{i}_Z";
            }
            
            _fileStream.WriteLine(header);
        }
        
        public void AppendFrameData(int frameNumber, float timeStamp, List<PosePoint> posePoints) {
            var line = $"{frameNumber},{timeStamp:F5}";

            var currIndex = 0;
            for (int i = 0; i < _highestPoseID; i++) {
                if (currIndex < posePoints.Count && posePoints[currIndex].PoseID == i) {
                    Vector3 pos = _camera.WorldToViewportPoint(posePoints[currIndex].GetPosition());
                    line += $",{pos.x:F5},{pos.y:F5},{pos.z:F5}";
                    currIndex++;
                } else {
                    line += ",,,";
                }
            }

            _fileStream.WriteLine(line);
        }
        
        public void Close() {
            _fileStream.Close();
        }
    }
}
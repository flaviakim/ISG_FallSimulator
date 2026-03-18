namespace VideoRecording {
    /// <summary>
    /// Classifies camera height relative to a subject.
    /// </summary>
    public static class CameraHeightClassifier {
        public const string ChestShoulderLevel = "chest_shoulder_level";
        public const string AboveHeadDown = "above_head_down";

        /// <summary>
        /// Returns a height label based on whether the camera is above the subject's head.
        /// </summary>
        /// <param name="cameraY">World-space Y of the camera.</param>
        /// <param name="subjectY">World-space Y of the subject's reference point.</param>
        /// <param name="aboveHeadThreshold">Height delta above which the camera is considered overhead.</param>
        public static string Classify(float cameraY, float subjectY, float aboveHeadThreshold) {
            return (cameraY - subjectY) >= aboveHeadThreshold ? AboveHeadDown : ChestShoulderLevel;
        }
    }
}

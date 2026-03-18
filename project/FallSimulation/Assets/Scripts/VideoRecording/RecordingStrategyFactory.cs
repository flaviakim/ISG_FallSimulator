namespace VideoRecording {
    /// <summary>
    /// Creates the appropriate <see cref="IRecordingStrategy"/> and appends the correct file extension.
    /// </summary>
    public static class RecordingStrategyFactory {
        /// <summary>
        /// Creates a recording strategy for the given mode.
        /// The correct extension (.csv or .json) is appended to <paramref name="filePath"/>.
        /// </summary>
        public static IRecordingStrategy Create(
            RecordingMode mode,
            string        filePath,
            int           highestPoseID,
            float         label                = 0f,
            string        labelStr             = "",
            string        cameraHeightPosition = "") {

            switch (mode) {
                case RecordingMode.Csv:
                    return new CsvRecordingStrategy(filePath + ".csv", highestPoseID);

                case RecordingMode.Json:
                    return new JsonRecordingStrategy(filePath + ".json", label, labelStr, cameraHeightPosition);

                default:
                    throw new System.ArgumentOutOfRangeException(nameof(mode), mode, "Unknown recording mode.");
            }
        }
    }
}

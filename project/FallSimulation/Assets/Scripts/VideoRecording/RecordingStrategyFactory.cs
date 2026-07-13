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
            string        cameraHeightPosition = "") {
            
            return mode switch {
                RecordingMode.Csv => new CsvRecordingStrategy(filePath, highestPoseID),
                RecordingMode.Json => new JsonRecordingStrategy(filePath, cameraHeightPosition),
                RecordingMode.Full3DCSV => new Full3DRecordingStrategy(filePath),
                _ => throw new System.ArgumentOutOfRangeException(nameof(mode), mode, "Unknown recording mode.")
            };
        }
    }
}

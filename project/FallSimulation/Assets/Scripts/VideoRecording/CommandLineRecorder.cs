using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.Recorder;
using UnityEngine;

public class CommandLineRecorder : MonoBehaviour {
    // The RecorderController starts and stops the recording.
    private RecorderController controller;

    // The first frame to record.
    [SerializeField] private float startTime = 0;

    // The last frame to record.
    [SerializeField] private float endTime = 10;

    // The path to the Recorder Settings preset file to use for the recording.
    [SerializeField] private string presetPath;

    static RecorderSettings LoadRecorderSettingsFromPreset(string presetPath) {
        // Load the Preset from the provided path.
        var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);

        // Use reflection to determine the type of the RecorderSettings to use
        // (for example a MovieRecorderSettings).
        var recorderSettingsTypes = TypeCache.GetTypesDerivedFrom<RecorderSettings>().ToList();

        var recorderSettingsType = recorderSettingsTypes.SingleOrDefault(t => t.Name == preset.GetTargetTypeName());

        if (recorderSettingsType == null) {
            Debug.Log("Preset must be a subclass of RecorderSettings");
            return null;
        }

        // Create a new RecorderSettings instance and apply the Preset to it.
        RecorderSettings outSettings = (RecorderSettings)ScriptableObject.CreateInstance(recorderSettingsType);

        preset.ApplyTo(outSettings);
        outSettings.name = preset.name;

        return outSettings;
    }

    void StartRecording(string presetPath, float startTime, float endTime) {
        // Create RecorderSettings from the provided Preset path.
        RecorderSettings recorderSettings = LoadRecorderSettingsFromPreset(presetPath);
        recorderSettings.FrameRate = 30;

        // Create a new RecorderControllerSettings to set the start and end frame for
        // the recording session and add the RecorderSettings to it.
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        controllerSettings.AddRecorderSettings(recorderSettings);
        controllerSettings.SetRecordModeToTimeInterval(startTime, endTime);

        // Create and setup a new RecorderController and start the recording.
        controller = new RecorderController(controllerSettings);
        controller.PrepareRecording();
        controller.StartRecording();
    }

    private void OnEnable() {
        // This is called once when Unity enters PlayMode.
        StartRecording(presetPath, startTime, endTime);
    }

    private void Update() {
        // This is called on every frame when Unity is in PlayMode.
        if (controller != null && !controller.IsRecording()) {
            // When the RecorderController has no more frame to record, stop
            // the recording and exit the PlayMode.
            controller.StopRecording();
            EditorApplication.ExitPlaymode();
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager instance;
    private JObject allSettings;

    void Awake()
    {
        instance = this;
        LoadSettings();
    }

    void LoadSettings()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "gameSettings.json");
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            allSettings = JObject.Parse(jsonContent);
            Debug.Log("Settings loaded successfully");
        }
        else
        {
            Debug.LogError("Settings file not found!");
        }
    }

    public JObject GetSettings(string stageName, string playerCount, string difficulty)
    {
        if (allSettings != null &&
            allSettings.TryGetValue(stageName, out JToken stageSettings) &&
            stageSettings[playerCount] != null &&
            stageSettings[playerCount][difficulty] != null)
        {
            return (JObject)stageSettings[playerCount][difficulty];
        }

        Debug.LogWarning($"Settings not found for stage: {stageName}, playerCount: {playerCount}, difficulty: {difficulty}");
        return null;
    }

    public float GetSettingValue(string stageName, string playerCount, string difficulty, string settingName)
    {
        JObject settings = GetSettings(stageName, playerCount, difficulty);
        if (settings != null && settings.TryGetValue(settingName, out JToken value))
        {
            return value.Value<float>();
        }

        Debug.LogWarning($"Setting {settingName} not found for stage: {stageName}, playerCount: {playerCount}, difficulty: {difficulty}");
        return 0f;
    }
}
using System.IO;
using UnityEngine;

public static class GameSettingManager
{
    private static string fileName = "ChzzWithViewer.txt";

    private static string filePath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "..", fileName);
#else
            return Path.Combine(Application.dataPath, fileName);
#endif
        }
    }

    public static GameSettingData Load()
    {
        if (!File.Exists(filePath))
        {
            GameSettingData defaultData = new GameSettingData();
            Save(defaultData);
            return defaultData;
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<GameSettingData>(json);
    }

    public static void Save(GameSettingData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }
}
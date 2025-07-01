using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class GameSettingManager
{
    private static string fileName = "ChzzWithViewer.txt";
    private static GameSettingData settingData; 
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

    public static void Load()
    {
        if (!File.Exists(filePath))
        {
            GameSettingData defaultData = new GameSettingData();
            Save(defaultData);
            return;
        }

        string json = File.ReadAllText(filePath);
        settingData = JsonConvert.DeserializeObject<GameSettingData>(json);
    }

    public static void Save(GameSettingData data)
    {
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(filePath, json);
    }

    public static HashSet<string> GetBannedWorlds()
    {
        return settingData.bannedWords;
    }

    public static void AddBannedWord(string word)
    {
        settingData.bannedWords.Add(word);
        Save(settingData);
    }

    public static void RemoveBannedWord(string word)
    {
        settingData.bannedWords.Remove(word);
        Save(settingData);
    }

    public static bool ContainBannedWord(string word)
    {
        foreach (var bannedWord in settingData.defaultBannedWords)
        {
            if (word.Contains(bannedWord))
            {
                return true;
            }
        }
        
        foreach (var bannedWord in settingData.bannedWords)
        {
            if (word.Contains(bannedWord))
            {
                return true;
            }
        }

        return true;
    }
}
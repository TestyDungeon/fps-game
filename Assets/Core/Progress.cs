using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Progress
{
    [Serializable] class Data { public List<string> completed = new List<string>(); }

    static Data data;
    static string FilePath => Path.Combine(Application.persistentDataPath, "progress.json");

    static void Load()
    {
        if (data != null) return;
        data = File.Exists(FilePath) ? JsonUtility.FromJson<Data>(File.ReadAllText(FilePath)) : new Data();
    }

    public static bool IsCompleted(LevelDefinition level)
    {
        Load();
        return data.completed.Contains(level.sceneName);
    }

    public static void MarkCompleted(LevelDefinition level)
    {
        Load();
        if (data.completed.Contains(level.sceneName)) return;
        data.completed.Add(level.sceneName);
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }

    // Level 0 is always open. Every other level opens when the previous one is completed.
    public static bool IsUnlocked(LevelDatabase db, LevelDefinition level)
    {
        if (db.unlockAll) return true;
        int i = Array.IndexOf(db.levels, level);
        return i <= 0 || IsCompleted(db.levels[i - 1]);
    }

    public static void Reset()
    {
        data = new Data();
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }
}
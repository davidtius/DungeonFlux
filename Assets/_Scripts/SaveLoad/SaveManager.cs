using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string filename = "player_save.json";

    private static string path => Path.Combine(Application.persistentDataPath, filename);

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        Debug.Log($"Game Saved to: {path}");
    }

    public static SaveData Load()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }
        else
        {
            Debug.Log("Save file not found. Creating new data.");
            return new SaveData();
        }
    }

    public static bool HasSaveFile()
    {
        return File.Exists(path);
    }

    public static void DeleteSave()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save file deleted.");
        }
    }
}
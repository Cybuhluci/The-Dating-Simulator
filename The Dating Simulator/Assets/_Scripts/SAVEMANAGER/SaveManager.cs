using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static string SkillsPath => Application.persistentDataPath + "/Save/SkillsFile.json";
    public static List<SkillEntry> CurrentSkills = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LoadSkills();
    }

    // Save skills (individual skills' unlock states)
    public static void SaveSkills()
    {
        CreateSaveDirectoryIfNeeded();

        var data = new SkillSaveData { unlockedSkills = CurrentSkills };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SkillsPath, json);
        Debug.Log("Skills Saved");
    }

    // Load skills (individual skills' unlock states)
    public static void LoadSkills()
    {
        CreateSaveDirectoryIfNeeded();

        if (!File.Exists(SkillsPath))
        {
            Debug.Log("No skill save found. Starting fresh.");
            CurrentSkills = new List<SkillEntry>();
            return;
        }

        string json = File.ReadAllText(SkillsPath);
        SkillSaveData data = JsonUtility.FromJson<SkillSaveData>(json);
        CurrentSkills = data.unlockedSkills ?? new List<SkillEntry>();
        Debug.Log($"Skills Loaded: {CurrentSkills.Count} entries");
    }

    // Reset skills (clear saved data)
    public static void ResetSkills()
    {
        CurrentSkills.Clear();
        SaveSkills();
        Debug.Log("Skills Reset");
    }

    private static void CreateSaveDirectoryIfNeeded()
    {
        string dir = Application.persistentDataPath + "/Save";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            Debug.Log("Save directory created");
        }
    }
}

[System.Serializable]
public class SkillEntry
{
    public string skillID;  // The ID of the skill (from the Skill class)
    public bool isUnlocked; // Whether the skill is unlocked or not
}

[System.Serializable]
public class SkillSaveData
{
    public List<SkillEntry> unlockedSkills = new();
}

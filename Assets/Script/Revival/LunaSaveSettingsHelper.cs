using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>Game Framework settings backed by an atomic, recoverable local save.</summary>
public sealed class LunaSaveSettingsHelper : SettingHelperBase
{
    private DefaultSetting settings = new DefaultSetting();
    public string FilePath { get; private set; }
    public override int Count => settings.Count;

    public void Configure(string directory)
    {
        FilePath = Path.Combine(directory, "LunaSave.dat");
    }

    public override bool Load()
    {
        if (!File.Exists(FilePath) && !File.Exists(FilePath + ".bak")) return true;
        if (TryLoad(FilePath)) return true;
        if (TryLoad(FilePath + ".bak"))
        {
            Debug.LogWarning("Recovered Luna progress from the backup save.");
            return true;
        }
        Debug.LogWarning("Luna save could not be read. Existing files have been preserved.");
        return false;
    }

    private bool TryLoad(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            var candidate = new DefaultSetting();
            using (var stream = File.OpenRead(path)) candidate.Deserialize(stream);
            settings = candidate;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not read Luna save: " + exception.Message);
            return false;
        }
    }

    public override bool Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            var temporary = FilePath + ".tmp";
            using (var stream = File.Create(temporary)) settings.Serialize(stream);
            if (File.Exists(FilePath)) File.Replace(temporary, FilePath, FilePath + ".bak");
            else File.Move(temporary, FilePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not save Luna progress: " + exception.Message);
            return false;
        }
    }

    public override string[] GetAllSettingNames() => settings.GetAllSettingNames();
    public override void GetAllSettingNames(List<string> results) => settings.GetAllSettingNames(results);
    public override bool HasSetting(string name) => settings.HasSetting(name);
    public override bool RemoveSetting(string name) => settings.RemoveSetting(name);
    public override void RemoveAllSettings() => settings.RemoveAllSettings();
    public override bool GetBool(string name) => settings.GetBool(name);
    public override bool GetBool(string name, bool fallback) => settings.GetBool(name, fallback);
    public override void SetBool(string name, bool value) => settings.SetBool(name, value);
    public override int GetInt(string name) => settings.GetInt(name);
    public override int GetInt(string name, int fallback) => settings.GetInt(name, fallback);
    public override void SetInt(string name, int value) => settings.SetInt(name, value);
    public override float GetFloat(string name) => settings.GetFloat(name);
    public override float GetFloat(string name, float fallback) => settings.GetFloat(name, fallback);
    public override void SetFloat(string name, float value) => settings.SetFloat(name, value);
    public override string GetString(string name) => settings.GetString(name);
    public override string GetString(string name, string fallback) => settings.GetString(name, fallback);
    public override void SetString(string name, string value) => settings.SetString(name, value);
    public override T GetObject<T>(string name) => JsonUtility.FromJson<T>(GetString(name));
    public override object GetObject(Type type, string name) => JsonUtility.FromJson(GetString(name), type);
    public override T GetObject<T>(string name, T fallback) => HasSetting(name) ? GetObject<T>(name) : fallback;
    public override object GetObject(Type type, string name, object fallback) => HasSetting(name) ? GetObject(type, name) : fallback;
    public override void SetObject<T>(string name, T value) => SetString(name, JsonUtility.ToJson(value));
    public override void SetObject(string name, object value) => SetString(name, JsonUtility.ToJson(value));
}

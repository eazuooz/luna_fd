using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>Game Framework settings backed by an atomic, recoverable local save.</summary>
public sealed class LunaSaveSettingsHelper : SettingHelperBase
{
    private DefaultSetting settings = new DefaultSetting();
    private readonly object saveGate = new object();
    private volatile int lastSaveAttemptCount;
    private long saveAttemptSerial;
    public const int MaxSaveAttempts = 6;
    public const int RetryDelayMilliseconds = 25;
    public int LastSaveAttemptCount => lastSaveAttemptCount;
    public long SaveAttemptSerial => Interlocked.Read(ref saveAttemptSerial);
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
        lock (saveGate)
        {
            lastSaveAttemptCount = 0;
            string temporary = FilePath + ".tmp." + Guid.NewGuid().ToString("N");
            string backupTemporary = temporary + ".backup";
            string operation = "serialize";
            byte[] pending, previous = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    settings.Serialize(stream);
                    pending = stream.ToArray();
                }
            }
            catch (Exception exception)
            {
                ReportSaveFailure(exception, operation, temporary, backupTemporary);
                return false;
            }

            bool prepared = false, backupPublished = false;
            for (int attempt = 1; attempt <= MaxSaveAttempts; attempt++)
            {
                lastSaveAttemptCount = attempt;
                Interlocked.Increment(ref saveAttemptSerial);
                try
                {
                    if (!prepared)
                    {
                        operation = "prepare";
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                        if (File.Exists(FilePath))
                        {
                            byte[] candidate = File.ReadAllBytes(FilePath);
                            // A recovered corrupt primary must never replace the last good backup.
                            if (IsReadableSetting(candidate)) previous = candidate;
                        }
                        WriteFlushed(temporary, pending);
                        if (previous != null) WriteFlushed(backupTemporary, previous);
                        prepared = true;
                    }
                    if (previous != null && !backupPublished)
                    {
                        operation = "publish backup";
                        PublishAtomically(backupTemporary, FilePath + ".bak", previous);
                        backupPublished = true;
                    }
                    operation = "publish primary";
                    PublishAtomically(temporary, FilePath, pending);
                    if (attempt > 1) Debug.Log("Luna progress saved after " + attempt + " atomic save attempts.");
                    return true;
                }
                catch (IOException exception)
                {
                    // Sharing/rename failures can be transient. Keep a complete candidate;
                    // never fall back to deleting or overwriting the destination in place.
                    if (attempt == MaxSaveAttempts)
                    {
                        ReportSaveFailure(exception, operation, temporary, backupTemporary);
                        return false;
                    }
                    Thread.Sleep(RetryDelayMilliseconds);
                }
                catch (Exception exception)
                {
                    ReportSaveFailure(exception, operation, temporary, backupTemporary);
                    return false;
                }
            }
            return false;
        }
    }

    private static bool IsReadableSetting(byte[] bytes)
    {
        try
        {
            using (var stream = new MemoryStream(bytes, false)) new DefaultSetting().Deserialize(stream);
            return true;
        }
        catch (InvalidDataException) { return false; }
        catch (IOException) { return false; }
        catch (ArgumentException) { return false; }
        catch (FormatException) { return false; }
    }

    private static void WriteFlushed(string path, byte[] bytes)
    {
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
        }
    }

    private static void PublishAtomically(string temporary, string destination, byte[] expected)
    {
        if (!File.Exists(temporary))
        {
            // Some replace failures can occur after a rename. Confirm the full commit
            // before treating a missing source as success or recreating its candidate.
            if (File.Exists(destination) && SameBytes(File.ReadAllBytes(destination), expected)) return;
            WriteFlushed(temporary, expected);
        }
        if (File.Exists(destination)) File.Replace(temporary, destination, null);
        else File.Move(temporary, destination);
    }

    private static bool SameBytes(byte[] left, byte[] right)
    {
        if (left.Length != right.Length) return false;
        for (int index = 0; index < left.Length; index++) if (left[index] != right[index]) return false;
        return true;
    }

    private void ReportSaveFailure(Exception exception, string operation, string temporary, string backupTemporary)
    {
        Debug.LogWarning("Could not save Luna progress: " + operation + " failed after " + lastSaveAttemptCount +
            " attempt(s); " + exception.GetType().Name + " HResult=0x" + exception.HResult.ToString("X8") +
            "; path=" + FilePath + ". " + exception.Message +
            ". Existing complete saves and any temporary recovery files were retained: " + temporary + ", " + backupTemporary);
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

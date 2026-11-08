using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Batch-only bridge to the development runtime matrix.</summary>
[InitializeOnLoad]
public static class LunaContentMatrix
{
    private const string ActiveKey = "LunaContentMatrix.Active";
    private const string StartedKey = "LunaContentMatrix.Started";
    private const string FailedKey = "LunaContentMatrix.Failed";
    private const string LeavingKey = "LunaContentMatrix.Leaving";
    private static readonly string ReportPath = Path.GetFullPath("Logs/Revival/content-matrix.json");
    [Serializable] private sealed class Summary { public string startedUtc = "", finishedUtc = ""; public bool success = false; }

    static LunaContentMatrix() { if (SessionState.GetBool(ActiveKey, false)) Subscribe(); }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("ContentMatrix requires a dedicated batch process.");
        string save = Path.GetFullPath("Logs/Revival/MatrixSave/" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(save);
        File.WriteAllText(Path.Combine(save, ".luna-validation-isolated"), "Dedicated validation save; never a user's progress.");
        Environment.SetEnvironmentVariable("LUNA_SAVE_DIRECTORY", save);
        Environment.SetEnvironmentVariable("LUNA_VALIDATION_DIRECTORY", Path.GetDirectoryName(ReportPath));
        Environment.SetEnvironmentVariable("LUNA_VALIDATION_MODE", "content-matrix");
        SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O"));
        SessionState.SetBool(ActiveKey, true); SessionState.SetBool(FailedKey, false); SessionState.SetBool(LeavingKey, false);
        Subscribe();
        EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    private static void Subscribe()
    {
        EditorApplication.update -= Tick; EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged -= OnPlayState; EditorApplication.playModeStateChanged += OnPlayState;
        Application.logMessageReceived -= OnLog; Application.logMessageReceived += OnLog;
    }
    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) SessionState.SetBool(FailedKey, true);
    }
    private static Summary ReadFreshReport()
    {
        if (!File.Exists(ReportPath)) return null;
        Summary report;
        try { report = JsonUtility.FromJson<Summary>(File.ReadAllText(ReportPath)); }
        catch (IOException) { return null; }
        catch (ArgumentException) { return null; }
        if (report == null || string.IsNullOrEmpty(report.startedUtc)) return null;
        DateTime started = DateTime.Parse(SessionState.GetString(StartedKey, ""), null, System.Globalization.DateTimeStyles.RoundtripKind);
        return DateTime.Parse(report.startedUtc, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime() >= started.AddSeconds(-2) ? report : null;
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(ActiveKey, false) || SessionState.GetBool(LeavingKey, false)) return;
        DateTime started = DateTime.Parse(SessionState.GetString(StartedKey, ""), null, System.Globalization.DateTimeStyles.RoundtripKind);
        if (DateTime.UtcNow - started > TimeSpan.FromMinutes(120))
        {
            SessionState.SetBool(FailedKey, true); Debug.LogError("Content matrix exceeded the 120-minute hard deadline.");
            LeavePlay(); return;
        }
        Summary report = ReadFreshReport();
        if (report != null && !string.IsNullOrEmpty(report.finishedUtc)) LeavePlay();
    }
    private static void LeavePlay()
    {
        SessionState.SetBool(LeavingKey, true);
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); else Finish();
    }
    private static void OnPlayState(PlayModeStateChange state)
    {
        if (SessionState.GetBool(ActiveKey, false) && state == PlayModeStateChange.EnteredEditMode) Finish();
    }
    private static void Finish()
    {
        Summary report = ReadFreshReport();
        bool passed = report != null && !string.IsNullOrEmpty(report.finishedUtc) && report.success && !SessionState.GetBool(FailedKey, false);
        SessionState.SetBool(ActiveKey, false);
        Environment.SetEnvironmentVariable("LUNA_VALIDATION_MODE", null);
        EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayState;
        Application.logMessageReceived -= OnLog;
        EditorApplication.Exit(passed ? 0 : 1);
    }
}

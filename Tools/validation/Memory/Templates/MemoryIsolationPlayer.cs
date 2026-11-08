using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Collections;
using Unity.Profiling.Memory;
using UnityEngine;
using UnityEngine.Profiling;

public sealed class MemoryIsolationPlayer : MonoBehaviour
{
    [Serializable]
    private sealed class Report
    {
        public string unityVersion, startedUtc, finishedUtc, graphicsDevice, snapshotPath;
        public string[] commandLine;
        public bool completed, developmentBuild, snapshotRequested, snapshotCompleted;
        public double requestedSeconds, actualSeconds;
        public long initialAllocatedBytes, finalAllocatedBytes, initialMonoUsedBytes, finalMonoUsedBytes;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    private Report report;
    private string outputDirectory;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        // This file is only copied to the standalone, marked experiment project.
        // It is not part of the Luna Assets tree or its player.
        if (Application.isEditor) return;
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        var host = new GameObject("Minimal memory experiment");
        DontDestroyOnLoad(host);
        host.AddComponent<MemoryIsolationPlayer>();
    }

    private IEnumerator Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        outputDirectory = Value(args, "-memory-isolation-directory");
        if (string.IsNullOrWhiteSpace(outputDirectory) || !Path.IsPathRooted(outputDirectory))
        {
            Debug.LogError("An absolute memory isolation directory is required.");
            Application.Quit(2); yield break;
        }
        Directory.CreateDirectory(outputDirectory);
        double seconds;
        if (!double.TryParse(Value(args, "-memory-isolation-seconds"), NumberStyles.Float, CultureInfo.InvariantCulture, out seconds)) seconds = 3d;
        seconds = Math.Max(1d, Math.Min(300d, seconds));
        report = new Report
        {
            unityVersion = Application.unityVersion, startedUtc = DateTime.UtcNow.ToString("o"), commandLine = args,
            graphicsDevice = SystemInfo.graphicsDeviceType.ToString(), developmentBuild = Debug.isDebugBuild,
            requestedSeconds = seconds, snapshotRequested = Array.IndexOf(args, "-memory-isolation-snapshot") >= 0,
            initialAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong(), initialMonoUsedBytes = Profiler.GetMonoUsedSizeLong()
        };
        Application.logMessageReceived += CaptureLog;
        double start = Time.realtimeSinceStartupAsDouble;
        while (Time.realtimeSinceStartupAsDouble - start < seconds) yield return null;
        if (report.snapshotRequested)
        {
            report.snapshotPath = Path.Combine(outputDirectory, "before-quit.snap");
            bool finished = false;
            MemoryProfiler.TakeSnapshot(report.snapshotPath, (path, success) => { report.snapshotCompleted = success; finished = true; },
                CaptureFlags.ManagedObjects | CaptureFlags.NativeObjects | CaptureFlags.NativeAllocations);
            double deadline = Time.realtimeSinceStartupAsDouble + 30d;
            while (!finished && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!finished || !report.snapshotCompleted) report.errors.Add("Requested memory snapshot did not complete successfully.");
        }
        report.actualSeconds = Time.realtimeSinceStartupAsDouble - start;
        report.finalAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong();
        report.finalMonoUsedBytes = Profiler.GetMonoUsedSizeLong();
        report.completed = report.errors.Count == 0;
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        File.WriteAllText(Path.Combine(outputDirectory, "operation.json"), JsonUtility.ToJson(report, true));
        Debug.Log("Minimal player finished; raw process shutdown diagnostics remain authoritative.");
        Application.Quit(report.completed ? 0 : 1);
    }

    private void CaptureLog(string message, string stack, LogType type)
    {
        if (type == LogType.Warning) report.warnings.Add(message);
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack);
    }

    private static string Value(string[] args, string key)
    {
        int index = Array.IndexOf(args, key);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}

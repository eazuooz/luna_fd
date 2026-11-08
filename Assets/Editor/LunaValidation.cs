using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Luna.Gameplay;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Native architecture audit, opt-in runtime checks and reproducible Windows builds.</summary>
[InitializeOnLoad]
public static class LunaValidation
{
    private const string StateKey = "LunaNativeEditorValidation.State";
    private const string ReportKey = "LunaNativeEditorValidation.Report";
    private const string RuntimePathKey = "LunaNativeEditorValidation.RuntimePath";
    private const string StartedKey = "LunaNativeEditorValidation.Started";
    private const string CoreHash = "b42d5cd6343c8a5f9ff0d4a5d9219fadf34b5068cad66d75387052c6e0fe943b";
    private static readonly string Output = Path.GetFullPath("Logs/Revival");
    private static readonly Regex GuidReference = new Regex(@"guid:\s*([a-fA-F0-9]{32})", RegexOptions.Compiled);
    private static Report report;
    private static int idleUpdates;
    private static double drainStart;
    private static bool completing;
    [Serializable] private sealed class Report
    {
        public string operation, startedUtc, finishedUtc, unityVersion;
        public bool success;
        public int sceneCount, templateSceneCount, prefabCount, dataAssetCount, runtimeFrames;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> expectedWarnings = new List<string>();
        public List<string> observations = new List<string>();
        public List<string> injectedDiagnostics = new List<string>();
    }
    [Serializable] private sealed class RuntimeReport
    {
        public string startedUtc = "", finishedUtc = "";
        public bool success = false;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> expectedWarnings = new List<string>();
        public List<string> expectedDiagnostics = new List<string>();
    }
    static LunaValidation()
    {
        if (string.IsNullOrEmpty(SessionState.GetString(StateKey, ""))) return;
        report = JsonUtility.FromJson<Report>(SessionState.GetString(ReportKey, "{}"));
        drainStart = EditorApplication.timeSinceStartup;
        Subscribe();
    }
    private static void Begin(string operation)
    {
        if (!string.IsNullOrEmpty(SessionState.GetString(StateKey, ""))) throw new InvalidOperationException("A Luna editor validation is already running.");
        Directory.CreateDirectory(Output);
        report = new Report { operation = operation, startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion };
        completing = false; SessionState.SetString(StartedKey, report.startedUtc); Subscribe(); Persist();
    }
    private static void Subscribe()
    {
        Application.logMessageReceived -= CaptureLog; Application.logMessageReceived += CaptureLog;
        EditorApplication.update -= Tick; EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged -= OnPlayState; EditorApplication.playModeStateChanged += OnPlayState;
    }
    public static void Audit()
    {
        Begin("audit");
        var priorScenes = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var buildGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string logSymbols = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildGroup));
            var symbols = new HashSet<string>(logSymbols.Split(';'));
            bool commonLogs = symbols.Overlaps(new[] { "ENABLE_LOG", "ENABLE_DEBUG_AND_ABOVE_LOG", "ENABLE_INFO_AND_ABOVE_LOG", "ENABLE_WARNING_AND_ABOVE_LOG" });
            Require(commonLogs || (symbols.Contains("ENABLE_WARNING_LOG") && (symbols.Contains("ENABLE_ERROR_LOG") || symbols.Contains("ENABLE_ERROR_AND_ABOVE_LOG"))),
                "Native framework warning/error logs are compiled out for " + buildGroup + ". Enable their public scripting symbols before validation.");
            report.checks.Add("Native framework warnings and errors are enabled for the current build target: " + buildGroup + ".");
            LunaAssetRepair.Validate(report.checks);
            using (var stream = File.OpenRead("Assets/Plugins/GameFramework.dll"))
            using (var sha = SHA256.Create())
                Require(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant() == CoreHash,
                    "Core DLL differs from supplied GameFramework-Core-0.1.0.unitypackage.");
            report.checks.Add("Core DLL SHA-256 exactly matches the supplied GameFramework-Core-0.1.0 package.");
            foreach (string path in new[] { LunaMigrationSetup.LoadingScene }.Concat(LunaMigrationSetup.NativeScenes))
            {
                InspectScene(path); report.sceneCount++;
            }
            string templateScene = ConfiguredTemplateScene();
            InspectScene(templateScene); report.templateSceneCount++;
            report.checks.Add(report.sceneCount + " operational scenes and " + report.templateSceneCount +
                " configured default scene template passed hierarchy/serialized-reference inspection: " + templateScene + ".");
            string[] roots = { "Assets/Resources", "Assets/Resources_moved", "Assets/Resources_static" };
            roots = roots.Where(AssetDatabase.IsValidFolder).ToArray();
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", roots))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Require(prefab != null, "Unreadable prefab: " + path);
                InspectHierarchy(prefab, path); InspectYaml(path); report.prefabCount++;
            }
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", roots))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); InspectYaml(path);
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) if (asset != null) InspectReferences(asset, path);
                report.dataAssetCount++;
            }
            LunaSpriteSliceRegression.Run();
            report.checks.Add("Spine sprite slicing preserves authored sprite IDs/local fileIDs, updates/appends slices idempotently and converts an explicitly plain texture.");
            InspectDuplicateGuids();
            Require(!Directory.Exists("Assets/LunaFramework"), "Retired framework directory remains.");
            Require(!Directory.Exists("Packages/com.everyday.devup.framework.template.v.0.1.1"), "Retired framework package remains.");
            foreach (string path in Directory.EnumerateFiles("Assets/Script", "*.cs", SearchOption.AllDirectories))
            {
                string source = File.ReadAllText(path);
                Require(!source.Contains("EverydayDevup." + "Framework"), "Runtime still references retired framework: " + path);
                Require(!source.Contains("LunaEntity" + "Behaviour"), "Runtime still uses the retired lifecycle bridge: " + path);
            }
            report.checks.Add("Production scenes, configured default scene template, prefabs and data have no missing scripts or unresolved serialized references; retired framework and lifecycle bridge are absent.");
        }
        catch (Exception error) { Fail(error.ToString()); }
        finally
        {
            if (!Application.isBatchMode && priorScenes.Length > 0 && priorScenes.All(s => !string.IsNullOrEmpty(s.path) && File.Exists(s.path)))
                EditorSceneManager.RestoreSceneManagerSetup(priorScenes);
            Complete();
        }
    }
    public static void Smoke() => RunRuntime("smoke", "native-smoke");
    public static void ContentMatrix() => RunRuntime("content-matrix", "native-matrix");
    private static void RunRuntime(string operation, string mode)
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate Unity batch process for integration checks.");
        Begin(operation);
        try
        {
            string save = Path.Combine(Output, "NativeSave", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(save); File.WriteAllText(Path.Combine(save, ".luna-validation-isolated"), "Dedicated native validation save; never user progress.");
            string runtimePath = Path.Combine(Output, "native-runtime-" + operation + ".json");
            SessionState.SetString(RuntimePathKey, runtimePath);
            Environment.SetEnvironmentVariable("LUNA_SAVE_DIRECTORY", save);
            Environment.SetEnvironmentVariable("LUNA_VALIDATION_DIRECTORY", Output);
            Environment.SetEnvironmentVariable("LUNA_NATIVE_REPORT_PATH", runtimePath);
            Environment.SetEnvironmentVariable("LUNA_VALIDATION_MODE", mode);
            report.observations.Add("Isolated save: " + save);
            report.observations.Add("Native runtime evidence: " + runtimePath);
            SessionState.SetString(StateKey, "runtime"); Persist();
            EditorSceneManager.OpenScene(LunaMigrationSetup.LoadingScene, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Fail(error.ToString()); LeavePlay(); }
    }
    public static void BuildWindows() => Build(false);
    public static void BuildWindowsClean() => Build(true);
    private static void Build(bool clean)
    {
        Begin("build-windows");
        try
        {
            LunaMigrationSetup.PrepareBuild();
            string target = Path.GetFullPath("Builds/Revival/Windows/LunaDefense.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = target, target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | (clean ? BuildOptions.CleanBuildCache : BuildOptions.None)
            });
            report.sceneCount = 1;
            report.checks.Add("Build result: " + result.summary.result + "; errors=" + result.summary.totalErrors + "; warnings=" + result.summary.totalWarnings);
            report.observations.Add("Player: " + target);
            Require(result.summary.result == BuildResult.Succeeded, "Windows build failed.");
        }
        catch (Exception error) { Fail(error.ToString()); }
        // A bounded normal Editor-loop drain is diagnostic only. Shutdown logs remain the
        // authority for native allocator messages emitted after this process writes its report.
        idleUpdates = 0; drainStart = EditorApplication.timeSinceStartup;
        SessionState.SetString(StateKey, "build-drain"); Persist();
    }
    private static void Tick()
    {
        if (report == null || completing) return;
        string state = SessionState.GetString(StateKey, "");
        if (state == "build-drain")
        {
            idleUpdates = ShaderUtil.anythingCompiling || EditorApplication.isCompiling || EditorApplication.isUpdating ? 0 : idleUpdates + 1;
            if (idleUpdates < 16 && EditorApplication.timeSinceStartup - drainStart < 60d) return;
            report.observations.Add("Normal post-build Editor updates: " + idleUpdates + " consecutive idle observations; elapsed=" + (EditorApplication.timeSinceStartup - drainStart).ToString("F3", CultureInfo.InvariantCulture) + "s. Native shutdown allocator status must be read from the complete process log.");
            if (idleUpdates < 16) Fail("Post-build Editor work did not settle within 60 seconds.");
            Complete(); return;
        }
        if (state != "runtime") return;
        DateTime started = DateTime.Parse(SessionState.GetString(StartedKey, ""), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
        if (DateTime.UtcNow - started > TimeSpan.FromMinutes(report.operation == "smoke" ? 10 : 120))
        { Fail("Native runtime validation exceeded its deadline."); LeavePlay(); return; }
        report.runtimeFrames = Time.frameCount;
        string path = SessionState.GetString(RuntimePathKey, "");
        if (!File.Exists(path)) return;
        RuntimeReport runtime;
        try { runtime = JsonUtility.FromJson<RuntimeReport>(File.ReadAllText(path)); }
        catch (IOException) { return; }
        catch (ArgumentException) { return; }
        if (runtime == null || string.IsNullOrEmpty(runtime.startedUtc) || string.IsNullOrEmpty(runtime.finishedUtc)) return;
        if (!DateTime.TryParse(runtime.startedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime runtimeStart)
            || runtimeStart.ToUniversalTime() < started.AddSeconds(-2)) return;
        if (runtime.checks != null) report.checks.AddRange(runtime.checks);
        if (runtime.errors != null) foreach (string error in runtime.errors) Fail(error);
        ReconcileWarnings(runtime);
        foreach (string diagnostic in report.injectedDiagnostics)
            if (runtime.expectedDiagnostics == null || !runtime.expectedDiagnostics.Contains(diagnostic))
                Fail("An injected diagnostic was not verified by a completed ownership case: " + diagnostic);
        if (runtime.expectedDiagnostics != null)
            foreach (string diagnostic in runtime.expectedDiagnostics)
                if (!report.injectedDiagnostics.Contains(diagnostic))
                    Fail("The native report claimed an injected diagnostic not observed by the Editor: " + diagnostic);
        if (report.injectedDiagnostics.Count > 0)
            report.observations.Add("Exact injected error messages verified against completed native ownership assertions: " + report.injectedDiagnostics.Count);
        if (!runtime.success) Fail("Native runtime driver reported failure. Full cases: " + path);
        Persist(); LeavePlay();
    }
    private static void ReconcileWarnings(RuntimeReport runtime)
    {
        var claimed = runtime.expectedWarnings ?? new List<string>();
        foreach (string warning in claimed)
            if (string.IsNullOrEmpty(warning)) Fail("Native report claimed an empty expected warning.");
        CheckWarningOccurrences(runtime.warnings, claimed, "native runtime");
        CheckWarningOccurrences(report.warnings, claimed, "Editor-captured");
        report.expectedWarnings = new List<string>(claimed);
    }
    private static void CheckWarningOccurrences(IEnumerable<string> observed, IEnumerable<string> expected, string context)
    {
        var actual = WarningCounts(observed);
        var claimed = WarningCounts(expected);
        foreach (string warning in actual.Keys.Union(claimed.Keys, StringComparer.Ordinal))
        {
            actual.TryGetValue(warning, out int actualCount);
            claimed.TryGetValue(warning, out int expectedCount);
            if (actualCount != expectedCount)
                Fail(context + " warning occurrences differ: expected=" + expectedCount + ", observed=" + actualCount + ": " + warning);
        }
    }
    private static Dictionary<string, int> WarningCounts(IEnumerable<string> warnings)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (string warning in warnings ?? Array.Empty<string>())
        {
            string key = warning ?? "<null warning>";
            result.TryGetValue(key, out int count); result[key] = count + 1;
        }
        return result;
    }
    private static void LeavePlay()
    {
        SessionState.SetString(StateKey, "leaving"); Persist();
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); else Complete();
    }
    private static void OnPlayState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode || report == null) return;
        string phase = SessionState.GetString(StateKey, "");
        if (phase == "runtime") Fail("Play mode stopped before the native report completed.");
        if (phase == "runtime" || phase == "leaving") Complete();
    }
    private static string ConfiguredTemplateScene()
    {
        const string settingsPath = "ProjectSettings/ProjectSettings.asset";
        const string property = "  templateDefaultScene:";
        string[] fields = File.ReadLines(settingsPath).Where(line => line.StartsWith(property, StringComparison.Ordinal)).ToArray();
        Require(fields.Length == 1, "Expected one serialized templateDefaultScene property in " + settingsPath + ".");
        string path = fields[0].Substring(property.Length).Trim();
        Require(path.StartsWith("Assets/", StringComparison.Ordinal) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) &&
            !path.Contains("\\") && path.Split('/').All(segment => segment.Length > 0 && segment != "." && segment != ".."),
            "Configured templateDefaultScene must be a project-relative Assets scene path: " + path);
        Require(File.Exists(path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null,
            "Configured default scene template is missing or unreadable: " + path);
        return path;
    }
    private static void InspectScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects()) InspectHierarchy(root, path);
        InspectYaml(path);
    }
    private static void InspectHierarchy(GameObject root, string path)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0) Fail(path + ": missing script on " + child.name);
            foreach (var component in child.GetComponents<Component>()) if (component != null) InspectReferences(component, path + ":" + child.name);
        }
    }
    private static void InspectReferences(UnityEngine.Object target, string context)
    {
        using (var serialized = new SerializedObject(target))
        {
            var property = serialized.GetIterator();
            while (property.Next(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null && property.objectReferenceEntityIdValue != EntityId.None)
                    Fail(context + ": broken " + target.GetType().Name + "." + property.propertyPath);
        }
    }
    private static void InspectYaml(string path)
    {
        string source = File.ReadAllText(path);
        if (!source.StartsWith("%YAML", StringComparison.Ordinal)) return;
        foreach (string guid in GuidReference.Matches(source).Cast<Match>().Select(m => m.Groups[1].Value).Distinct())
            if (!guid.StartsWith("0000000000000000", StringComparison.Ordinal) && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid))) Fail(path + ": unresolved asset GUID " + guid);
    }
    private static void InspectDuplicateGuids()
    {
        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string root in new[] { "Assets", "Packages" })
            foreach (string path in Directory.EnumerateFiles(root, "*.meta", SearchOption.AllDirectories))
            {
                string line = File.ReadLines(path).FirstOrDefault(s => s.StartsWith("guid: ", StringComparison.Ordinal));
                if (line == null) continue;
                string guid = line.Substring(6).Trim();
                if (seen.TryGetValue(guid, out string previous)) Fail("Duplicate GUID: " + previous + " and " + path);
                else seen.Add(guid, path);
            }
        report.checks.Add("Assets/embedded Packages metadata has no duplicate GUIDs.");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
    private static void CaptureLog(string message, string stack, LogType type)
    {
        if (report == null || message.StartsWith("[LunaValidation]", StringComparison.Ordinal)) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            // Native fault probes must prove their ownership/release assertions before the
            // runtime report may claim these exact messages. Unclaimed diagnostics still fail.
            if (SessionState.GetString(StateKey, "") == "runtime" && message.Contains("LUNA_INJECTED_OWNERSHIP"))
            {
                if (!report.injectedDiagnostics.Contains(message)) report.injectedDiagnostics.Add(message);
                Persist();
            }
            else Fail(message + "\n" + stack);
        }
        else if (type == LogType.Warning) { report.warnings.Add(message); Persist(); }
    }
    private static void Fail(string message) { if (!report.errors.Contains(message)) report.errors.Add(message); Persist(); }
    private static void Persist()
    {
        if (report == null) return;
        string json = JsonUtility.ToJson(report, true);
        SessionState.SetString(ReportKey, json); File.WriteAllText(Path.Combine(Output, report.operation + ".json"), json);
    }
    private static void Complete()
    {
        if (completing || report == null) return;
        completing = true;
        SessionState.SetString(StateKey, "");
        Application.logMessageReceived -= CaptureLog; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayState;
        Environment.SetEnvironmentVariable("LUNA_VALIDATION_MODE", null);
        Environment.SetEnvironmentVariable("LUNA_NATIVE_REPORT_PATH", null);
        // Recheck after play-mode shutdown as saves and teardown can log after the
        // completed native report. Only exact warnings proven by a fixture are expected.
        CheckWarningOccurrences(report.warnings, report.expectedWarnings, "Editor completion");
        report.finishedUtc = DateTime.UtcNow.ToString("O"); report.success = report.errors.Count == 0; Persist();
        Debug.Log("[LunaValidation] " + report.operation + ": " + (report.success ? "PASS" : "FAIL") + "; errors=" + report.errors.Count + "; warnings=" + report.warnings.Count);
        if (Application.isBatchMode) EditorApplication.Exit(report.success ? 0 : 1);
    }
}

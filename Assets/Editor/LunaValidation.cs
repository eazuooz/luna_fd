using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Reproducible integration checks. Invoke with Tools/validation/Validate-Luna.ps1.</summary>
[InitializeOnLoad]
public static class LunaValidation
{
    private const string ActiveKey = "LunaValidation.Active";
    private const string ReportKey = "LunaValidation.Report";
    private const string PhaseKey = "LunaValidation.Phase";
    private const string PhaseTimeKey = "LunaValidation.PhaseTime";
    private const string SceneTimeKey = "LunaValidation.SceneTime";
    private const string SceneFrameKey = "LunaValidation.SceneFrame";
    private const string MenuIndexKey = "LunaValidation.MenuIndex";
    private const string BuildDrainKey = "LunaValidation.BuildDrain";
    private const string BuildDrainStartedKey = "LunaValidation.BuildDrainStarted";
    private const string BuildDrainUpdatesKey = "LunaValidation.BuildDrainUpdates";
    private const string BuildDrainIdleKey = "LunaValidation.BuildDrainIdle";
    private static double buildDrainStarted;
    private static int buildDrainUpdates, buildDrainIdleUpdates;
    private static string buildDrainLastFlags;
    private static readonly string[][] LobbyMenus =
    {
        new[] { "OnClickCharacterList", "UICharacterList", "OnClickReleaseButton" },
        new[] { "OnClickInventory", "UIInventory", "ReleaseThisUI" },
        new[] { "OnClickShop", "UIGameShop", "OnReleaseButton" },
        new[] { "OnClickUpgrade", "UIUpgrade", "OnClickReleaseButton" }
    };
    private static readonly string OutputDirectory = Path.GetFullPath("Logs/Revival");
    private static readonly Regex GuidReference = new Regex(@"guid:\s*([a-fA-F0-9]{32})", RegexOptions.Compiled);
    private static ValidationReport report;
    private static double nextUpdate;
    private static bool finishing;
    private static GameObject originalPlayer;
    private static GameObject firstPooledPlayer;
    private static GameObject respawnTower;
    private static float respawnTowerHp;
    private static int originalTowerLevel;
    private static Vector3 respawnAuthoredScale;
    private static readonly string[] CharacterTabs = { "HeroScroll View", "SoldierScroll View", "AllScroll View", "HeroScroll View" };
    private static GameObject[] characterPreviews;
    private static Transform[] characterPreviewParents;
    private static int characterTabIndex;
    private static Scene previousGameScene;
    private static GameObject previousStage;
    private static string resultStageName;
    private static string expectedNextStage;
    private static bool stageWasClear;
    private static string savedStageBeforeDefeat;
    private static int goldBeforeVictory;
    private static int rewardBeforeVictory;
    private static int collectedVictoryCoins;

    [Serializable]
    private sealed class ValidationReport
    {
        public string operation;
        public string startedUtc;
        public string finishedUtc;
        public string unityVersion;
        public bool success;
        public int sceneCount;
        public int prefabCount;
        public int dataAssetCount;
        public int runtimeFrames;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> observations = new List<string>();
    }

    static LunaValidation()
    {
        bool drainingBuild = SessionState.GetBool(BuildDrainKey, false);
        if (!drainingBuild && !SessionState.GetBool(ActiveKey, false)) return;
        report = JsonUtility.FromJson<ValidationReport>(SessionState.GetString(ReportKey, "{}"));
        if (drainingBuild)
        {
            buildDrainStarted = double.Parse(SessionState.GetString(BuildDrainStartedKey, "0"), CultureInfo.InvariantCulture);
            buildDrainUpdates = SessionState.GetInt(BuildDrainUpdatesKey, 0);
            buildDrainIdleUpdates = SessionState.GetInt(BuildDrainIdleKey, 0);
            SubscribeBuildDrain();
        }
        else Subscribe();
    }

    public static void Audit()
    {
        Begin("audit");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) Fail("No enabled build scenes.");
            if (scenes.FirstOrDefault() != "Assets/Scenes/Loading.unity") Fail("Loading must be the first build scene.");
            foreach (string name in new[] { "Loading", "Lobby", "Game", "Dungeon" })
                if (!scenes.Any(s => Path.GetFileNameWithoutExtension(s) == name)) Fail("Missing enabled scene: " + name);

            var inspected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in scenes)
            {
                if (!File.Exists(path)) { Fail("Missing scene asset: " + path); continue; }
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects()) InspectHierarchy(root, path);
                InspectYamlReferences(path);
                inspected.Add(path);
                report.sceneCount++;
            }
            // Resources includes all production stages, units, UI and ScriptableObject data.
            // Addressable UI can live outside Resources, so also inspect their serialized assets.
            string[] folders = new[] { "Assets/Resources", "Assets/Resources_moved", "Assets/Resources_static", "Assets/Scenes", "Assets/LunaFramework" }
                .Where(AssetDatabase.IsValidFolder).ToArray();
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!inspected.Add(path)) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) { Fail("Cannot load prefab: " + path); continue; }
                InspectHierarchy(prefab, path);
                InspectYamlReferences(path);
                report.prefabCount++;
            }
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!inspected.Add(path)) continue;
                InspectYamlReferences(path);
                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (asset) InspectObjectReferences(asset, path);
                report.dataAssetCount++;
            }
            InspectProductionContent();
            CheckBalanceCsv(false);
            FindType("LunaSpriteSliceRegression").GetMethod("Run").Invoke(null, null);
            report.checks.Add("Sprite slicing preserved authored GUID/local fileIDs, pivots/borders and unrelated mappings; update plus append and repeat were checked.");
            InspectDuplicateGuids();
            report.checks.Add("Inspected all enabled scenes and production prefab/data roots for missing scripts and broken serialized references.");
            report.checks.Add("Third-party demonstration scenes are excluded from the production audit.");
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        finally
        {
            try
            {
                // Batch startup often contains an untitled scene, which cannot be restored.
                if (!Application.isBatchMode && setup.Length > 0 && setup.All(s => !string.IsNullOrEmpty(s.path) && File.Exists(s.path)))
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
            catch (Exception ex) { Fail("Could not restore editor scene setup: " + ex.Message); }
            finally { Complete(); }
        }
    }

    public static void Smoke()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Run Smoke in a separate Unity batch process.");
        Begin("smoke");
        try
        {
            // Keep integration checks independent of the user's real save, including quit-time autosave.
            string saveDirectory = Path.Combine(OutputDirectory, "SmokeSave", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(saveDirectory);
            Environment.SetEnvironmentVariable("LUNA_SAVE_DIRECTORY", saveDirectory);
            report.checks.Add("Isolated save directory: " + saveDirectory);
            SessionState.SetBool(ActiveKey, true);
            SetPhase("enter-play");
            Subscribe();
            EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity", OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception ex) { Fail(ex.ToString()); FinishSmoke(); }
    }

    public static void ContentMatrix() => LunaContentMatrix.Run();

    public static void BuildWindows() => BuildWindows(false);
    public static void BuildWindowsClean() => BuildWindows(true);

    private static void BuildWindows(bool cleanBuildCache)
    {
        Begin("build-windows");
        try
        {
            FindType("LunaMigrationSetup").GetMethod("PrepareBuild", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No enabled scenes to build.");
            string buildPath = Path.GetFullPath("Builds/Revival/Windows/LunaDefense.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
            BuildReport build = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | (cleanBuildCache ? BuildOptions.CleanBuildCache : BuildOptions.None)
            });
            report.sceneCount = scenes.Length;
            report.checks.Add("Build result: " + build.summary.result + "; errors=" + build.summary.totalErrors + "; warnings=" + build.summary.totalWarnings);
            report.checks.Add("Player: " + buildPath);
            if (build.summary.result != BuildResult.Succeeded) Fail("Windows player build did not succeed.");
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        BeginBuildDrain();
    }

    // BuildPlayer may return while editor shader jobs still await normal update-loop cleanup.
    // Return to that loop with log capture/report open before deciding the process exit code.
    private static void BeginBuildDrain()
    {
        buildDrainStarted = EditorApplication.timeSinceStartup;
        buildDrainUpdates = buildDrainIdleUpdates = 0;
        buildDrainLastFlags = null;
        SessionState.SetBool(BuildDrainKey, true);
        SessionState.SetString(BuildDrainStartedKey, buildDrainStarted.ToString("R", CultureInfo.InvariantCulture));
        SessionState.SetInt(BuildDrainUpdatesKey, 0);
        SessionState.SetInt(BuildDrainIdleKey, 0);
        report.observations.Add("BuildPlayer returned; waiting on normal editor updates for 16 consecutive idle shader/script/import observations (60 second bound). Diagnostic grace window only; native allocation reclamation is not guaranteed.");
        Persist();
        SubscribeBuildDrain();
    }
    private static void SubscribeBuildDrain()
    {
        Application.logMessageReceived -= CaptureLog;
        Application.logMessageReceived += CaptureLog;
        EditorApplication.update -= UpdateBuildDrain;
        EditorApplication.update += UpdateBuildDrain;
    }
    private static void UpdateBuildDrain()
    {
        if (!SessionState.GetBool(BuildDrainKey, false)) { EditorApplication.update -= UpdateBuildDrain; return; }
        try
        {
            buildDrainUpdates++;
            bool shaders = ShaderUtil.anythingCompiling;
            bool scripts = EditorApplication.isCompiling;
            bool imports = EditorApplication.isUpdating;
            buildDrainIdleUpdates = shaders || scripts || imports ? 0 : buildDrainIdleUpdates + 1;
            SessionState.SetInt(BuildDrainUpdatesKey, buildDrainUpdates);
            SessionState.SetInt(BuildDrainIdleKey, buildDrainIdleUpdates);
            double elapsed = EditorApplication.timeSinceStartup - buildDrainStarted;
            string flags = "shaderCompiling=" + shaders + "; scriptCompiling=" + scripts + "; editorUpdating=" + imports;
            if (flags != buildDrainLastFlags)
            {
                buildDrainLastFlags = flags;
                report.observations.Add("Build drain state at " + elapsed.ToString("F3", CultureInfo.InvariantCulture) + "s/update " + buildDrainUpdates + ": " + flags);
                Persist();
            }
            if (buildDrainIdleUpdates < 16 && elapsed < 60d) return;
            report.observations.Add("Build drain finished: elapsedSeconds=" + elapsed.ToString("F3", CultureInfo.InvariantCulture) +
                "; updates=" + buildDrainUpdates + "; consecutiveIdleUpdates=" + buildDrainIdleUpdates + "; " + flags);
            if (buildDrainIdleUpdates < 16) Fail("Build finalization timed out after 60 seconds while waiting for idle editor shader/script/import flags: " + flags);
            else report.checks.Add("Returned from BuildPlayer and observed shader/script/import work idle on 16 consecutive editor updates before process exit. This diagnostic observation does not prove native allocations were reclaimed.");
            Complete();
        }
        catch (Exception ex)
        {
            Fail("Build finalization failed: " + ex);
            Complete();
        }
    }

    private static void Begin(string operation)
    {
        Directory.CreateDirectory(OutputDirectory);
        report = new ValidationReport { operation = operation, startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion };
        finishing = false;
        Application.logMessageReceived -= CaptureLog;
        Application.logMessageReceived += CaptureLog;
        Persist();
    }

    private static void Subscribe()
    {
        Application.logMessageReceived -= CaptureLog;
        Application.logMessageReceived += CaptureLog;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(ActiveKey, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) SetPhase("loading");
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (SessionState.GetString(PhaseKey, "") != "complete") Fail("Play mode stopped before smoke checks completed.");
            SessionState.SetBool(ActiveKey, false);
            Complete();
        }
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(ActiveKey, false) || EditorApplication.timeSinceStartup < nextUpdate || finishing) return;
        nextUpdate = EditorApplication.timeSinceStartup + 0.1;
        string phase = SessionState.GetString(PhaseKey, "enter-play");
        double elapsed = EditorApplication.timeSinceStartup - SessionState.GetFloat(PhaseTimeKey, 0);
        if (phase == "complete") return;
        if (elapsed > 90) { Fail("Timed out in smoke phase: " + phase); FinishSmoke(); return; }
        if (!EditorApplication.isPlaying) return;
        report.runtimeFrames = Time.frameCount;
        if (report.errors.Count > 0) { FinishSmoke(); return; }
        try
        {
            switch (phase)
            {
                case "loading":
                    if (elapsed < 2) return;
                    if (SceneManager.GetActiveScene().name == "Lobby")
                    {
                        report.checks.Add("Loading UI initialized and completed normal lobby continuation.");
                        SetPhase("lobby");
                        break;
                    }
                    MonoBehaviour loading = FindActive("UILoading");
                    if (!loading) return;
                    // The real action accepts input only once async loading is ready.
                    Invoke(loading, "ContinueToLobby");
                    break;
                case "lobby":
                    if (!SceneReady("Lobby", elapsed, 3)) return;
                    RequireActive("UILobby");
                    RequireActive("Status");
                    report.checks.Add("Lobby initialized with player/deck entities.");
                    CheckBalanceCsv(true);
                    CheckEntityOwnership("initial lobby");
                    CheckSaveRoundTrip();
                    CheckColdStageSelection();
                    SessionState.SetInt(MenuIndexKey, 0);
                    Invoke(RequireActive("UILobby"), LobbyMenus[0][0]);
                    SetPhase("lobby-menu");
                    break;
                case "lobby-menu":
                    string[] menu = LobbyMenus[SessionState.GetInt(MenuIndexKey, 0)];
                    if (elapsed < 2 || !FindActive(menu[1])) return;
                    if (menu[1] == "UICharacterList")
                    {
                        BeginCharacterTabCheck(RequireActive(menu[1]));
                        SetPhase("character-tabs");
                        break;
                    }
                    Invoke(RequireActive(menu[1]), menu[2]);
                    report.checks.Add("Opened and closed visible " + menu[1] + " through lobby callbacks.");
                    SetPhase("lobby-menu-return");
                    break;
                case "character-tabs":
                    if (elapsed < 1) return;
                    if (!CheckCharacterTab()) { SetPhase("character-tabs"); break; }
                    Invoke(RequireActive("UICharacterList"), "OnClickReleaseButton");
                    report.checks.Add("Character preview identities, parents and native ownership survived Hero -> Soldier -> All -> Hero tabs, then the menu closed.");
                    SetPhase("lobby-menu-return");
                    break;
                case "lobby-menu-return":
                    if (elapsed < 1) return;
                    MonoBehaviour lobby = RequireActive("UILobby");
                    int menuIndex = SessionState.GetInt(MenuIndexKey, 0) + 1;
                    SessionState.SetInt(MenuIndexKey, menuIndex);
                    if (menuIndex < LobbyMenus.Length)
                    {
                        Invoke(lobby, LobbyMenus[menuIndex][0]);
                        SetPhase("lobby-menu");
                    }
                    else
                    {
                        Invoke(lobby, "OnClickStart");
                        SetPhase("game");
                    }
                    break;
                case "game":
                    if (!SceneReady("Game", elapsed, 12)) return;
                    RequireActive("BasicDungeon");
                    RequireActive("UIPlayer");
                    RequireActive("Status");
                    CheckEntityOwnership("main stage");
                    report.checks.Add("Main stage entered through lobby start and ran for at least 12 seconds and 30 frames without logged errors.");
                    BeginRespawnCheck();
                    SetPhase("respawn-first");
                    break;
                case "respawn-first":
                    if (!CheckRespawn(false)) return;
                    // Simulate a temporary growth effect interrupted by death; pool reuse must restore authored scale.
                    firstPooledPlayer.transform.localScale = respawnAuthoredScale * 1.5f;
                    KillPlayerForRespawn(firstPooledPlayer);
                    SetPhase("respawn-reuse");
                    break;
                case "respawn-reuse":
                    if (!CheckRespawn(true)) return;
                    object runtimePlayer = GetRuntimePlayerData();
                    runtimePlayer.GetType().GetField("TowerHPLevel").SetValue(runtimePlayer, originalTowerLevel);
                    report.checks.Add("Player completed two pooled respawns after real zero-HP death, reusing the returned instance; active player count stayed one, tower linkage persisted, tower HP did not multiply and interrupted growth scale reset to authored scale. Respawn delay was set to zero for deterministic validation.");
                    BeginStageResultCheck(false);
                    SetPhase("stage-defeat");
                    break;
                case "stage-defeat":
                    if (!FindActive("UIResult")) return;
                    CheckStageResult(false);
                    previousGameScene = SceneManager.GetActiveScene();
                    previousStage = RequireActive("BasicDungeon").gameObject;
                    Invoke(RequireActive("UIResult"), "ReStage");
                    SetPhase("stage-retry");
                    break;
                case "stage-retry":
                    if (SceneManager.GetActiveScene() == previousGameScene || !SceneReady("Game", elapsed, 12)) return;
                    CheckStageReload("retry", resultStageName);
                    report.checks.Add("Defeat result's retry callback reloaded Game -> Game with exactly one new stage and HUD.");
                    BeginStageResultCheck(true);
                    SetPhase("stage-victory");
                    break;
                case "stage-victory":
                    CollectVictoryCoins();
                    if (!FindActive("UIResult")) return;
                    CheckStageResult(true);
                    previousGameScene = SceneManager.GetActiveScene();
                    previousStage = RequireActive("BasicDungeon").gameObject;
                    Invoke(RequireActive("UIResult"), "GoNextStage");
                    SetPhase("stage-next");
                    break;
                case "stage-next":
                    if (SceneManager.GetActiveScene() == previousGameScene || !SceneReady("Game", elapsed, 12)) return;
                    CheckStageReload("next stage", expectedNextStage);
                    report.checks.Add("Victory result's next-stage callback completed the transition to " + expectedNextStage + " with one new stage and HUD.");
                    SceneManager.LoadScene("Lobby");
                    SetPhase("boss-lobby");
                    break;
                case "boss-lobby":
                case "endless-lobby":
                    if (!SceneReady("Lobby", elapsed, 3)) return;
                    Invoke(RequireActive("UILobby"), "OnClickSelectDungeon");
                    SetPhase(phase == "boss-lobby" ? "boss-select" : "endless-select");
                    break;
                case "boss-select":
                case "endless-select":
                    if (elapsed < 2 || !FindActive("UISelectDungeon")) return;
                    Invoke(RequireActive("UISelectDungeon"), "OnClickSelectDungeon", phase == "boss-select" ? 1 : 4);
                    SetPhase(phase == "boss-select" ? "boss-info" : "endless-info");
                    break;
                case "boss-info":
                case "endless-info":
                    if (elapsed < 2 || !FindActive("UIDungeonInfo")) return;
                    Invoke(RequireActive("UIDungeonInfo"), "OnClickSelectDungeon", phase == "boss-info" ? 1 : 4);
                    SetPhase(phase == "boss-info" ? "boss" : "endless");
                    break;
                case "boss":
                case "endless":
                    if (!SceneReady("Dungeon", elapsed, 12)) return;
                    RequireActive(phase == "boss" ? "BossDungeon" : "EndlessDungeon");
                    RequireActive("UIPlayer");
                    RequireActive("Status");
                    CheckEntityOwnership(phase + " dungeon");
                    report.checks.Add((phase == "boss" ? "Boss dungeon" : "Endless dungeon") + " entered through selection/info UI and ran for at least 12 seconds and 30 frames without logged errors.");
                    SceneManager.LoadScene("Lobby");
                    SetPhase(phase == "boss" ? "endless-lobby" : "final-lobby");
                    break;
                case "final-lobby":
                    if (!SceneReady("Lobby", elapsed, 3)) return;
                    RequireActive("UILobby");
                    CheckEntityOwnership("final lobby");
                    report.checks.Add("Returned to lobby after all three combat paths.");
                    FinishSmoke();
                    break;
            }
        }
        catch (Exception ex) { Fail(ex.ToString()); FinishSmoke(); }
    }

    private static bool SceneReady(string name, double elapsed, double minimum)
    {
        if (SceneManager.GetActiveScene().name != name) return false;
        double arrived = SessionState.GetFloat(SceneTimeKey, 0);
        if (arrived <= 0)
        {
            SessionState.SetFloat(SceneTimeKey, (float)EditorApplication.timeSinceStartup);
            SessionState.SetInt(SceneFrameKey, Time.frameCount);
            return false;
        }
        return EditorApplication.timeSinceStartup - arrived >= minimum && Time.frameCount - SessionState.GetInt(SceneFrameKey, 0) >= 30;
    }

    private static MonoBehaviour FindActive(string typeName)
    {
        return UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
            .FirstOrDefault(component => component && component.GetType().Name == typeName && component.isActiveAndEnabled);
    }

    private static MonoBehaviour RequireActive(string typeName)
    {
        MonoBehaviour component = FindActive(typeName);
        if (!component) throw new InvalidOperationException("Expected active component " + typeName + " in " + SceneManager.GetActiveScene().name);
        if (typeName.StartsWith("UI", StringComparison.Ordinal))
        {
            Canvas canvas = component.GetComponent<Canvas>();
            if (!canvas || !canvas.isActiveAndEnabled) throw new InvalidOperationException("Expected visible canvas for " + typeName);
        }
        return component;
    }

    private static void Invoke(MonoBehaviour component, string method, params object[] arguments)
    {
        MethodInfo action = component.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
        if (action == null) throw new MissingMethodException(component.GetType().FullName, method);
        action.Invoke(component, arguments);
    }

    private static void SetPhase(string phase)
    {
        SessionState.SetString(PhaseKey, phase);
        SessionState.SetFloat(PhaseTimeKey, (float)EditorApplication.timeSinceStartup);
        SessionState.SetFloat(SceneTimeKey, 0);
        Persist();
        Debug.Log("[LunaValidation] " + phase);
    }

    private static object Member(object target, string name)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = target.GetType().GetField(name, flags);
        return field != null ? field.GetValue(target) : target.GetType().GetProperty(name, flags).GetValue(target);
    }

    private static object RuntimeTable()
    {
        return FindType("LunaDataTable").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
    }

    private static object RuntimeManager() => Member(RequireActive("Game"), "dataManager");

    private static void CheckColdStageSelection()
    {
        object manager = RuntimeManager();
        MethodInfo select = manager.GetType().GetMethod("SetStagePrefab");
        select.Invoke(manager, new object[] { "Stage01-02" });
        var selected = (GameObject)Member(manager, "SelectStagePrefab");
        if (!selected || selected.name != "Stage01-02")
            throw new InvalidOperationException("Cold stage selection depends on opening a chooser menu first.");
        string saved = (string)Member(GetRuntimePlayerData(), "SaveStageName");
        select.Invoke(manager, new object[] { saved });
        report.checks.Add("Cold bootstrap selected Stage01-02 before any chooser opened, then restored the saved stage prefab.");
    }

    private static void BeginCharacterTabCheck(MonoBehaviour menu)
    {
        characterPreviews = menu.GetComponentsInChildren<MonoBehaviour>(true)
            .Where(c => c && c.GetType().Name == "UICharacterButton")
            .Select(c => (GameObject)Member(c, "instCharacterUI")).ToArray();
        if (characterPreviews.Length == 0 || characterPreviews.Any(go => !go))
            throw new InvalidOperationException("Character list did not instantiate all card previews.");
        characterPreviewParents = characterPreviews.Select(go => go.transform.parent).ToArray();
        characterTabIndex = 0;
        Invoke(menu, "OnTabClick", CharacterTabs[characterTabIndex]);
    }

    private static bool CheckCharacterTab()
    {
        MonoBehaviour menu = RequireActive("UICharacterList");
        MethodInfo isSpawned = FindType("EverydayDevup.Framework.LunaEntities").GetMethod("IsSpawned");
        GameObject[] current = menu.GetComponentsInChildren<MonoBehaviour>(true)
            .Where(c => c && c.GetType().Name == "UICharacterButton")
            .Select(c => (GameObject)Member(c, "instCharacterUI")).ToArray();
        if (current.Length != characterPreviews.Length) throw new InvalidOperationException("Tab change lost character preview cards.");
        for (int i = 0; i < characterPreviews.Length; i++)
        {
            GameObject preview = characterPreviews[i];
            if (!preview || !current.Contains(preview) || !preview.activeSelf || preview.transform.parent != characterPreviewParents[i] ||
                !(bool)isSpawned.Invoke(null, new object[] { preview }))
                throw new InvalidOperationException("Tab change destroyed, replaced, detached or unregistered a character preview: " + CharacterTabs[characterTabIndex]);
        }
        Transform selected = menu.GetComponentsInChildren<Transform>(true).Single(t => t.name == CharacterTabs[characterTabIndex]);
        if (!selected.gameObject.activeInHierarchy || !characterPreviews.Any(go => go.activeInHierarchy && go.transform.IsChildOf(selected)))
            throw new InvalidOperationException("Selected character tab has no visible preview: " + selected.name);
        characterTabIndex++;
        if (characterTabIndex == CharacterTabs.Length) return true;
        Invoke(menu, "OnTabClick", CharacterTabs[characterTabIndex]);
        return false;
    }

    private static void BeginStageResultCheck(bool victory)
    {
        MonoBehaviour stage = RequireActive("BasicDungeon");
        if ((bool)Member(stage, "isStageStop")) throw new InvalidOperationException("Stage stopped before result integration check.");
        object player = GetRuntimePlayerData();
        object manager = RuntimeManager();
        resultStageName = (string)Member(manager, "CurrentStageName");
        expectedNextStage = (string)manager.GetType().GetMethod("NextStageString").Invoke(manager, null);
        object stageData = ((System.Collections.IDictionary)Member(RuntimeTable(), "StageTable"))[resultStageName];
        stageWasClear = (bool)Member(stageData, "isClear");
        savedStageBeforeDefeat = (string)Member(player, "SaveStageName");
        goldBeforeVictory = (int)Member(player, "Gold");
        rewardBeforeVictory = (int)Member(player, "RewardGold");
        collectedVictoryCoins = 0;
        GameObject tower = (GameObject)Member(stage, victory ? "gameEnemyTower" : "gameAllyTower");
        if (!tower) throw new InvalidOperationException("Required tower is absent before result validation.");
        object stats = GetStats(tower);
        stats.GetType().GetField("Hp").SetValue(stats, 0f);
    }

    private static void CollectVictoryCoins()
    {
        foreach (MonoBehaviour coin in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
            .Where(c => c && c.isActiveAndEnabled && c.GetType().Name == "DropCoin"))
        {
            // Use the live reward object's normal public collection action after native initialization.
            Invoke(coin, "EnsureEntityInitialized");
            Invoke(coin, "GetCoin");
            collectedVictoryCoins++;
        }
    }

    private static void CheckStageResult(bool victory)
    {
        MonoBehaviour stage = RequireActive("BasicDungeon");
        MonoBehaviour result = RequireActive("UIResult");
        if (!(bool)Member(stage, "isStageStop") || (bool)Member(stage, "isStageWin") != victory)
            throw new InvalidOperationException("Tower death produced the wrong stage result.");
        object player = GetRuntimePlayerData();
        object stageData = ((System.Collections.IDictionary)Member(RuntimeTable(), "StageTable"))[resultStageName];
        Transform next = result.transform.Find("ImagePlate/NextStage");
        if (!victory)
        {
            if (next.gameObject.activeSelf || (bool)Member(stageData, "isClear") != stageWasClear ||
                (string)Member(player, "SaveStageName") != savedStageBeforeDefeat)
                throw new InvalidOperationException("Defeat incorrectly enabled next stage or advanced saved progress.");
            report.checks.Add("Real allied-tower zero-HP death produced a defeat result, disabled next stage and left stage progress unchanged.");
            return;
        }
        int rewardDelta = (int)Member(player, "RewardGold") - rewardBeforeVictory;
        if (!next.gameObject.activeSelf || !(bool)Member(stageData, "isClear") ||
            (string)Member(player, "SaveStageName") != expectedNextStage || rewardDelta <= 0 ||
            (int)Member(player, "Gold") != goldBeforeVictory + rewardDelta * 100)
            throw new InvalidOperationException("Victory result did not grant the live coin rewards and advance stage progress.");
        Type runtime = FindType("LunaRuntimeData");
        if (!(bool)runtime.GetMethod("SaveNow").Invoke(null, null) || !(bool)runtime.GetMethod("Reload").Invoke(null, null) ||
            !(bool)Member(stageData, "isClear") || (string)Member(player, "SaveStageName") != expectedNextStage)
            throw new InvalidOperationException("Victory progress did not survive save/reload.");
        report.checks.Add("Real enemy-tower death produced victory, awarded " + rewardDelta + " coin pickups (" + collectedVictoryCoins + " collected via the public GetCoin hook), and persisted stage clear/next stage.");
    }

    private static void CheckStageReload(string context, string expectedName)
    {
        var stages = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
            .Where(c => c && c.isActiveAndEnabled && c.GetType().Name == "BasicDungeon").ToArray();
        if (stages.Length != 1 || stages[0].gameObject == previousStage || stages[0].name.Replace("(Clone)", "") != expectedName ||
            (string)Member(RuntimeManager(), "CurrentStageName") != expectedName)
            throw new InvalidOperationException(context + ": duplicate, stale or wrong main stage after scene reload.");
        int playerUis = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
            .Count(c => c && c.isActiveAndEnabled && c.GetType().Name == "UIPlayer");
        if (playerUis != 1) throw new InvalidOperationException(context + ": expected exactly one player HUD.");
        RequireActive("UIPlayer");
        if (!GetCurrentPlayer()) throw new InvalidOperationException(context + ": active player pointer is missing.");
        CheckEntityOwnership(context);
    }

    private sealed class SavedField
    {
        public object target;
        public FieldInfo field;
        public object original;
        public object expected;
    }

    private static object FirstTableValue(object table, string name)
    {
        var dictionary = (System.Collections.IDictionary)Member(table, name);
        if (dictionary.Count == 0) throw new InvalidOperationException("Runtime table is empty: " + name);
        return dictionary.Values.Cast<object>().First();
    }

    private static bool SameSavedValue(object actual, object expected)
    {
        if (actual is List<string> actualList && expected is List<string> expectedList)
            return actualList.SequenceEqual(expectedList);
        return Equals(actual, expected);
    }

    private static void CheckSaveRoundTrip()
    {
        Type runtime = FindType("LunaRuntimeData");
        object table = RuntimeTable();
        object player = Member(table, "playerData");
        UnityEngine.Object source = Resources.Load("ScriptableObjects/PlayerData");
        if (ReferenceEquals(player, source)) throw new InvalidOperationException("Runtime PlayerData aliases the imported source asset.");
        string saveDirectory = (string)runtime.GetProperty("SaveDirectory").GetValue(null);
        if (!string.Equals(Path.GetFullPath(saveDirectory), Path.GetFullPath(Environment.GetEnvironmentVariable("LUNA_SAVE_DIRECTORY")), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Runtime did not honor the smoke test's isolated save directory.");
        var sourceSnapshots = Resources.LoadAll<ScriptableObject>("ScriptableObjects").ToDictionary(asset => asset, asset => JsonUtility.ToJson(asset));
        CheckUpgradeLimits(player);
        var fields = new List<SavedField>();
        Action<object, string, object> add = (target, name, expected) =>
        {
            FieldInfo field = target.GetType().GetField(name);
            if (field == null) throw new MissingFieldException(target.GetType().Name, name);
            if (field.FieldType.IsEnum) expected = Enum.ToObject(field.FieldType, expected);
            fields.Add(new SavedField { target = target, field = field, original = field.GetValue(target), expected = expected });
        };
        add(player, "Gold", (int)Member(player, "Gold") + 137);
        add(player, "StarStone", (int)Member(player, "StarStone") + 11);
        add(player, "LunaLevel", (int)Member(player, "LunaLevel") == 2 ? 3 : 2);
        add(player, "LunaMoveSpeedLevel", (int)Member(player, "LunaMoveSpeedLevel") == 2 ? 3 : 2);
        add(player, "SkinType", Convert.ToInt32(Member(player, "SkinType")) == 1 ? 0 : 1);
        add(player, "SaveStageName", (string)Member(player, "SaveStageName") == "Stage01-02" ? "Stage01-01" : "Stage01-02");
        var deck = new List<string>((List<string>)Member(player, "UnitNamesInDeck"));
        string firstHero = deck[0]; deck[0] = deck[1]; deck[1] = firstHero;
        add(player, "UnitNamesInDeck", deck);
        var skills = new List<string>((List<string>)Member(player, "equippedSkill"));
        string firstSkill = skills[0]; skills[0] = skills[1]; skills[1] = firstSkill;
        add(player, "equippedSkill", skills);
        object unit = FirstTableValue(table, "UnitTable");
        add(unit, "Level", Math.Max(1, (int)Member(unit, "Level")) + 1);
        add(unit, "moveSpeedLevel", Math.Max(1, (int)Member(unit, "moveSpeedLevel")) + 1);
        add(unit, "cardCount", (int)Member(unit, "cardCount") + 7);
        add(unit, "ultState", Convert.ToInt32(Member(unit, "ultState")) == 2 ? 3 : 2);
        object luna = FirstTableValue(table, "lunaTable");
        add(luna, "cardCount", (int)Member(luna, "cardCount") + 5);
        object item = FirstTableValue(table, "itemTable");
        add(item, "itemCount", (int)Member(item, "itemCount") + 3);
        add(item, "level", (int)Member(item, "level") + 1);
        add(item, "isActive", !(bool)Member(item, "isActive"));
        object skill = FirstTableValue(table, "skiiTable");
        add(skill, "level", Math.Max(1, (int)Member(skill, "level")) + 1);
        object stage = FirstTableValue(table, "StageTable");
        add(stage, "isClear", !(bool)Member(stage, "isClear"));
        Type settingsType = FindType("GameFramework.Setting.ISettingManager");
        object settings = FindType("GameFramework.GameFrameworkEntry").GetMethod("GetModule").MakeGenericMethod(settingsType).Invoke(null, null);
        MethodInfo getSound = settingsType.GetMethod("GetBool", new[] { typeof(string), typeof(bool) });
        MethodInfo setSound = settingsType.GetMethod("SetBool");
        bool originalSound = (bool)getSound.Invoke(settings, new object[] { "Luna.Sound.Enabled", true });
        Action<bool> apply = expected =>
        {
            foreach (SavedField change in fields) change.field.SetValue(change.target, expected ? change.expected : change.original);
            setSound.Invoke(settings, new object[] { "Luna.Sound.Enabled", expected ? !originalSound : originalSound });
        };
        Action<bool> assert = expected =>
        {
            foreach (SavedField change in fields)
                if (!SameSavedValue(change.field.GetValue(change.target), expected ? change.expected : change.original))
                    throw new InvalidOperationException("Save roundtrip mismatch: " + change.target.GetType().Name + "." + change.field.Name);
            bool sound = expected ? !originalSound : originalSound;
            if ((bool)getSound.Invoke(settings, new object[] { "Luna.Sound.Enabled", true }) != sound || Mathf.Abs(AudioListener.volume - (sound ? 1f : 0f)) > 0.001f)
                throw new InvalidOperationException("Sound setting or listener volume was not restored.");
            foreach (var snapshot in sourceSnapshots)
                if (JsonUtility.ToJson(snapshot.Key) != snapshot.Value)
                    throw new InvalidOperationException("Save roundtrip mutated an imported source asset: " + snapshot.Key.name);
        };
        Action<string> call = name =>
        {
            if (!(bool)runtime.GetMethod(name).Invoke(null, null)) throw new InvalidOperationException("Runtime save operation failed: " + name);
        };
        apply(true); call("SaveNow");
        apply(false); call("Reload"); assert(true);
        report.checks.Add("Saved/reloaded changed gold, stones, deck, equipped skills, skin, player/unit growth, rank/cards, items, skill levels, stage progress and sound; every loaded source ScriptableObject remained unchanged.");
        apply(false); call("SaveNow"); // Backup now contains the changed snapshot.
        string primary = Path.Combine(saveDirectory, "LunaSave.dat");
        if (!File.Exists(primary + ".bak")) throw new InvalidOperationException("Atomic save did not create the expected backup.");
        File.WriteAllBytes(primary, new byte[0]);
        call("Reload"); assert(true);
        File.WriteAllBytes(primary, new byte[] { 0xff, 0xff, 0xff, 0xff, 0x0f }); // Invalid negative 7-bit setting count.
        apply(false); call("Reload"); assert(true);
        File.WriteAllBytes(primary, new byte[] { 0x00, 0x7f }); // Empty setting count followed by forbidden trailing data.
        apply(false); call("Reload"); assert(true);
        File.Delete(primary); // The verified, unique smoke-save directory owns this test file.
        apply(false); call("Reload"); assert(true);
        apply(false); call("SaveNow"); call("Reload"); assert(false);
        report.checks.Add("Recovered the full changed snapshot after truncated data, a negative setting count, trailing bytes and a missing primary, then restored baseline progress; recovery warnings during these checks are expected.");
    }

    private static void CheckUpgradeLimits(object player)
    {
        FieldInfo gold = player.GetType().GetField("Gold");
        int originalGold = (int)gold.GetValue(player);
        string[][] upgrades =
        {
            new[] { "LunaLevel", "LunaLevelUp", "20" },
            new[] { "LunaMoveSpeedLevel", "LunaMoveSpeedLevelUp", "5" },
            new[] { "GoldSpeedLevel", "GoldSpeedLevelUp", "20" },
            new[] { "TowerHPLevel", "TowerHpLevelUp", "10" },
            new[] { "AuroraLevel", "AuroraLevelUp", "3" },
            new[] { "UnitMoveSpeedLevel", "UnitMoveSpeedLevelUp", "5" },
            new[] { "LunaReviveTimeLevel", "LunaReviveTimeLevelUp", "3" }
        };
        foreach (string[] upgrade in upgrades)
        {
            FieldInfo level = player.GetType().GetField(upgrade[0]);
            object original = level.GetValue(player);
            MethodInfo buy = player.GetType().GetMethod(upgrade[1]);
            try
            {
                gold.SetValue(player, 2000);
                int max = int.Parse(upgrade[2], CultureInfo.InvariantCulture);
                level.SetValue(player, max);
                if ((bool)buy.Invoke(player, new object[] { 1000 }) || (int)gold.GetValue(player) != 2000 || (int)level.GetValue(player) != max)
                    throw new InvalidOperationException("Maximum-level purchase spent money: " + upgrade[1]);
                level.SetValue(player, 1);
                if ((bool)buy.Invoke(player, new object[] { -1 }) || (int)gold.GetValue(player) != 2000 || (int)level.GetValue(player) != 1)
                    throw new InvalidOperationException("Negative upgrade cost changed money or level: " + upgrade[1]);
            }
            finally { level.SetValue(player, original); gold.SetValue(player, originalGold); }
        }
        report.checks.Add("All seven player upgrades rejected maximum-level and negative-cost purchases without changing gold or level.");
    }

    private static object GetRuntimePlayerData()
    {
        Type tableType = FindType("LunaDataTable");
        object table = tableType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
        return tableType.GetField("playerData").GetValue(table);
    }

    private static GameObject GetCurrentPlayer()
    {
        MonoBehaviour game = RequireActive("Game");
        object manager = game.GetType().GetField("dataManager").GetValue(game);
        return (GameObject)manager.GetType().GetProperty("LunaPlayer").GetValue(manager);
    }

    private static object GetStats(GameObject entity)
    {
        Component status = entity.GetComponent("Status");
        return status.GetType().GetField("stat").GetValue(status);
    }

    private static void BeginRespawnCheck()
    {
        originalPlayer = GetCurrentPlayer();
        if (!originalPlayer || !originalPlayer.activeInHierarchy) throw new InvalidOperationException("Player is not active before respawn validation.");
        Component state = originalPlayer.GetComponent("LunaStateMachine");
        respawnTower = (GameObject)state.GetType().GetField("myTower").GetValue(state);
        if (!respawnTower) throw new InvalidOperationException("Main-stage player has no allied tower before respawn.");
        object towerStats = GetStats(respawnTower);
        respawnTowerHp = (float)towerStats.GetType().GetField("Hp").GetValue(towerStats);
        object data = GetRuntimePlayerData();
        var playerPrefab = (GameObject)data.GetType().GetMethod("CurPlayer").Invoke(data, null);
        respawnAuthoredScale = playerPrefab.transform.localScale;
        originalTowerLevel = (int)data.GetType().GetField("TowerHPLevel").GetValue(data);
        // A value above one exposes an accidental repeat of the initial tower upgrade.
        data.GetType().GetField("TowerHPLevel").SetValue(data, 2);
        KillPlayerForRespawn(originalPlayer);
    }

    private static void KillPlayerForRespawn(GameObject player)
    {
        object stats = GetStats(player);
        stats.GetType().GetField("SpawnDelayTime").SetValue(stats, 0f);
        stats.GetType().GetField("Hp").SetValue(stats, 0f);
    }

    private static bool CheckRespawn(bool reused)
    {
        GameObject current = GetCurrentPlayer();
        if (!current || !current.activeInHierarchy) return false;
        object stats = GetStats(current);
        if ((float)stats.GetType().GetField("Hp").GetValue(stats) <= 0f) return false;
        if (reused && current != firstPooledPlayer) throw new InvalidOperationException("Second player respawn did not reuse the returned pool instance.");
        if (reused && (current.transform.localScale - respawnAuthoredScale).sqrMagnitude > 0.0001f)
            throw new InvalidOperationException("Pooled player retained an interrupted growth effect's scale.");
        int activePlayers = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude).Count(c => c && c.GetType().Name == "LunaStateMachine");
        if (activePlayers != 1) throw new InvalidOperationException("Expected one active Luna after respawn; found " + activePlayers);
        Component state = current.GetComponent("LunaStateMachine");
        if ((GameObject)state.GetType().GetField("myTower").GetValue(state) != respawnTower) throw new InvalidOperationException("Respawn lost the allied tower reference.");
        object towerStats = GetStats(respawnTower);
        if ((float)towerStats.GetType().GetField("Hp").GetValue(towerStats) > respawnTowerHp + 0.01f) throw new InvalidOperationException("Respawn applied the tower HP multiplier again.");
        Type pool = FindType("EverydayDevup.Framework.PoolManager");
        if (!(bool)pool.GetMethod("IsSpawned", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { current }))
            throw new InvalidOperationException("Respawned player is not registered as a spawned pool object.");
        if (!reused) firstPooledPlayer = current;
        return true;
    }

    private static Type FindType(string name)
    {
        foreach (Assembly assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
        {
            Type type = assembly.GetType(name, false);
            if (type != null) return type;
        }
        throw new TypeLoadException("Required integration API is missing: " + name);
    }

    private static void CheckBalanceCsv(bool runtime)
    {
        Type rowType = FindType("LunaUnitBalanceRow");
        Type csvType = FindType("LunaCsv");
        MethodInfo parse = rowType.GetMethod("ParseDocument", BindingFlags.Static | BindingFlags.Public);
        TextAsset csv = Resources.Load<TextAsset>("DataTables/UnitBalance");
        if (!csv) throw new InvalidOperationException("Unit balance CSV is missing.");
        var invariantRows = (System.Collections.IList)parse.Invoke(null, new object[] { csv.text });
        if (!runtime)
        {
            FindType("LunaUnitBalanceExporter").GetMethod("ValidateTable").Invoke(null, null);
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                var localizedRows = (System.Collections.IList)parse.Invoke(null, new object[] { csv.text });
                if (localizedRows.Count != invariantRows.Count) throw new InvalidOperationException("CSV row count depends on current culture.");
                string[] fields = (string[])rowType.GetField("BalanceFields").GetValue(null);
                foreach (int i in Enumerable.Range(0, invariantRows.Count))
                    foreach (string field in fields)
                        if (!Equals(rowType.GetMethod("GetValue").Invoke(invariantRows[i], new object[] { field }), rowType.GetMethod("GetValue").Invoke(localizedRows[i], new object[] { field })))
                            throw new InvalidOperationException("CSV value depends on current culture: row " + i + ", " + field);
            }
            finally { CultureInfo.CurrentCulture = originalCulture; }

            MethodInfo read = csvType.GetMethod("Read");
            var quoted = (System.Collections.IList)read.Invoke(null, new object[] { "Name,Note\r\n\"Luna, Defense\",\"line 1\r\nline \"\"2\"\"\"\r\n" });
            if (quoted.Count != 2 || ((string[])quoted[1])[0] != "Luna, Defense" || ((string[])quoted[1])[1] != "line 1\r\nline \"2\"")
                throw new InvalidOperationException("CSV quoted comma, escaped quote or multiline parsing failed.");
            var records = (System.Collections.IList)read.Invoke(null, new object[] { csv.text });
            string duplicate = (string)csvType.GetMethod("WriteRow").Invoke(null, new object[] { records[1] });
            bool rejected = false;
            try { parse.Invoke(null, new object[] { csv.text.TrimEnd() + "\n" + duplicate + "\n" }); }
            catch (TargetInvocationException ex) when (ex.InnerException is InvalidDataException) { rejected = true; }
            if (!rejected) throw new InvalidOperationException("Duplicate unit CSV rows were accepted.");
            report.checks.Add("CSV covers every ObjectData source; " + invariantRows.Count + " rows retain identical values under de-DE; quoted commas/newlines/quotes parse and duplicate rows are rejected.");
            return;
        }

        Type balance = FindType("LunaUnitBalance");
        int count = (int)balance.GetProperty("Count").GetValue(null);
        if (count != invariantRows.Count) throw new InvalidOperationException("Runtime Game Framework unit table count differs from CSV.");
        Type entry = FindType("GameFramework.GameFrameworkEntry");
        Type managerType = FindType("GameFramework.DataTable.IDataTableManager");
        object manager = entry.GetMethod("GetModule").MakeGenericMethod(managerType).Invoke(null, null);
        MethodInfo getTable = managerType.GetMethods().Single(m => m.Name == "GetDataTable" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
        object table = getTable.MakeGenericMethod(rowType).Invoke(manager, new object[] { "UnitBalance" });
        if (!ReferenceEquals(table, balance.GetProperty("Table").GetValue(null))) throw new InvalidOperationException("Unit balance is not registered in the actual Game Framework DataTable manager.");
        string resourcePath = (string)rowType.GetProperty("ResourcePath").GetValue(invariantRows[0]);
        UnityEngine.Object source = Resources.Load(resourcePath);
        FieldInfo hp = source.GetType().GetField("Hp");
        object sourceHp = hp.GetValue(source);
        object view = FindType("LunaRuntimeData").GetMethod("ResolveObjectData").Invoke(null, new object[] { source });
        object expectedHp = rowType.GetMethod("GetValue").Invoke(invariantRows[0], new object[] { "Hp" });
        if (ReferenceEquals(source, view) || !Equals(hp.GetValue(view), expectedHp) || !Equals(hp.GetValue(source), sourceHp))
            throw new InvalidOperationException("Runtime ObjectData view did not apply CSV balance independently of the source asset.");
        report.checks.Add("Actual Game Framework UnitBalance DataTable contains " + count + " rows; runtime unit view uses CSV HP and leaves source asset unchanged.");
    }

    private static void CheckEntityOwnership(string context)
    {
        Type ownership = FindType("EverydayDevup.Framework.LunaEntities");
        int active = (int)ownership.GetProperty("ActiveCount", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        Type entry = FindType("UnityGameFramework.Runtime.GameEntry");
        Type entityType = FindType("UnityGameFramework.Runtime.EntityComponent");
        object component = entry.GetMethod("GetComponent", new[] { typeof(Type) }).Invoke(null, new object[] { entityType });
        Array loaded = (Array)entityType.GetMethod("GetAllLoadedEntities", Type.EmptyTypes).Invoke(component, null);
        if (active <= 0 || active != loaded.Length) throw new InvalidOperationException(context + ": tracked/native Entity counts differ: " + active + "/" + loaded.Length);
        foreach (object entity in loaded)
        {
            var go = (GameObject)entity.GetType().GetProperty("Handle").GetValue(entity);
            if (!go || !go.activeSelf) throw new InvalidOperationException(context + ": Game Framework retained an inactive-root or destroyed entity.");
        }
        Type actorType = FindType("EverydayDevup.Framework.LunaEntityBehaviour");
        MethodInfo isSpawned = ownership.GetMethod("IsSpawned", BindingFlags.Public | BindingFlags.Static);
        var actors = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
            .Where(c => c && c.isActiveAndEnabled && actorType.IsAssignableFrom(c.GetType())).Select(c => c.gameObject).Distinct();
        foreach (GameObject actor in actors)
            if (!(bool)isSpawned.Invoke(null, new object[] { actor })) throw new InvalidOperationException(context + ": active actor is not owned by the Game Framework Entity manager: " + actor.name);
        report.checks.Add(context + ": " + active + " live entities match the native Game Framework Entity manager; active actors are registered.");
    }

    private static void FinishSmoke()
    {
        finishing = true;
        SetPhase("complete");
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        else
        {
            SessionState.SetBool(ActiveKey, false);
            Complete();
        }
    }

    private static void InspectHierarchy(GameObject root, string assetPath)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
            if (missing > 0) Fail(assetPath + ": " + HierarchyPath(child) + " has " + missing + " missing scripts.");
            foreach (Component component in child.GetComponents<Component>())
                if (component) InspectObjectReferences(component, assetPath + ":" + HierarchyPath(child));
        }
    }

    private static string HierarchyPath(Transform transform)
    {
        return transform.parent ? HierarchyPath(transform.parent) + "/" + transform.name : transform.name;
    }

    private static void InspectObjectReferences(UnityEngine.Object target, string context)
    {
        using (var serialized = new SerializedObject(target))
        {
            SerializedProperty property = serialized.GetIterator();
            while (property.Next(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue == null && property.objectReferenceEntityIdValue != EntityId.None)
                    Fail(context + ": broken " + target.GetType().Name + "." + property.propertyPath);
        }
    }

    private static void InspectYamlReferences(string path)
    {
        if (!File.Exists(path)) return;
        string text = File.ReadAllText(path);
        if (!text.StartsWith("%YAML", StringComparison.Ordinal)) return;
        foreach (string guid in GuidReference.Matches(text).Cast<Match>().Select(m => m.Groups[1].Value).Distinct())
        {
            if (guid.StartsWith("0000000000000000", StringComparison.Ordinal)) continue; // Unity built-in resources.
            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid))) Fail(path + ": unresolved asset GUID " + guid);
        }
    }

    private static void InspectProductionContent()
    {
        int stageCount = 0;
        foreach (GameObject stage in Resources.LoadAll<GameObject>("Prefabs/10Stage"))
        {
            Component controller = stage.GetComponent("BasicDungeon");
            if (!controller) { Fail("Stage has no BasicDungeon controller: " + stage.name); continue; }
            using (var serialized = new SerializedObject(controller))
                foreach (string field in new[] { "backgroundImage", "enemyTower", "allyTower" })
                    if (!serialized.FindProperty(field).objectReferenceValue) Fail(stage.name + ": required " + field + " is empty.");
            string resourcePath = "ScriptableObjects/DungeonData/BasicDungeon/" + stage.name;
            UnityEngine.Object data = Resources.Load(resourcePath);
            if (!data || data.GetType().Name != "BasicDungeonData") { Fail(stage.name + ": missing BasicDungeonData resource " + resourcePath); continue; }
            using (var serialized = new SerializedObject(data))
            {
                SerializedProperty monsters = serialized.FindProperty("nonBossMonsters");
                int spawnOrders = 0;
                for (int i = 0; i < monsters.arraySize; i++)
                {
                    SerializedProperty monster = monsters.GetArrayElementAtIndex(i);
                    int order = monster.FindPropertyRelative("spawnOrder").intValue;
                    if (order < 0) Fail(stage.name + ": negative monster spawn order at " + i);
                    spawnOrders += order;
                    if (!monster.FindPropertyRelative("monster").objectReferenceValue) Fail(stage.name + ": missing monster prefab at " + i);
                }
                if (spawnOrders <= 0) Fail(stage.name + ": spawn order total must be positive (otherwise modulo-by-zero during spawning).");
                if (serialized.FindProperty("spawnTerm").floatValue <= 0) Fail(stage.name + ": spawn interval must be positive.");
            }
            if (!Resources.Load("ScriptableObjects/Stages/" + stage.name)) Fail(stage.name + ": missing stage progression data.");
            stageCount++;
        }

        UnityEngine.Object player = Resources.Load("ScriptableObjects/PlayerData");
        if (!player) Fail("Missing PlayerData template.");
        else using (var serialized = new SerializedObject(player))
        {
            SerializedProperty deck = serialized.FindProperty("UnitNamesInDeck");
            if (deck.arraySize != 6) Fail("Default player deck must contain six slots.");
            for (int i = 0; i < deck.arraySize; i++)
            {
                string unit = deck.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(unit) && !Resources.Load<GameObject>((i < 4 ? "Prefabs/01Heroes/" : "Prefabs/02Soldiers/") + unit))
                    Fail("Default deck slot " + i + " has no unit prefab: " + unit);
            }
            SerializedProperty skills = serialized.FindProperty("equippedSkill");
            if (skills.arraySize != 3) Fail("Default player must equip three skills.");
            for (int i = 0; i < skills.arraySize; i++)
                if (!Resources.Load("ScriptableObjects/LunaSkills/" + skills.GetArrayElementAtIndex(i).stringValue))
                    Fail("Missing default skill: " + skills.GetArrayElementAtIndex(i).stringValue);
            int skin = serialized.FindProperty("SkinType").intValue;
            if (skin < 0 || skin > 6) Fail("Default player skin index is out of range.");
            string stage = serialized.FindProperty("SaveStageName").stringValue;
            if (!Resources.Load<GameObject>("Prefabs/10Stage/" + stage)) Fail("Missing default player stage: " + stage);
        }

        UnityEngine.Object uiRegistry = Resources.Load("UIAddressable");
        if (!uiRegistry) Fail("Missing production UIAddressable registry.");
        else using (var serialized = new SerializedObject(uiRegistry))
        {
            string groupDirectory = "Assets/AddressableAssetsData/AssetGroups";
            string groups = Directory.Exists(groupDirectory) ? string.Join("\n", Directory.GetFiles(groupDirectory, "*.asset").Select(File.ReadAllText)) : "";
            SerializedProperty entries = serialized.FindProperty("uiList");
            var seenTypes = new HashSet<int>();
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                SerializedProperty type = entry.FindPropertyRelative("type");
                string name = type.enumNames[type.enumValueIndex];
                if (!seenTypes.Add(type.intValue)) Fail("Duplicate UI registry type: " + name);
                string guid = entry.FindPropertyRelative("reference").FindPropertyRelative("m_AssetGUID").stringValue;
                string path = string.IsNullOrEmpty(guid) ? "" : AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) { Fail("UI " + name + " has no resolvable runtime prefab key: " + guid); continue; }
                if (!prefab.GetComponent<Canvas>()) Fail("UI " + name + " prefab has no Canvas: " + path);
                if (!groups.Contains("m_GUID: " + guid)) Fail("UI " + name + " is absent from Addressables groups: " + guid);
            }
            report.checks.Add("Resolved " + entries.arraySize + " UI runtime keys to prefabs and Addressables group entries.");
        }
        report.checks.Add("Checked " + stageCount + " stage spawn/progression records and default player deck, skills, skin and stage.");
    }

    private static void InspectDuplicateGuids()
    {
        var owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string root in new[] { "Assets", "Packages" })
        {
            if (!Directory.Exists(root)) continue;
            foreach (string path in Directory.EnumerateFiles(root, "*.meta", SearchOption.AllDirectories))
            {
                string guid = File.ReadLines(path).FirstOrDefault(line => line.StartsWith("guid: ", StringComparison.Ordinal));
                if (guid == null) continue;
                guid = guid.Substring(6).Trim();
                if (owners.TryGetValue(guid, out string previous)) Fail("Duplicate GUID " + guid + ": " + previous + " and " + path);
                else owners.Add(guid, path);
            }
        }
        report.checks.Add("Checked source Assets/Packages metadata for duplicate GUIDs.");
    }

    private static void CaptureLog(string message, string stackTrace, LogType type)
    {
        if (report == null || message.StartsWith("[LunaValidation]", StringComparison.Ordinal)) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            Fail(message + "\n" + stackTrace);
        else if (type == LogType.Warning && report.warnings.Count < 500 && !report.warnings.Contains(message))
            report.warnings.Add(message);
    }

    private static void Fail(string message)
    {
        if (!report.errors.Contains(message) && report.errors.Count < 500) report.errors.Add(message);
        Persist();
    }

    private static void Persist()
    {
        if (report == null) return;
        string json = JsonUtility.ToJson(report, true);
        SessionState.SetString(ReportKey, json);
        File.WriteAllText(Path.Combine(OutputDirectory, report.operation + ".json"), json);
    }

    private static void Complete()
    {
        SessionState.SetBool(BuildDrainKey, false);
        EditorApplication.update -= UpdateBuildDrain;
        Application.logMessageReceived -= CaptureLog;
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        report.finishedUtc = DateTime.UtcNow.ToString("O");
        report.success = report.errors.Count == 0;
        Persist();
        Debug.Log("[LunaValidation] " + report.operation + ": " + (report.success ? "PASS" : "FAIL") + "; errors=" + report.errors.Count + "; warnings=" + report.warnings.Count);
        if (Application.isBatchMode) EditorApplication.Exit(report.success ? 0 : 1);
    }
}

#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>Opt-in diagnostics for the native authoring rewrite. Never installs in a release player.</summary>
public sealed partial class LunaNativeValidation : MonoBehaviour
{
    [Serializable] public sealed class CaseResult
    {
        public string group, name, startedUtc, finishedUtc, detail;
        public bool success;
        public float seconds;
        public List<string> observations = new List<string>();
    }
    [Serializable] public sealed class RunReport
    {
        public string operation, startedUtc, finishedUtc, unityVersion, saveDirectory, section, filter;
        public bool success;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> expectedDiagnostics = new List<string>();
        public List<string> expectedWarnings = new List<string>();
        public List<CaseResult> cases = new List<CaseResult>();
    }
    public static bool IsRunning { get; private set; }
    public static bool Completed { get; private set; }
    private RunReport report;
    private CaseResult current;
    private string reportPath, section, filter;
    private float caseStarted;
    private int caseErrors;
    private string expectedErrorToken;
    private readonly List<string> expectedErrors = new List<string>();
    private readonly HashSet<int> fixtureRounds = new HashSet<int>();
    private int fixtureRound = -900000;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() { IsRunning = Completed = false; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (!Application.isEditor && !Debug.isDebugBuild) return;
        string mode = Environment.GetEnvironmentVariable("LUNA_VALIDATION_MODE");
        if (mode != "native-smoke" && mode != "native-matrix" && mode != "native-player") return;
        string save = Environment.GetEnvironmentVariable("LUNA_SAVE_DIRECTORY");
        string output = Environment.GetEnvironmentVariable("LUNA_VALIDATION_DIRECTORY");
        string path = Environment.GetEnvironmentVariable("LUNA_NATIVE_REPORT_PATH");
        if (string.IsNullOrWhiteSpace(save) || string.IsNullOrWhiteSpace(output) || string.IsNullOrWhiteSpace(path) ||
            !Path.IsPathRooted(save) || !Path.IsPathRooted(output) || !Path.IsPathRooted(path) ||
            !File.Exists(Path.Combine(save, ".luna-validation-isolated")))
            throw new InvalidOperationException("Native validation requires absolute report/output/save paths and an isolated-save marker.");
        string fullSave = Path.GetFullPath(save).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string realSaveRoot = Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        // Normal progress lives at persistentDataPath/Luna. Reject the whole persistent
        // tree, including that actual default, even if somebody placed a diagnostic marker there.
        if (string.Equals(fullSave, realSaveRoot, StringComparison.OrdinalIgnoreCase) ||
            fullSave.StartsWith(realSaveRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            fullSave.StartsWith(realSaveRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Native validation refuses the persistent player-data tree, including its Luna save directory.");
        var host = new GameObject("Luna native validation (opt-in)");
        DontDestroyOnLoad(host);
        host.AddComponent<LunaNativeValidation>().Configure(mode, Path.GetFullPath(path), fullSave);
    }

    private void Configure(string mode, string path, string save)
    {
        reportPath = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        section = Environment.GetEnvironmentVariable("LUNA_MATRIX_SECTION") ?? "all";
        filter = Environment.GetEnvironmentVariable("LUNA_MATRIX_FILTER") ?? string.Empty;
        report = new RunReport { operation = mode, startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion, saveDirectory = save, section = section, filter = filter };
        IsRunning = true;
        Application.logMessageReceived += CaptureLog;
        SaveReport();
        if (mode == "native-player" && Environment.GetEnvironmentVariable("LUNA_PLAYER_MODE") == "early")
        {
            // Configure runs BeforeSceneLoad; this callback observes all Awake calls
            // while the native Procedure's Start coroutine has not run yet.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += StartEarlyPlayerShutdown;
            return;
        }
        StartCoroutine(Guard(mode == "native-player" ? PlayerProbeWithShutdown(Environment.GetEnvironmentVariable("LUNA_PLAYER_MODE") ?? "diagnostics") : mode == "native-smoke" ? Smoke() : Matrix()));
    }

    private IEnumerator Guard(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(routine);
        while (stack.Count > 0 && report.errors.Count == 0)
        {
            object yielded = null;
            try
            {
                IEnumerator next = stack.Peek();
                if (!next.MoveNext())
                {
                    stack.Pop(); (next as IDisposable)?.Dispose();
                    // A completed child is not a frame yield. Resume its parent
                    // now so Until(predicate) and the following assertion are atomic.
                    continue;
                }
                else yielded = next.Current;
            }
            catch (Exception exception) { report.errors.Add(exception.ToString()); break; }
            if (yielded is IEnumerator nested) stack.Push(nested);
            else if (stack.Count > 0) yield return yielded;
        }
        while (stack.Count > 0)
        {
            try { (stack.Pop() as IDisposable)?.Dispose(); }
            catch (Exception exception) { report.errors.Add("Fixture disposal: " + exception); }
        }
        foreach (int round in fixtureRounds) LunaSpawn.CancelRound(round);
        if (current != null)
        {
            current.finishedUtc = DateTime.UtcNow.ToString("O");
            current.detail = "Interrupted; remaining cases were not executed.";
        }
        report.success = report.errors.Count == 0;
        report.finishedUtc = DateTime.UtcNow.ToString("O");
        IsRunning = false; Completed = true;
        SaveReport();
        if (!Application.isEditor)
        {
            LunaBootstrap.Quit(report.success ? 0 : 1);
            if (Environment.GetEnvironmentVariable("LUNA_PLAYER_MODE") == "early")
                LunaBootstrap.Quit(report.success ? 0 : 1);
        }
    }

    private void CaptureLog(string message, string stack, LogType type)
    {
        if (report == null) return;
        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            if (!string.IsNullOrEmpty(expectedErrorToken) && message.Contains(expectedErrorToken))
            { expectedErrors.Add(message); return; }
            report.errors.Add((current != null ? current.group + "/" + current.name : "startup") + ": " + message + "\n" + stack);
            if (Completed) { report.success = false; SaveReport(); }
        }
        else if (type == LogType.Warning)
        {
            report.warnings.Add(message);
            if (Completed) SaveReport();
        }
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= CaptureLog;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= StartEarlyPlayerShutdown;
    }
    private void SaveReport() => File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static T Active<T>() where T : UnityEngine.Object => FindAnyObjectByType<T>();
    private IEnumerator Until(Func<bool> condition, string label, float timeout = 60f)
    {
        float start = Time.realtimeSinceStartup;
        while (!condition())
        {
            if (Time.realtimeSinceStartup - start > timeout) throw new TimeoutException(label + " after " + timeout + "s");
            yield return null;
        }
    }
    private void BeginCase(string group, string name)
    {
        current = new CaseResult { group = group, name = name, startedUtc = DateTime.UtcNow.ToString("O") };
        report.cases.Add(current); caseStarted = Time.realtimeSinceStartup; caseErrors = report.errors.Count;
        Debug.Log("[LunaNativeValidation] " + group + "/" + name); SaveReport();
    }
    private void EndCase(string detail)
    {
        current.success = caseErrors == report.errors.Count; current.finishedUtc = DateTime.UtcNow.ToString("O");
        current.seconds = Time.realtimeSinceStartup - caseStarted; current.detail = detail;
        if (current.success) report.checks.Add(current.group + "/" + current.name + ": " + detail);
        SaveReport(); current = null;
    }
    private bool Includes(string group, string name) => (section == "all" || section == group || (section == "ranks" && group == "heroes")) &&
        (string.IsNullOrEmpty(filter) || name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
    private int NewFixtureRound() { int id = --fixtureRound; fixtureRounds.Add(id); return id; }
    private static EntityLogicBase[] Entities() => GameEntry.GetComponent<EntityComponent>().GetAllLoadedEntities()
        .Select(entity => entity.Logic).OfType<EntityLogicBase>().ToArray();
    private static void CheckOwnership()
    {
        var ids = new HashSet<int>();
        foreach (var actor in Entities())
        {
            Require(actor != null && actor.Available && !actor.IsHiding && actor.SpawnData != null, "Native entity has no live data owner.");
            Require(ids.Add(actor.ID) && actor.SpawnData.Id == actor.ID, "Duplicate/mismatched entity spawn identity.");
            Require(actor.gameObject.activeSelf, "A loaded native entity root is inactive.");
        }
        var forms = GameEntry.GetComponent<UIComponent>().GetAllLoadedUIForms();
        foreach (var form in FindObjectsByType<UILuna>(FindObjectsInactive.Exclude))
            Require(forms.Any(native => native.Logic == form), "UI form exists outside native UI ownership: " + form.name);
        Require(GameEntry.GetComponent<ProcedureComponent>().CurrentProcedure is LunaProcedure, "Native Procedure manager does not own Luna flow.");
        if (LunaScenes.CurrentScene != "Loading")
            Require(GameEntry.GetComponent<SceneComponent>().GetLoadedSceneAssetNames().Contains("Assets/Scenes/" + LunaScenes.CurrentScene + ".unity"), "Gameplay scene bypassed the native Scene component.");
    }
    private IEnumerator Boot()
    {
        BeginCase("boot", "loading-to-lobby");
        yield return Until(() => LunaBootstrap.IsReady && LunaRuntimeData.IsInitialized, "Native bootstrap");
        yield return Until(() =>
        {
            if (LunaScenes.CurrentScene == "Lobby" && Active<UILobby>() != null && Active<UILobby>().ProfileReady) return true;
            Active<UILoading>()?.ContinueToLobby(); return false;
        }, "Normal loading form continuation");
        Require(string.Equals(Path.GetFullPath(LunaRuntimeData.SaveDirectory).TrimEnd(Path.DirectorySeparatorChar), report.saveDirectory, StringComparison.OrdinalIgnoreCase), "Runtime save path escaped isolation.");
        Require(LunaSession.Current.AllStagePrefab.Count == 30 && LunaSession.Current.SelectStagePrefab != null, "Cold bootstrap did not initialize campaign selection.");
        var lobby = Active<UILobby>();
        var level = lobby.transform.Find("Luna/NameBackImage/LevelText").GetComponent<UnityEngine.UI.Text>();
        Require(!string.IsNullOrEmpty(level.text) && level.text.Contains("LV"), "Lobby profile level was not initialized.");
        for (int slot = 0; slot < 3; slot++)
        {
            var icon = lobby.transform.Find("Luna/LunaImage/Skill_0" + (slot + 1)).GetComponent<UnityEngine.UI.Image>();
            string key = LunaDataTable.Instance.playerData.equippedSkill[slot];
            Require(icon.sprite != null && icon.sprite == LunaDataTable.Instance.skiiTable[key].skillImage, "Lobby profile skill icon was not initialized: " + key);
        }
        for (int slot = 0; slot < 4; slot++)
            Require(lobby.transform.Find("Luna/LunaImage/EquipDesign_0" + slot).GetComponent<UnityEngine.UI.Image>().sprite != null, "Lobby equipment profile was only partially initialized.");
        CheckOwnership();
        EndCase("Native Procedure/Scene/UI bootstrap reached Lobby and used the marked isolated save directory.");
    }

    private IEnumerator Smoke()
    {
        yield return Boot();
        yield return SpawnOwnership();
        yield return FailureOwnership();
        CheckPersistence();
        CheckSaveReplacementLocks();
        BeginCase("round", "stage-entry-and-input");
        yield return LoadStage("Stage01-01");
        var round = LunaScenes.CurrentGame;
        var player = round.Player as PlayerLogic;
        Require(player != null && player.Abilities != null, "Round player did not initialize its owned ability controller.");
        Require(GameEntry.GetComponent<FsmComponent>().HasFsm<PlayerLogic>("LunaPlayer/" + player.ID), "Player has no native per-life FSM.");
        float startX = player.transform.position.x;
        player.Invincible(0f); player.SetMovement(-1);
        yield return new WaitForSeconds(0.35f);
        player.SetMovement(0);
        Require(player.transform.position.x < startX - 0.01f, "Public player movement request did not move left through native FSM.");
        Require(player.transform.position.x >= Mathf.Min(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x) - 0.01f, "Player movement escaped stage bounds.");
        CheckOwnership();
        EndCase("Campaign round initialized both towers, player FSM, owned abilities, active HUD, and bounded normal movement.");
        BeginCase("round", "death-respawn-and-same-scene-reload");
        int oldPlayerId = player.ID, oldRound = round.RoundId;
        player.stat.SpawnDelayTime = 0.2f; player.stat.Hp = 0f;
        yield return Until(() => round.PlayerRespawnPending || (round.Player != null && round.Player.ID != oldPlayerId), "Procedure consumed player death", 15f);
        yield return Until(() => round.Player != null && round.Player.ID != oldPlayerId && !round.PlayerRespawnPending, "Round-owned async player respawn", 20f);
        Require(LunaSpawn.Find<PlayerLogic>(oldPlayerId) == null, "Dead spawn identity still resolves after respawn.");
        Require(round.Player.Data.AllyTowerId != 0 && round.Player.Data.RoundId == oldRound, "Respawn lost tower or round ownership.");
        yield return LoadStage("Stage01-01");
        Require(LunaScenes.CurrentGame.RoundId != oldRound && !round.IsActive, "Same-scene reload retained its previous round.");
        yield return Until(() => !Entities().Any(entity => entity.SpawnData.RoundId == oldRound), "Previous round entities retired");
        CheckOwnership();
        EndCase("Normal death reached the Procedure respawn timer, fresh native identity, and same-scene reload retired the prior round.");
        BeginCase("round", "leave-to-lobby");
        int lastRound = LunaScenes.CurrentGame.RoundId;
        LunaScenes.Load("Lobby");
        yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning && Active<UILobby>() != null, "Battle exit to lobby");
        yield return Until(() => !Entities().Any(entity => entity.SpawnData.RoundId == lastRound), "Battle actor cleanup");
        Require(LunaScenes.CurrentGame == null, "Leaving Procedure retained its battle round.");
        var lobbyPlayer = UnitLogic.From(LunaSession.Current.LunaPlayer);
        Require(LunaSession.Current.LunaPlayer == null || (lobbyPlayer != null && lobbyPlayer.Data != null && lobbyPlayer.Data.RoundId == 0 && lobbyPlayer.Data.InitialMode == UnitSpawnMode.Lobby), "Lobby retained a stale battle player instead of its own native lobby actor.");
        CheckOwnership(); EndCase("Battle exit closed its round and native actors without a late respawn.");
    }

    private IEnumerator LoadStage(string key)
    {
        int previous = LunaScenes.CurrentGame != null ? LunaScenes.CurrentGame.RoundId : 0;
        LunaSession.Current.CurrentStageName = key; LunaSession.Current.SetStagePrefab(key); LunaScenes.Load("Game");
        yield return Until(() => LunaScenes.CurrentScene == "Game" && !LunaScenes.IsTransitioning && LunaScenes.CurrentGame != null && LunaScenes.CurrentGame.RoundId != previous, "New native battle procedure: " + key);
        yield return Until(() => LunaScenes.CurrentGame.FailureMessage != null || (LunaScenes.CurrentGame is BasicDungeon stage && stage.PhaseName == "Combat" && Active<UIPlayer>() != null && Active<UISkill>() != null), "Battle intro and HUD: " + key);
        var game = LunaScenes.CurrentGame as BasicDungeon;
        Require(game != null && game.FailureMessage == null && game.StageKey == key, "Wrong or failed battle: " + LunaScenes.CurrentGame?.FailureMessage);
        Require(game.gameAllyTower != null && game.gameEnemyTower != null && game.Player != null, "Campaign actor graph is incomplete.");
    }

    private IEnumerator SpawnOwnership()
    {
        BeginCase("entity", "typed-data-cancel-and-reuse");
        var prefab = Resources.LoadAll<GameObject>("Prefabs/01Heroes").OrderBy(asset => asset.name).First();
        var view = prefab.GetComponent<UnitView>();
        float sourceHp = view.Definition.Hp;
        int round = NewFixtureRound();
        var cancelled = HeroData.CreateFrom(view, new Vector3(0f, -50f));
        cancelled.RoundId = round; cancelled.InitialMode = UnitSpawnMode.Preview;
        int cancelledId = cancelled.Id, shownCount = 0;
        LunaSpawn.Show<HeroUnitLogic>(prefab, cancelled, shown => shownCount++);
        LunaSpawn.Cancel(cancelledId);
        yield return Until(() => LunaSpawn.PendingCount == 0, "Cancelled asynchronous native show");
        yield return null;
        Require(shownCount == 0 && LunaSpawn.Find<HeroUnitLogic>(cancelledId) == null, "Cancelled pending spawn reached its completion callback or stayed live.");
        HeroUnitLogic first = null;
        var data = HeroData.CreateFrom(view, new Vector3(1f, -50f));
        data.RoundId = round; data.InitialMode = UnitSpawnMode.Preview; data.Level = 3; data.Rank = eUltState.STAR5;
        int firstId = data.Id;
        LunaSpawn.Show<HeroUnitLogic>(prefab, data, actor => first = actor);
        yield return Until(() => first != null, "Typed HeroData show");
        Require(first.ID == firstId && first.Data.Level == 3 && first.ultState == eUltState.STAR5 && first.IsRankReady, "Pre-show level/rank/identity was not initialized before completion.");
        Require(first.Data.Definition != view.Definition && view.Definition.Hp == sourceHp, "Spawn mutated the authored object definition.");
        Vector3 authoredScale = prefab.transform.localScale;
        first.transform.localScale = authoredScale * 1.5f;
        first.SafeHide();
        yield return null;
        HeroUnitLogic second = null;
        var next = HeroData.CreateFrom(view, new Vector3(2f, -50f));
        next.RoundId = round; next.InitialMode = UnitSpawnMode.Preview; next.Rank = eUltState.STAR1;
        int secondId = next.Id;
        LunaSpawn.Show<HeroUnitLogic>(prefab, next, actor => second = actor);
        yield return Until(() => second != null, "Reused native hero show");
        Require(secondId != firstId && LunaSpawn.Find<HeroUnitLogic>(firstId) == null, "A stale entity ID resolves after reuse.");
        Require(second.ultState == eUltState.STAR1 && second.IsRankReady && Vector3.Distance(second.transform.localScale, authoredScale) < 0.001f, "Rank or authored scale leaked across native reuse.");
        LunaSpawn.CancelRound(round);
        yield return Until(() => LunaSpawn.PendingCount == 0 && !Entities().Any(entity => entity.SpawnData.RoundId == round), "Fixture round cancellation");
        EndCase("Pending cancellation suppressed completion; typed rank/level preceded OnShow completion; fresh ID and scale/rank reset after hide/show; authored SO remained unchanged.");
    }

    private void CheckPersistence()
    {
        BeginCase("save", "native-roundtrip-and-backup");
        var player = LunaDataTable.Instance.playerData;
        int originalGold = player.Gold;
        player.Gold = 876543;
        Require(LunaRuntimeData.SaveNow(), "Native save failed.");
        player.Gold = 1;
        Require(LunaRuntimeData.Reload() && player.Gold == 876543, "Native settings roundtrip failed.");
        Require(LunaRuntimeData.SaveNow(), "Backup seed failed.");
        byte[] validBackup = File.ReadAllBytes(Path.Combine(LunaRuntimeData.SaveDirectory, "LunaSave.dat.bak"));
        int warningStart = report.warnings.Count;
        File.WriteAllBytes(Path.Combine(LunaRuntimeData.SaveDirectory, "LunaSave.dat"), new byte[] { 255, 255, 255, 255, 15 });
        player.Gold = 2;
        Require(LunaRuntimeData.Reload() && player.Gold == 876543, "Malformed primary was accepted instead of the valid atomic backup.");
        player.Gold = originalGold; Require(LunaRuntimeData.SaveNow(), "Restoring isolated save failed.");
        Require(File.ReadAllBytes(Path.Combine(LunaRuntimeData.SaveDirectory, "LunaSave.dat.bak")).SequenceEqual(validBackup),
            "Saving after corrupt-primary recovery overwrote the last valid backup with corrupt bytes.");
        string[] recoveryWarnings = report.warnings.Skip(warningStart).ToArray();
        Require(recoveryWarnings.SequenceEqual(new[] { "Could not read Luna save: Setting count is outside the supported range.", "Recovered Luna progress from the backup save." }),
            "Malformed-primary recovery emitted unexpected or duplicate warnings.");
        report.expectedWarnings.AddRange(recoveryWarnings);
        EndCase("Runtime gold roundtrip and malformed-primary backup recovery passed in the isolated directory.");
    }

    private IEnumerator Matrix()
    {
        yield return Boot();
        int initialCases = report.cases.Count;
        if (Includes("ownership", "typed-data-cancel-and-reuse")) yield return SpawnOwnership();
        if (Includes("ownership", "callback-failure-disposal")) yield return FailureOwnership();
        if (Includes("ownership", "resource-failure-and-retry")) yield return ResourceFailureOwnership();
        if (Includes("ownership", "pending-player-loadout-snapshots")) yield return PlayerLoadoutOwnership();
        if (section == "all" || section == "heroes" || section == "ranks")
        {
            yield return HeroContracts();
            yield return HeroContracts(true);
            yield return HeroScheduleContracts();
        }
        if (section == "all" || section == "motion") yield return MotionContracts();
        if (section == "all" || section == "buffs")
        {
            yield return BuffContracts();
            yield return CrossRoundPartyContracts();
            yield return GearOrderContracts();
        }
        if (section == "all" || section == "skins") yield return SkinContracts();
        if (section == "all" || section == "skills") yield return PlayerSkillContracts();
        if (section == "all" || section == "skill-levels") yield return SkillLevelContracts();
        if (section == "all" || section == "actors") yield return ActorContracts();
        if (section == "all" || section == "projectiles") yield return ProjectileContracts();
        if (section == "all" || section == "rewards") yield return RewardContracts();
        if (section == "all" || section == "combos") yield return CombinationContracts();
        if (section == "all" || section == "dungeons") yield return DungeonContracts();
        if (section == "all" || section == "stages")
            foreach (var stage in LunaSession.Current.AllStagePrefab.OrderBy(asset => asset.name))
            {
                if (!Includes("stages", stage.name)) continue;
                BeginCase("stages", stage.name); yield return LoadStage(stage.name);
                yield return new WaitForSeconds(1f); CheckOwnership();
                EndCase("Native Procedure created the selected campaign round, completed intro, and initialized both towers/player/HUD.");
            }
        if (section == "all" || section == "ui") yield return UiContracts();
        if (section == "all" || section == "lifecycle") yield return HudContracts();
        if (section != "all" && section != "stages" && section != "ownership" && section != "heroes" && section != "ranks" && section != "motion" && section != "buffs" && section != "skins" && section != "skills" && section != "skill-levels" && section != "actors" && section != "projectiles" && section != "rewards" && section != "combos" && section != "dungeons" && section != "ui" && section != "lifecycle")
            throw new InvalidOperationException("Native matrix section has not been implemented yet: " + section);
        Require(report.cases.Count > initialCases, "No content cases matched section=" + section + ", filter=" + filter + "; an empty selection cannot pass.");
    }
}
#endif

#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EverydayDevup.Framework;
using GameFramework;
using GameFramework.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Explicitly enabled development diagnostics; never runs in a release player.</summary>
public sealed partial class LunaRuntimeValidation : MonoBehaviour
{
    [Serializable] public sealed class CaseResult
    {
        public string group, name, startedUtc, finishedUtc, detail;
        public bool success;
        public int frames, entitiesBefore, entitiesAfter;
        public float seconds;
        public List<string> observations = new List<string>();
    }
    [Serializable] public sealed class RunReport
    {
        public string operation, mode, startedUtc, finishedUtc, unityVersion, saveDirectory;
        public bool success;
        public int runtimeFrames;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<CaseResult> cases = new List<CaseResult>();
    }
    private RunReport report;
    private CaseResult current;
    private string reportPath, outputDirectory, section, filter;
    private float caseStarted;
    private int caseFrame, caseErrorCount;
    private bool completed;
    private HashSet<GameObject> fixtureObjects;
    private Status allyTarget, enemyTarget;
    private Status[] protectedFixtureRoots;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (!Application.isEditor && !Debug.isDebugBuild) return;
        string mode = Environment.GetEnvironmentVariable("LUNA_VALIDATION_MODE");
        if (string.IsNullOrEmpty(mode)) return;
        string save = Environment.GetEnvironmentVariable("LUNA_SAVE_DIRECTORY");
        string output = Environment.GetEnvironmentVariable("LUNA_VALIDATION_DIRECTORY");
        // Require an explicitly prepared, dedicated diagnostic folder. Never borrow a user's save.
        if (string.IsNullOrWhiteSpace(save) || string.IsNullOrWhiteSpace(output) ||
            !Path.IsPathRooted(save) || !Path.IsPathRooted(output) ||
            !File.Exists(Path.Combine(save, ".luna-validation-isolated")))
            throw new InvalidOperationException("Runtime validation requires absolute output/save paths and an isolated-save marker.");
        var host = new GameObject("Luna development validation");
        DontDestroyOnLoad(host);
        var driver = host.AddComponent<LunaRuntimeValidation>();
        driver.Configure(mode, Path.GetFullPath(output), Path.GetFullPath(save));
    }

    private void Configure(string mode, string output, string save)
    {
        outputDirectory = output;
        Directory.CreateDirectory(outputDirectory);
        reportPath = Path.Combine(output, mode == "content-matrix" ? "content-matrix.json" : "player-" + mode + ".json");
        section = Environment.GetEnvironmentVariable("LUNA_MATRIX_SECTION") ?? "all";
        filter = Environment.GetEnvironmentVariable("LUNA_MATRIX_FILTER") ?? "";
        report = new RunReport { operation = mode == "content-matrix" ? mode : "player-" + mode,
            mode = mode, startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion, saveDirectory = save };
        Application.logMessageReceived += CaptureLog;
        SaveReport();
        StartCoroutine(Guard(mode == "content-matrix" ? Matrix() : PlayerProbe(mode)));
    }

    private IEnumerator Guard(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0)
        {
            object yielded = null;
            Exception failure = null;
            try
            {
                IEnumerator top = stack.Peek();
                if (!top.MoveNext()) stack.Pop();
                else yielded = top.Current;
            }
            catch (Exception ex) { failure = ex; }
            if (failure != null) { report.errors.Add(failure.ToString()); break; }
            if (report.mode == "content-matrix" && report.errors.Count > 0) break;
            if (yielded is IEnumerator nested) stack.Push(nested);
            else if (stack.Count > 0) yield return yielded;
        }
        if (current != null && !current.success)
        {
            current.finishedUtc = DateTime.UtcNow.ToString("O");
            current.detail = "Interrupted; see errors. Remaining planned cases were not executed.";
        }
        report.success = report.errors.Count == 0;
        report.finishedUtc = DateTime.UtcNow.ToString("O");
        report.runtimeFrames = Time.frameCount;
        completed = true;
        SaveReport();
        if (!Application.isEditor) Application.Quit(report.success ? 0 : 1);
    }

    private void CaptureLog(string message, string stack, LogType type)
    {
        if (report == null) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            report.errors.Add((current == null ? "startup" : current.group + "/" + current.name) + ": " + message + "\n" + stack);
            if (completed) { report.success = false; SaveReport(); }
        }
        else if (type == LogType.Warning && report.warnings.Count < 1000) report.warnings.Add(message);
    }
    private void OnDestroy() { Application.logMessageReceived -= CaptureLog; }
    private void SaveReport() { File.WriteAllText(reportPath, JsonUtility.ToJson(report, true)); }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static T Active<T>() where T : UnityEngine.Object => FindAnyObjectByType<T>();
    private IEnumerator Until(Func<bool> condition, string label, float timeout = 60f)
    {
        float start = Time.realtimeSinceStartup;
        while (!condition())
        {
            if (Time.realtimeSinceStartup - start > timeout) throw new TimeoutException(label);
            yield return null;
        }
    }
    private void BeginCase(string group, string name)
    {
        current = new CaseResult { group = group, name = name, startedUtc = DateTime.UtcNow.ToString("O"), entitiesBefore = LunaEntities.ActiveCount };
        report.cases.Add(current); caseStarted = Time.realtimeSinceStartup; caseFrame = Time.frameCount; caseErrorCount = report.errors.Count;
        Debug.Log("[LunaMatrix] " + group + "/" + name);
        SaveReport();
    }
    private void EndCase(string detail)
    {
        current.success = report.errors.Count == caseErrorCount; current.detail = detail; current.finishedUtc = DateTime.UtcNow.ToString("O");
        current.seconds = Time.realtimeSinceStartup - caseStarted; current.frames = Time.frameCount - caseFrame;
        current.entitiesAfter = LunaEntities.ActiveCount;
        SaveReport(); current = null;
    }
    private bool Includes(string group, string name) => (section == "all" || section == group) &&
        (string.IsNullOrEmpty(filter) || name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);

    private IEnumerator Boot()
    {
        yield return Until(() => LunaEntities.IsReady && LunaRuntimeData.IsInitialized, "Framework initialization");
        yield return Until(() =>
        {
            if (SceneManager.GetActiveScene().name == "Lobby" && Active<UILobby>()) return true;
            var loading = Active<UILoading>(); if (loading) loading.ContinueToLobby();
            return false;
        }, "Loading to lobby");
        yield return new WaitForSecondsRealtime(1f);
        Require(Path.GetFullPath(LunaRuntimeData.SaveDirectory) == report.saveDirectory, "Runtime used a different save directory.");
        report.checks.Add("Normal Loading -> Lobby bootstrap; isolated save marker/path verified.");
    }
    private static GameObject[] NativeObjects()
    {
        var component = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.EntityComponent>();
        return component.GetAllLoadedEntities().Select(e => (GameObject)e.Handle).ToArray();
    }
    private static void CheckOwnership()
    {
        GameObject[] native = NativeObjects();
        Require(native.Length == LunaEntities.ActiveCount, "Native/tracked Entity counts differ.");
        foreach (GameObject go in native) Require(go && go.activeSelf, "Native Entity retained a destroyed/inactive root.");
        foreach (LunaEntityBehaviour module in FindObjectsByType<LunaEntityBehaviour>(FindObjectsInactive.Exclude))
            if (module.isActiveAndEnabled) Require(LunaEntities.IsSpawned(module.gameObject), "Unowned gameplay module: " + module.name);
    }
    private IEnumerator LoadStage(string stageName, float combatSeconds = 2f)
    {
        Scene before = SceneManager.GetActiveScene();
        Game.Instance.dataManager.CurrentStageName = stageName;
        Game.Instance.dataManager.SetStagePrefab(stageName);
        Game.Instance.lunaSceneManager.LoadScene("Game");
        yield return Until(() => SceneManager.GetActiveScene() != before && SceneManager.GetActiveScene().name == "Game", "Scene entry " + stageName);
        yield return Until(() => Active<BasicDungeon>() && !Active<BasicDungeon>().isStageStop && Active<UIPlayer>() && Active<UISkill>(), "Stage HUD/combat initialization " + stageName);
        yield return new WaitForSecondsRealtime(combatSeconds);
        BasicDungeon[] stages = FindObjectsByType<BasicDungeon>(FindObjectsInactive.Exclude);
        Require(stages.Length == 1 && stages[0].name.Replace("(Clone)", "") == stageName, "Wrong/duplicate stage after entry.");
        Require(stages[0].gameAllyTower && stages[0].gameEnemyTower && Game.Instance.dataManager.LunaPlayer, "Stage actors missing.");
        CheckOwnership();
    }
    private IEnumerator Matrix()
    {
        yield return Boot();
        string[] stages = Resources.LoadAll<GameObject>("Prefabs/10Stage").Where(p => p.GetComponent<BasicDungeon>()).Select(p => p.name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Require(stages.Length == 30, "Expected the known 30 production stages; update matrix expectation deliberately if content changes.");
        foreach (string stage in stages)
        {
            if (!Includes("stages", stage)) continue;
            BeginCase("stages", stage);
            yield return LoadStage(stage);
            EndCase("Normal scene entry, complete intro, instantiated active player/skill HUD, both towers, live combat >=2 seconds and native Entity ownership.");
        }
        if (section == "all" || section == "skins") yield return PlayerSkins();
        if (section == "all" || section == "actors" || section == "skills" || section == "projectiles" || section == "rewards" || section == "combos" || section == "ranks" || section == "lifecycle")
        {
            yield return LoadStage("Stage01-01");
            yield return MakeFixture();
        }
        if (section == "all" || section == "ranks")
        {
            if (Includes("ranks", "authored-hero-thresholds"))
            {
                BeginCase("ranks", "authored-hero-thresholds");
                yield return LunaHeroRankRegression.Validate(current.observations.Add);
                yield return LunaHeroRankRegression.ValidateJoeReuse(current.observations.Add);
                EndCase("Authored Lene/Ria/Lena/Scarlet rank boundaries exercised with real gameplay callbacks and restored runtime fixtures.");
            }
        }
        if (section == "all" || section == "actors") yield return Actors();
        if (section == "all" || section == "lifecycle") yield return InterruptedDamageLabel();
        if (section == "all" || section == "projectiles") yield return Projectiles();
        if (section == "all" || section == "skills") yield return PlayerSkills();
        if (section == "all" || section == "rewards") yield return EndlessReward();
        if (section == "all" || section == "combos" || section == "ranks") yield return ConcurrentFights();
        Require(report.cases.Count > 0, "The selected matrix section/filter matched no cases.");
        report.checks.Add("Finite matrix only; case records are the exact executed coverage. Passive/no-op skill observations are not treated as proven damage behavior.");
    }
    private IEnumerator PlayerSkins()
    {
        PlayerData player = LunaDataTable.Instance.playerData;
        foreach (PlayerData.eLunaSkin skin in Enum.GetValues(typeof(PlayerData.eLunaSkin)))
        {
            if (!Includes("skins", skin.ToString())) continue;
            BeginCase("skins", skin.ToString());
            player.SkinType = skin; player.SetCurPlayer();
            yield return LoadStage("Stage01-01");
            var actor = Game.Instance.dataManager.LunaPlayer;
            Require(actor.name.StartsWith(player.CurPlayer().name, StringComparison.Ordinal), "Wrong player prefab for skin.");
            if (skin == PlayerData.eLunaSkin.Dragon)
            {
                DragonCompanion(actor);
                CheckDragonAuthoredPose(actor, player.CurPlayer());
            }
            LunaHeroRankRegression.ValidateEquipmentSlots(Active<UISkill>());
            var equipment = (List<UISkill.SkillInfo>)typeof(UISkill).GetField("equipDesignSkillDatas", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Active<UISkill>());
            Require(equipment.Select(i => i.skillButton).Distinct().Count() == equipment.Count, "Equipment skills overlap the same UI slot.");
            foreach (var item in equipment)
            {
                if (item.skillData.type == SkillData.SkillType.Passive)
                    Require(item.completePassive, "Authored passive equipment callback did not complete: " + item.skillData.name);
                current.observations.Add("Equipment " + item.skillData.name + ": type=" + item.skillData.type + ", passiveCompleted=" + item.completePassive);
            }
            Status status = actor.GetComponent<Status>();
            CheckStatus(status);
            status.stat.SpawnDelayTime = 0; status.stat.Hp = 0;
            yield return Until(() => Game.Instance.dataManager.LunaPlayer && Game.Instance.dataManager.LunaPlayer.activeInHierarchy &&
                Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().stat.Hp > 0, "Skin death/respawn " + skin);
            yield return null;
            Require(FindObjectsByType<LunaStateMachine>(FindObjectsInactive.Exclude).Length == 1, "Duplicate skin player after respawn.");
            CheckStatus(Game.Instance.dataManager.LunaPlayer.GetComponent<Status>());
            if (skin == PlayerData.eLunaSkin.Dragon)
            {
                GameObject revived = Game.Instance.dataManager.LunaPlayer;
                DragonCompanion(revived);
                CheckDragonAuthoredPose(revived, player.CurPlayer());
                yield return LoadStage("Stage01-02");
                GameObject reentered = Game.Instance.dataManager.LunaPlayer;
                Require(reentered == revived, "Dragon scene hop did not exercise the reusable player instance.");
                DragonCompanion(reentered);
                CheckDragonAuthoredPose(reentered, player.CurPlayer());
                current.observations.Add("Dragon pet survived actual player death and Game -> Game reuse: exactly one live native-owned pet, valid finite stats/skeleton, and cached authored spawn pose matches the source prefab. AI may move the live pet; live post-combat coordinates are not compared. Pet identity may change; player identity reused.");
            }
            CheckOwnership(); EndCase("Actual skin stage entry, zero-HP death, zero-delay real respawn, single active player and native ownership.");
        }
        player.SkinType = PlayerData.eLunaSkin.Basic; player.SetCurPlayer();
    }
    private static GameObject DragonCompanion(GameObject player)
    {
        var controller = player.GetComponent<LunaCtrl_Dragon>();
        GameObject pet = controller ? controller.DragonPet : null;
        Require(pet && pet.activeInHierarchy && pet.transform.IsChildOf(player.transform), "Dragon companion is missing, inactive or detached.");
        Require(LunaEntities.IsSpawned(pet), "Dragon companion has no native Entity owner.");
        Require(player.GetComponentsInChildren<UnitAI>(true).Length == 1, "Dragon player has duplicate or missing pet AI.");
        CheckStatus(pet.GetComponent<Status>());
        var skeleton = pet.transform.Find("Sprite").GetComponent<Spine.Unity.SkeletonAnimation>();
        Require(skeleton && skeleton.Skeleton != null && skeleton.AnimationState != null, "Dragon companion skeleton did not initialize.");
        return pet;
    }
    private static void CheckDragonAuthoredPose(GameObject actor, GameObject source)
    {
        LunaCtrl_Dragon controller = actor.GetComponent<LunaCtrl_Dragon>();
        Transform authored = source.GetComponent<LunaCtrl_Dragon>().DragonPet.transform;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Vector3 position = (Vector3)typeof(LunaCtrl_Dragon).GetField("petLocalPosition", flags).GetValue(controller);
        Quaternion rotation = (Quaternion)typeof(LunaCtrl_Dragon).GetField("petLocalRotation", flags).GetValue(controller);
        Vector3 scale = (Vector3)typeof(LunaCtrl_Dragon).GetField("petLocalScale", flags).GetValue(controller);
        Require((position - authored.localPosition).sqrMagnitude < 0.0001f && Quaternion.Angle(rotation, authored.localRotation) < 0.01f &&
            (scale - authored.localScale).sqrMagnitude < 0.0001f,
            "Dragon cached spawn pose differs from authored prefab: expected position=" + authored.localPosition.ToString("F4") +
            ", rotation=" + authored.localRotation.ToString("F4") + ", scale=" + authored.localScale.ToString("F4") +
            "; actual position=" + position.ToString("F4") + ", rotation=" + rotation.ToString("F4") + ", scale=" + scale.ToString("F4"));
    }

    private IEnumerator InterruptedDamageLabel()
    {
        if (!Includes("lifecycle", "damage-label-interrupted-hide")) yield break;
        BeginCase("lifecycle", "damage-label-interrupted-hide");
        GameObject prefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
        GameObject actor = LunaEntities.Spawn(prefab, new Vector3(-2, 0, 0), Quaternion.identity, null, true);
        yield return null; yield return null;
        Status status = actor.GetComponent<Status>(); CheckStatus(status); status.unitState.IsLobbyMove = true;
        status.Attack(enemyTarget.gameObject, 1f);
        DamageTextControl label = actor.GetComponentInChildren<DamageTextControl>();
        Require(label, "Real damage did not create its floating damage label.");
        yield return null;
        Require(label && LunaEntities.IsSpawned(label.gameObject), "Damage label was not adopted before interrupted hide.");
        LunaEntities.Hide(actor);
        Require(!label || !LunaEntities.IsSpawned(label.gameObject), "Hiding an actor left its generated damage label registered.");
        yield return null; yield return null;
        GameObject reused = LunaEntities.Spawn(prefab, new Vector3(-2, 0, 0), Quaternion.identity, null, true);
        yield return null;
        Require(reused.GetComponentsInChildren<DamageTextControl>(true).Length == 0, "Old damage label reappeared on pooled actor reuse.");
        yield return ClearFixtureAdditions();
        EndCase("A real damage hit generated/adopted a label; actor hide retired it before its normal timeout, and reuse contained no stale label.");
    }
    private IEnumerator MakeFixture()
    {
        BasicDungeon stage = Active<BasicDungeon>();
        stage.StopAllCoroutines(); stage.isStageStop = true;
        var keep = new HashSet<GameObject> { stage.gameAllyTower, stage.gameEnemyTower, Game.Instance.dataManager.LunaPlayer };
        foreach (Status status in FindObjectsByType<Status>(FindObjectsInactive.Exclude))
            if (!keep.Contains(status.gameObject)) LunaEntities.Hide(status.gameObject);
        protectedFixtureRoots = keep.Select(actor => actor.GetComponent<Status>()).ToArray();
        foreach (Status status in protectedFixtureRoots)
        {
            status.unitState.IsLobbyMove = true;
            status.Invincible(0f);
            Require(status.stat.AttackAvoidance >= 200f && status.stat.SkillAvoidance >= 200f && status.stat.DebuffAvoidance >= 200f, "Fixture root protection did not apply.");
        }
        report.checks.Add("Controlled content fixture protects only retained player/towers with the existing permanent invincibility effect. Opposing disposable targets remain damageable; actor skill checks do not measure full-stage balance.");
        GameObject allyPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
        GameObject enemyPrefab = Resources.LoadAll<GameObject>("Prefabs/04Monsters").First(p => p.GetComponent<Status>() && p.GetComponent<UnitAI>());
        allyTarget = LunaEntities.Spawn(allyPrefab, new Vector3(-0.5f, 0, 0), Quaternion.identity, null, true).GetComponent<Status>();
        enemyTarget = LunaEntities.Spawn(enemyPrefab, new Vector3(0.5f, 0, 0), Quaternion.identity, null, true).GetComponent<Status>();
        yield return null; yield return null;
        ResetTargets(); fixtureObjects = new HashSet<GameObject>(NativeObjects());
    }
    private void ResetTargets()
    {
        foreach (Status status in new[] { allyTarget, enemyTarget })
        {
            Require(status, "Controlled target disappeared.");
            status.StopAllCoroutines(); status.ResetRespwanData();
            status.unitState.IsLobbyMove = true; status.stat.Hp = 100000f; status.stat.Armour = 0f;
        }
    }
    private IEnumerator ClearFixtureAdditions()
    {
        foreach (Status root in protectedFixtureRoots)
            Require(root && root.gameObject.activeInHierarchy && LunaEntities.IsSpawned(root.gameObject) && root.stat.Hp > 0f && !root.unitState.IsDeath,
                "A protected fixture root died or was retired before cleanup; pending respawn effects cannot be cleared safely.");
        foreach (GameObject actor in NativeObjects()) if (!fixtureObjects.Contains(actor)) LunaEntities.Hide(actor);
        yield return null; yield return null;
        ResetTargets();
    }
    private static void CheckStatus(Status status)
    {
        Require(status && status.IsStatusDataLoadComplete, "Combat status never initialized.");
        Require(status.stat.Hp > 0f && status.ProjecttileArrivalLocation, "Actor has no HP or projectile arrival anchor.");
        foreach (FieldInfo field in typeof(Stat).GetFields(BindingFlags.Public | BindingFlags.Instance))
            if (field.FieldType == typeof(float)) { float value = (float)field.GetValue(status.stat); Require(!float.IsNaN(value) && !float.IsInfinity(value), "Non-finite stat " + field.Name); }
    }
    private static string NumericSnapshot(object value) => string.Join("|", value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Where(f => f.FieldType.IsPrimitive || f.FieldType.IsEnum).OrderBy(f => f.Name, StringComparer.Ordinal)
        .Select(f => f.Name + "=" + Convert.ToString(f.GetValue(value), System.Globalization.CultureInfo.InvariantCulture)));
    private static string Coefficients(Status status) => NumericSnapshot(status.coefficient);
    private static string SkillSnapshot(Status status)
    {
        return NumericSnapshot(status.stat) + NumericSnapshot(status.coefficient) + status.transform.localScale +
            string.Join("|", typeof(Status).GetFields(BindingFlags.Instance | BindingFlags.Public).Where(f => f.FieldType == typeof(bool)).Select(f => f.Name + "=" + f.GetValue(status)));
    }
    private static bool HasSkill(GameObject actor) => actor.GetComponents<LunaEntityBehaviour>().Any(c => c is HeroSkillControl ||
        c.GetType().GetMethod("Skill", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null);
    private IEnumerator ExerciseActor(GameObject actor, string label)
    {
        yield return null; yield return null; yield return new WaitForSecondsRealtime(0.25f);
        Status status = actor.GetComponent<Status>(); CheckStatus(status);
        status.unitState.IsLobbyMove = true;
        status.AttackTarget = status.UnitSide == eUnitSide.Enemy ? allyTarget.gameObject : enemyTarget.gameObject;
        if (HasSkill(actor))
        {
            string before = SkillSnapshot(status) + SkillSnapshot(allyTarget) + SkillSnapshot(enemyTarget);
            int entities = LunaEntities.ActiveCount;
            status.unitState.IsSkillActivaion = true;
            yield return new WaitForSecondsRealtime(3.2f);
            bool observable = before != SkillSnapshot(status) + SkillSnapshot(allyTarget) + SkillSnapshot(enemyTarget) || entities != LunaEntities.ActiveCount;
            current.observations.Add(label + ": skill flag triggered; " + (observable ? "damage/stat/buff/summon change observed" : "no sampled effect (passive/rank-dependent path; effect not proven)"));
        }
        CheckStatus(status); Require(LunaEntities.IsSpawned(actor), "Actor vanished during activation: " + actor.name);
    }
    private IEnumerator Actors()
    {
        string[] folders = { "Prefabs/01Heroes", "Prefabs/02Soldiers", "Prefabs/02SoldierSommon", "Prefabs/03EliteMonters", "Prefabs/04Monsters" };
        foreach (string folder in folders)
        foreach (GameObject prefab in Resources.LoadAll<GameObject>(folder).Where(p => p.GetComponent<Status>()).OrderBy(p => p.name, StringComparer.Ordinal))
        {
            int[] ranks = prefab.GetComponent<HeroSkillControl>() ? new[] { 1, 7 } : new[] { 1 };
            foreach (int rank in ranks)
            {
                string name = folder + "/" + prefab.name + "/rank-" + rank;
                if (!Includes("actors", name)) continue;
                BeginCase("actors", name);
                if (rank == 1 && prefab.GetComponent<Joe>())
                {
                    current.observations.Add("Cross-rank warmup: prime the actual reusable Joe with repeated ULT1 initialization before testing STAR1 coefficients.");
                    yield return LunaHeroRankRegression.ValidateJoeReuse(current.observations.Add);
                }
                ObjectData data = prefab.GetComponent<Status>().objectData;
                eUltState original = data.ultState; data.ultState = (eUltState)rank;
                Vector3 pos = data.UnitSide == eUnitSide.Enemy ? new Vector3(1.2f, 0, 0) : new Vector3(-1.2f, 0, 0);
                GameObject cold = LunaEntities.Spawn(prefab, pos, prefab.transform.rotation);
                yield return ExerciseActor(cold, "fresh transient");
                yield return ClearFixtureAdditions();
                GameObject pooled = LunaEntities.Spawn(prefab, pos, prefab.transform.rotation, null, true);
                yield return null; yield return null; yield return new WaitForSecondsRealtime(0.25f);
                CheckStatus(pooled.GetComponent<Status>());
                string coefficient = Coefficients(pooled.GetComponent<Status>());
                string coefficientContext = ActorRankContext(pooled);
                MartinKing chargingBoss = pooled.GetComponent<MartinKing>();
                FieldInfo chargeField = chargingBoss ? typeof(MartinKing).GetField("isCharge", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                if (chargingBoss)
                {
                    Status chargeStatus = pooled.GetComponent<Status>();
                    chargeStatus.unitState.IsLobbyMove = true;
                    chargeStatus.AttackTarget = allyTarget.gameObject;
                    chargeStatus.unitState.IsSkillActivaion = true;
                    float chargeDeadline = Time.realtimeSinceStartup + 2f;
                    while (chargingBoss && !(bool)chargeField.GetValue(chargingBoss) && Time.realtimeSinceStartup < chargeDeadline)
                        yield return null;
                    Require(chargingBoss && (bool)chargeField.GetValue(chargingBoss), "MartinKing did not begin its actual charge within two seconds: skillFlag=" +
                        chargeStatus.unitState.IsSkillActivaion + "; moduleEnabled=" + (chargingBoss && chargingBoss.isActiveAndEnabled) +
                        "; actorActive=" + pooled.activeInHierarchy + "; initialized=" + chargeStatus.IsStatusDataLoadComplete + "; nativeOwned=" + LunaEntities.IsSpawned(pooled));
                    pooled.transform.Find("Sprite").gameObject.SetActive(false); // Interrupted Boom presentation.
                }
                pooled.transform.localScale *= 1.5f;
                LunaEntities.Hide(pooled);
                if (chargingBoss) Require(CountSpineCallbacks(pooled, "Complete", chargingBoss) == 0, "Hidden MartinKing retained its animation callback.");
                yield return null; yield return null;
                GameObject reused = LunaEntities.Spawn(prefab, pos, prefab.transform.rotation, null, true);
                yield return null; yield return null; yield return new WaitForSecondsRealtime(0.25f);
                Require(reused.GetComponents<LunaEntityBehaviour>().Any(m => !m.IsFirstEntityShow), "Native pool did not reuse an initialized instance.");
                Require((reused.transform.localScale - prefab.transform.localScale).sqrMagnitude < 0.0001f, "Interrupted growth survived reuse.");
                string reusedCoefficient = Coefficients(reused.GetComponent<Status>());
                Require(reusedCoefficient == coefficient, "Rank coefficients changed across hide/reuse: before=[" + coefficient + "]; after=[" + reusedCoefficient +
                    "]; before context=[" + coefficientContext + "]; after context=[" + ActorRankContext(reused) + "]");
                if (chargingBoss)
                {
                    MartinKing reusedBoss = reused.GetComponent<MartinKing>();
                    Require(!(bool)chargeField.GetValue(reusedBoss) && reused.transform.Find("Sprite").gameObject.activeSelf &&
                        !reused.GetComponent<Status>().unitState.IsTargetingSkill && CountSpineCallbacks(reused, "Complete", reusedBoss) == 1,
                        "MartinKing reuse retained charge/hidden sprite/targeting state or duplicated its completion callback.");
                }
                yield return ExerciseActor(reused, "native pool reuse");
                if (chargingBoss)
                {
                    Require((bool)chargeField.GetValue(reused.GetComponent<MartinKing>()), "Reused MartinKing did not accept a second skill charge.");
                    current.observations.Add("MartinKing actual charge interrupted by hide; hidden callback count 0, reuse callback count 1, charge/targeting/sprite reset, and second charge accepted. The 60-second terminal Boom is outside this short activation check.");
                }
                CheckOwnership();
                yield return ClearFixtureAdditions(); data.ultState = original;
                EndCase("Fresh initialization and actual pool hide/reuse; finite stats, rank coefficients, authored scale and live skill execution checked. Controlled opposing targets are used.");
            }
        }
    }

    private static string ActorRankContext(GameObject actor)
    {
        Status status = actor.GetComponent<Status>();
        string context = "rank=" + status.ultState + "; dataRank=" + status.objectData.ultState + "; lobbyPaused=" + status.unitState.IsLobbyMove +
            "; target=" + (status.AttackTarget ? status.AttackTarget.name : "none");
        Joe joe = actor.GetComponent<Joe>();
        if (joe)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            context += "; Joe.triggerSkill_1=" + typeof(Joe).GetField("triggerSkill_1", flags).GetValue(joe) +
                "; Joe.triggerSkill_2=" + typeof(Joe).GetField("triggerSkill_2", flags).GetValue(joe);
        }
        return context;
    }

    private static int CountSpineCallbacks(GameObject actor, string eventName, object target)
    {
        var animation = actor.transform.Find("Sprite").GetComponent<Spine.Unity.SkeletonAnimation>().AnimationState;
        var field = typeof(Spine.AnimationState).GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
        Require(field != null, "Cannot inspect Spine callback ownership: " + eventName);
        var callbacks = field.GetValue(animation) as Delegate;
        return callbacks == null ? 0 : callbacks.GetInvocationList().Count(callback => ReferenceEquals(callback.Target, target));
    }

    private IEnumerator Projectiles()
    {
        foreach (GameObject prefab in Resources.LoadAll<GameObject>("Prefabs").Where(p => p.GetComponent<Projectile>() || p.GetComponent<Arrow>()).OrderBy(p => p.name, StringComparer.Ordinal))
        {
            if (!Includes("projectiles", prefab.name)) continue;
            BeginCase("projectiles", prefab.name);
            for (int iteration = 0; iteration < 2; iteration++)
            {
                ResetTargets(); float hp = enemyTarget.stat.Hp;
                GameObject shot = LunaEntities.Spawn(prefab, enemyTarget.ProjecttileArrivalLocation.transform.position + Vector3.left * 0.1f, prefab.transform.rotation, null, true);
                Projectile projectile = shot.GetComponent<Projectile>();
                if (projectile)
                {
                    Require(!projectile.criticalShot && !projectile.skillShot && !projectile.isStun && projectile.StunTime == 0, "Projectile carried prior hit flags into spawn.");
                    projectile.SetProjectileState(allyTarget.gameObject, enemyTarget.gameObject, 100f, iteration == 0, 0.2f);
                    projectile.criticalShot = iteration == 0; projectile.skillShot = iteration == 0;
                    yield return Until(() => !LunaEntities.IsSpawned(shot), "Projectile impact/release " + prefab.name, 10f);
                    Require(enemyTarget.stat.Hp < hp, "Projectile did not damage its real target.");
                }
                else
                {
                    Arrow arrow = shot.GetComponent<Arrow>(); arrow.ShooterObject = allyTarget.gameObject;
                    arrow.PathEnd = enemyTarget.transform.position;
                    yield return new WaitForSecondsRealtime(0.3f);
                    Require(shot && LunaEntities.IsSpawned(shot), "Arrow did not survive caller parameter binding.");
                    arrow.DestroyArrow();
                }
                yield return null; yield return null;
            }
            yield return ClearFixtureAdditions(); EndCase("Two native pool launches with actual shooter/target binding; Projectile hit damage/release and flag reset, or Arrow path initialization/release.");
        }
    }

    private IEnumerator PlayerSkills()
    {
        string[] activeSkills = { "FireBall", "Blizzard", "Healing", "Thunder", "GoldyBlessing", "LoneBleesing", "WaterDropBarrier", "Dwarf", "AngerArmy", "DemiGod", "SerenBlessing", "BeleifWave", "KingSlime", "SkeletonArcher" };
        foreach (string name in activeSkills)
        {
            if (!Includes("skills", name)) continue;
            BeginCase("skills", name);
            LunaDataTable.Instance.playerData.equippedSkill = new List<string> { name, "FireBall", "Healing" };
            Game.Instance.uiManager.Pop(UI_TYPE.SKILL); Game.Instance.uiManager.Push(UI_TYPE.SKILL);
            yield return Until(() => Active<UISkill>(), "Player skill HUD reload"); yield return null; yield return null;
            UISkill ui = Active<UISkill>();
            fixtureObjects = new HashSet<GameObject>(NativeObjects());
            var infos = (List<UISkill.SkillInfo>)typeof(UISkill).GetField("lunaSkillDatas", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui);
            UISkill.SkillInfo info = infos.First(i => i.skillData.name == name);
            ResetTargets(); allyTarget.stat.Hp = allyTarget.objectData.Hp * 0.25f;
            GameObject player = Game.Instance.dataManager.LunaPlayer; player.transform.position = new Vector3(-0.8f, 0, 0);
            player.GetComponent<Status>().stat.Hp = player.GetComponent<Status>().objectData.Hp * 0.5f;
            Active<UIPlayer>().Mana = 1000; int mana = Active<UIPlayer>().Mana;
            info.isActive = true;
            string before = SkillSnapshot(allyTarget) + SkillSnapshot(enemyTarget) + SkillSnapshot(player.GetComponent<Status>());
            float goldyBefore = Defined.SkillTrigger_Goldy, timeScaleBefore = Time.timeScale;
            float enemyHpBefore = enemyTarget.stat.Hp, allyHpBefore = allyTarget.stat.Hp;
            float lowestEnemyHp = enemyHpBefore, highestAllyHp = allyHpBefore;
            int samples = 0, peakSpawnedActors = 0;
            bool statusChanged = false, goldyChanged = false;
            var serenEffects = new HashSet<GameObject>();
            int serenLiveSamples = 0;
            float serenLastLiveAge = 0f, activationGameTime = Time.time;
            var actorsBefore = new HashSet<GameObject>(NativeObjects());
            info.skillButton.GetComponent<Button>().onClick.Invoke();
            Require(Active<UIPlayer>().Mana == mana - info.skillData.mana && !info.isActive, "Equipped UI skill did not consume mana/start cooldown: " + name);
            if (name == "Thunder")
            {
                MethodInfo submit = typeof(UISkill).GetMethod("TrySubmitThunderTarget", new[] { typeof(Vector3) });
                Require(submit != null && (bool)submit.Invoke(ui, new object[] { enemyTarget.transform.position }), "Thunder did not accept the target through its normal shared input handler.");
                Require(!(bool)submit.Invoke(ui, new object[] { enemyTarget.transform.position }), "Thunder accepted a duplicate target for one activation.");
            }
            // Sample immediately and every frame: short buffs may legitimately expire before 3.5 seconds.
            float started = Time.realtimeSinceStartup;
            do
            {
                samples++;
                lowestEnemyHp = Mathf.Min(lowestEnemyHp, enemyTarget.stat.Hp);
                highestAllyHp = Mathf.Max(highestAllyHp, allyTarget.stat.Hp);
                statusChanged |= before != SkillSnapshot(allyTarget) + SkillSnapshot(enemyTarget) + SkillSnapshot(player.GetComponent<Status>());
                goldyChanged |= Defined.SkillTrigger_Goldy != goldyBefore;
                if (name == "SerenBlessing")
                {
                    foreach (GameObject candidate in NativeObjects())
                        if (!actorsBefore.Contains(candidate) && candidate.name.StartsWith(info.skillData.effectPrefab.name, StringComparison.Ordinal) && candidate.GetComponent<Effect>())
                            serenEffects.Add(candidate);
                    float age = Time.time - activationGameTime;
                    bool auraAlive = serenEffects.Any(effect => effect && LunaEntities.IsSpawned(effect) &&
                        effect.GetComponent<Effect>().ChaseTargetObject == allyTarget.gameObject &&
                        effect.GetComponentsInChildren<ParticleSystem>().Any(particle => particle.isPlaying && particle.IsAlive(true)));
                    if (auraAlive) { serenLiveSamples++; serenLastLiveAge = age; }
                    if (age >= 0.15f && age <= 4.8f)
                        Require(auraAlive, "Seren blessing lost its native-owned, playing nested-particle aura before the five-second buff expired; game age=" + age.ToString("R"));
                }
                if (info.skillData.effectPrefab)
                    peakSpawnedActors = Math.Max(peakSpawnedActors, NativeObjects().Count(actor => !actorsBefore.Contains(actor) &&
                        actor.name.StartsWith(info.skillData.effectPrefab.name, StringComparison.Ordinal) &&
                        actor.GetComponent<Status>() && actor.GetComponent<Status>().IsStatusDataLoadComplete));
                yield return null;
            }
            while (Time.realtimeSinceStartup - started < 3.5f);
            bool changed;
            switch (name)
            {
                case "FireBall": case "Blizzard": case "Thunder": changed = lowestEnemyHp < enemyHpBefore; break;
                case "Healing": changed = highestAllyHp > allyHpBefore; break;
                case "GoldyBlessing": changed = goldyChanged; break;
                case "Dwarf": case "DemiGod": case "KingSlime": case "SkeletonArcher": changed = peakSpawnedActors > 0; break;
                default: changed = statusChanged; break;
            }
            current.observations.Add("Continuous samples=" + samples + "; enemy HP drop=" + (enemyHpBefore - lowestEnemyHp).ToString("R") +
                "; ally healing=" + (highestAllyHp - allyHpBefore).ToString("R") + "; matching initialized summons=" + peakSpawnedActors +
                "; stat/buff change=" + statusChanged + "; Goldy income modifier " + goldyBefore.ToString("R") + " -> " + Defined.SkillTrigger_Goldy.ToString("R"));
            Require(changed, "Player skill accepted input but its damage/heal/buff/summon/income-modifier effect was not observed: " + name);
            if (name == "Thunder")
                Require(Mathf.Approximately(Time.timeScale, timeScaleBefore) && !ui.transform.Find("mask").gameObject.activeSelf, "Thunder retained its targeting slowdown or mask after submission.");
            if (name == "SerenBlessing")
            {
                Require(serenLiveSamples >= 2 && serenLastLiveAge >= 1f, "Seren blessing never sustained a playing nested-particle aura.");
                yield return Until(() => serenEffects.Count > 0 && serenEffects.All(effect => !effect || !LunaEntities.IsSpawned(effect)), "Seren blessing normal five-second aura expiry", 4f);
                Require(Time.time - activationGameTime >= 4.8f, "Seren blessing aura retired before its authored five-second lifetime.");
                current.observations.Add("Seren aura: live nested-particle samples=" + serenLiveSamples + "; last alive game age=" + serenLastLiveAge.ToString("R") +
                    "; all " + serenEffects.Count + " observed aura entities retired naturally at game age=" + (Time.time - activationGameTime).ToString("R") + ". Particle simulation/ownership checked; rendered visibility is separate.");
            }
            yield return ClearFixtureAdditions();
            // The reloaded skill form owns new native cooldown entities; preserve them for subsequent fixtures.
            fixtureObjects = new HashSet<GameObject>(NativeObjects());
            EndCase("Actual equipped UI button invoked; mana/cooldown and observable combat effect verified against live targets.");
        }
    }

    private IEnumerator ConcurrentFights()
    {
        string[][] groups = { new[] { "Lene", "Alice", "Tira" }, new[] { "Ria", "Lena", "Pochi" } };
        foreach (string[] names in groups)
        {
            string label = string.Join("+", names) + "-vs-mixed-enemies";
            if (!Includes("combos", label)) continue;
            BeginCase("combos", label);
            var actors = new List<GameObject>();
            foreach (string name in names)
                actors.Add(LunaEntities.Spawn(Resources.Load<GameObject>("Prefabs/01Heroes/" + name), new Vector3(-1.5f, 0, actors.Count * 0.001f), Quaternion.identity, null, true));
            foreach (string name in new[] { "SkeletonSword", "SkeletonArcher", "PoisonSlime" })
                actors.Add(LunaEntities.Spawn(Resources.Load<GameObject>("Prefabs/04Monsters/" + name), new Vector3(1.5f, 0, actors.Count * 0.001f), Quaternion.identity, null, true));
            yield return null; yield return null;
            foreach (GameObject actor in actors)
            {
                CheckStatus(actor.GetComponent<Status>());
                actor.GetComponent<Status>().unitState.IsLobbyMove = false;
                if (HasSkill(actor)) actor.GetComponent<Status>().unitState.IsSkillActivaion = true;
            }
            float initialHp = actors.Sum(a => a.GetComponent<Status>().stat.Hp);
            yield return new WaitForSecondsRealtime(8f);
            float finalHp = actors.Where(a => a && a.activeInHierarchy).Sum(a => a.GetComponent<Status>().stat.Hp);
            Require(finalHp < initialHp || actors.Any(a => !a || !a.activeInHierarchy), "Concurrent combat produced no measured damage/death.");
            CheckOwnership(); yield return ClearFixtureAdditions();
            EndCase("Three real heroes and three opposing monster types fought simultaneously for 8 seconds; actual damage/death and native ownership observed. Only this named combination is covered.");
        }
    }

    private IEnumerator EndlessReward()
    {
        if (!Includes("rewards", "endless-loot-mapping")) yield break;
        BeginCase("rewards", "endless-loot-mapping");
        yield return LunaDropRegression.Validate(current.observations.Add);
        EndCase("Actual boss-only and mixed loot spawn/collection mappings, counts, native ownership and immutable source prefabs checked by the dedicated regression.");
    }
}
#endif

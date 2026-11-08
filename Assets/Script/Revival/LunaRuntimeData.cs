using System;
using System.Collections.Generic;
using System.IO;
using Luna.Gameplay;
using UnityGameFramework.Runtime;
using GameFramework;
using GameFramework.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns mutable runtime data. Resource assets remain authoring templates.</summary>
public static class LunaRuntimeData
{
    private const string SaveKey = "Luna.Progress.v1";
    private static readonly Dictionary<UnityEngine.Object, UnityEngine.Object> clones = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
    private static readonly HashSet<UnityEngine.Object> runtimeObjects = new HashSet<UnityEngine.Object>();
    private static ISettingManager settings;
    private static int tutorialIndex;
    private static bool allowSave;
    private static GameObject progressHost;
    public static bool IsInitialized { get; private set; }
    public static string SaveDirectory { get; private set; }
    public static int RuntimeAssetCount => runtimeObjects.Count;
    public static UnityEngine.Object[] GetRuntimeAssets()
    {
        var result = new UnityEngine.Object[runtimeObjects.Count];
        runtimeObjects.CopyTo(result);
        return result;
    }
    internal static void TrackRuntimeAsset(ScriptableObject instance)
    {
        if (instance != null) runtimeObjects.Add(instance);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        IsInitialized = false;
        settings = null;
        tutorialIndex = 0;
        allowSave = false;
        ReleaseRuntimeAssets();
        progressHost = null;
    }

    internal static void ReleaseRuntimeAssets(bool immediate = false)
    {
        // DontSave clones are not authoring assets and need an explicit owner on teardown,
        // including play sessions with domain reload disabled. During play, defer native
        // destruction until the frame ends so Procedure shutdown can settle and save loot.
        foreach (var instance in runtimeObjects)
        {
            if (instance == null) continue;
            if (immediate || !Application.isPlaying) UnityEngine.Object.DestroyImmediate(instance);
            else UnityEngine.Object.Destroy(instance);
        }
        runtimeObjects.Clear();
        clones.Clear();
    }

    // Called only after the round has settled/saved and native consumers have stopped.
    internal static void Shutdown(bool immediate = false)
    {
        IsInitialized = false;
        allowSave = false;
        settings = null;
        if (progressHost != null)
        {
            var autosave = progressHost.GetComponent<LunaProgressAutosave>();
            if (autosave != null) autosave.enabled = false;
            if (immediate) UnityEngine.Object.DestroyImmediate(progressHost);
            else UnityEngine.Object.Destroy(progressHost);
            progressHost = null;
        }
        ReleaseRuntimeAssets(immediate);
        LunaDataTable.ReleaseRuntimeTable();
        LunaUnitBalance.ReleaseRuntimeTable();
        LunaSkillBalance.ReleaseRuntimeTable();
        LunaStageTables.ReleaseRuntimeTables();
        LunaEntityParameters.ReleaseRuntimeTable();
    }

public static T CloneAsset<T>(T source) where T : ScriptableObject
    {
        if (source == null || !Application.isPlaying || runtimeObjects.Contains(source)) return source;
        if (clones.TryGetValue(source, out var clone)) return (T)clone;
        var instance = UnityEngine.Object.Instantiate(source);
        instance.name = source.name;
        instance.hideFlags = HideFlags.DontSave;
        if (source is ObjectData sourceUnit && instance is ObjectData runtimeUnit)
            LunaUnitBalance.Apply(sourceUnit, runtimeUnit);
        if (source is SkillData sourceSkill && instance is SkillData runtimeSkill)
            LunaSkillBalance.Apply(sourceSkill, runtimeSkill);
        clones.Add(source, instance);
        runtimeObjects.Add(instance);
        return instance;
    }

    public static ObjectData ResolveObjectData(ObjectData source) => CloneAsset(source);

    public static void Initialize()
    {
        if (IsInitialized) return;
        LunaUnitBalance.Initialize();
        LunaSkillBalance.Initialize();
        var table = LunaDataTable.Instance;
        var template = Resources.Load<PlayerData>("ScriptableObjects/PlayerData");
        if (template == null) throw new InvalidOperationException("PlayerData resource is missing.");
        table.playerData = CloneAsset(template);
        table.UnitTable.Clear();
        table.lunaTable.Clear();
        table.itemTable.Clear();
        table.skiiTable.Clear();
        table.StageTable.Clear();
        table.InitializeUnitNames();
        table.InitializeLunaDatas();
        table.InitializeObjectDatas();
        table.InitializeItemDatas();
        table.InitializeSKillDatas();
        table.InitializeStageDatas();
        SaveDirectory = Environment.GetEnvironmentVariable("LUNA_SAVE_DIRECTORY");
        if (string.IsNullOrWhiteSpace(SaveDirectory)) SaveDirectory = Path.Combine(Application.persistentDataPath, "Luna");
        SaveDirectory = Path.GetFullPath(SaveDirectory);
        var host = new GameObject("Luna Progress");
        progressHost = host;
        UnityEngine.Object.DontDestroyOnLoad(host);
        var helper = host.AddComponent<LunaSaveSettingsHelper>();
        helper.Configure(SaveDirectory);
        settings = GameFrameworkEntry.GetModule<ISettingManager>();
        settings.SetSettingHelper(helper);
        allowSave = settings.Load();
        IsInitialized = true;
        if (allowSave) RestoreSnapshot();
        tutorialIndex = Math.Max(0, settings.GetInt("Luna.Tutorial.Index", tutorialIndex));
        AudioListener.volume = settings.GetBool("Luna.Sound.Enabled", true) ? 1f : 0f;
        table.playerData.ValidateDefaults();
        table.SetLunaDatasLevel(table.playerData.LunaLevel);
        foreach (var pair in table.lunaTable) pair.Value.moveSpeedLevel = table.playerData.LunaMoveSpeedLevel;
        LunaSession.Current.RefreshDeckPrefabs();
        host.AddComponent<LunaProgressAutosave>();
    }

    public static bool SaveNow()
    {
        // Editor teardown may outlive play mode. At that point table access resolves
        // authoring assets, which must never overwrite the captured runtime progress.
        if (!Application.isPlaying || !IsInitialized || !allowSave || settings == null) return false;
        settings.SetString(SaveKey, JsonUtility.ToJson(Capture()));
        settings.SetInt("Luna.Tutorial.Index", tutorialIndex);
        return settings.Save();
    }

    public static bool Reload()
    {
        if (!IsInitialized || settings == null || !settings.Load()) return false;
        if (!RestoreSnapshot()) return false;
        allowSave = true;
        tutorialIndex = Math.Max(0, settings.GetInt("Luna.Tutorial.Index", tutorialIndex));
        AudioListener.volume = settings.GetBool("Luna.Sound.Enabled", true) ? 1f : 0f;
        var table = LunaDataTable.Instance;
        table.SetLunaDatasLevel(table.playerData.LunaLevel);
        foreach (var pair in table.lunaTable) pair.Value.moveSpeedLevel = table.playerData.LunaMoveSpeedLevel;
        table.playerData.SetCurPlayer();
        LunaSession.Current.RefreshDeckPrefabs();
        return true;
    }

    private static bool RestoreSnapshot()
    {
        if (!settings.HasSetting(SaveKey)) return true;
        try
        {
            var snapshot = JsonUtility.FromJson<Snapshot>(settings.GetString(SaveKey));
            if (snapshot == null || snapshot.version != 1 || snapshot.player == null)
                throw new InvalidDataException("Unsupported Luna save version or missing player state.");
            ValidateSnapshot(snapshot);
            var table = LunaDataTable.Instance;
            snapshot.player.Apply(table.playerData);
            ApplyObjects(snapshot.units, table.UnitTable);
            ApplyObjects(snapshot.luna, table.lunaTable);
            if (snapshot.items != null) foreach (var item in snapshot.items)
                if (table.itemTable.TryGetValue(item.id, out var data)) { data.itemCount = Math.Max(0, item.count); data.level = Math.Max(0, item.level); data.isActive = item.active; }
            if (snapshot.skills != null) foreach (var skill in snapshot.skills)
                if (table.skiiTable.TryGetValue(skill.name, out var data)) data.level = Math.Max(1, skill.level);
            if (snapshot.stages != null) foreach (var stage in snapshot.stages)
                if (table.StageTable.TryGetValue(stage.name, out var data)) data.isClear = stage.clear;
            table.playerData.ValidateDefaults();
            return true;
        }
        catch (Exception exception)
        {
            allowSave = false;
            Debug.LogWarning("Luna progress cannot be restored; automatic saves are disabled to preserve it. " + exception.Message);
            return false;
        }
    }

    private static void ValidateSnapshot(Snapshot snapshot)
    {
        // Validate the complete shape before applying it, so malformed JSON cannot half-restore a session.
        if (snapshot.player.deck == null || snapshot.player.skills == null || snapshot.units == null ||
            snapshot.luna == null || snapshot.items == null || snapshot.skills == null || snapshot.stages == null)
            throw new InvalidDataException("Luna save collections are missing.");
        foreach (var state in snapshot.units)
            if (state == null || string.IsNullOrEmpty(state.name)) throw new InvalidDataException("Invalid unit save entry.");
        foreach (var state in snapshot.luna)
            if (state == null || string.IsNullOrEmpty(state.name)) throw new InvalidDataException("Invalid player character save entry.");
        foreach (var state in snapshot.items)
            if (state == null) throw new InvalidDataException("Invalid item save entry.");
        foreach (var state in snapshot.skills)
            if (state == null || string.IsNullOrEmpty(state.name)) throw new InvalidDataException("Invalid skill save entry.");
        foreach (var state in snapshot.stages)
            if (state == null || string.IsNullOrEmpty(state.name)) throw new InvalidDataException("Invalid stage save entry.");
    }

    private static void ApplyObjects(List<ObjectState> states, Dictionary<string, ObjectData> table)
    {
        if (states == null) return;
        foreach (var state in states)
            if (table.TryGetValue(state.name, out var data))
            {
                data.Level = Math.Max(1, state.level);
                data.moveSpeedLevel = Math.Max(1, state.moveSpeedLevel);
                data.cardCount = Math.Max(0, state.cards);
                data.ultState = (eUltState)Mathf.Clamp(state.rank, 1, 7);
            }
    }

    private static Snapshot Capture()
    {
        var table = LunaDataTable.Instance;
        var snapshot = new Snapshot { player = PlayerState.Capture(table.playerData) };
        foreach (var pair in table.UnitTable) snapshot.units.Add(ObjectState.Capture(pair.Key, pair.Value));
        foreach (var pair in table.lunaTable) snapshot.luna.Add(ObjectState.Capture(pair.Key, pair.Value));
        foreach (var pair in table.itemTable) snapshot.items.Add(new ItemState { id = pair.Key, count = pair.Value.itemCount, level = pair.Value.level, active = pair.Value.isActive });
        foreach (var pair in table.skiiTable) snapshot.skills.Add(new SkillState { name = pair.Key, level = pair.Value.level });
        foreach (var pair in table.StageTable) snapshot.stages.Add(new StageState { name = pair.Key, clear = pair.Value.isClear });
        return snapshot;
    }

    [Serializable] private sealed class Snapshot
    {
        public int version = 1;
        public PlayerState player;
        public List<ObjectState> units = new List<ObjectState>();
        public List<ObjectState> luna = new List<ObjectState>();
        public List<ItemState> items = new List<ItemState>();
        public List<SkillState> skills = new List<SkillState>();
        public List<StageState> stages = new List<StageState>();
    }

    [Serializable] private sealed class ObjectState
    {
        public string name;
        public int level, moveSpeedLevel, cards, rank;
        public static ObjectState Capture(string name, ObjectData data) => new ObjectState { name = name, level = data.Level, moveSpeedLevel = data.moveSpeedLevel, cards = data.cardCount, rank = (int)data.ultState };
    }
    [Serializable] private sealed class ItemState { public int id, count, level; public bool active; }
    [Serializable] private sealed class SkillState { public string name; public int level; }
    [Serializable] private sealed class StageState { public string name; public bool clear; }
    [Serializable] private sealed class PlayerState
    {
        public string stage;
        public List<string> deck, skills;
        public bool beginning;
        public int gold, starStone, lunaLevel, lunaMove, goldSpeed, towerHp, aurora, unitMove, revive, skin;
        public static PlayerState Capture(PlayerData data) => new PlayerState
        {
            stage = data.SaveStageName, deck = new List<string>(data.UnitNamesInDeck), skills = new List<string>(data.equippedSkill), beginning = data.IsBegining,
            gold = data.Gold, starStone = data.StarStone, lunaLevel = data.LunaLevel, lunaMove = data.LunaMoveSpeedLevel, goldSpeed = data.GoldSpeedLevel,
            towerHp = data.TowerHPLevel, aurora = data.AuroraLevel, unitMove = data.UnitMoveSpeedLevel, revive = data.LunaReviveTimeLevel, skin = (int)data.SkinType
        };
        public void Apply(PlayerData data)
        {
            data.SaveStageName = stage; data.UnitNamesInDeck = deck; data.equippedSkill = skills; data.IsBegining = beginning;
            data.Gold = Math.Max(0, gold); data.StarStone = Math.Max(0, starStone); data.LunaLevel = lunaLevel; data.LunaMoveSpeedLevel = lunaMove;
            data.GoldSpeedLevel = goldSpeed; data.TowerHPLevel = towerHp; data.AuroraLevel = aurora; data.UnitMoveSpeedLevel = unitMove;
            data.LunaReviveTimeLevel = revive; data.SkinType = (PlayerData.eLunaSkin)skin;
        }
    }
}

public sealed class LunaProgressAutosave : GameFrameworkComponent
{
    private float nextSave;
    private void OnEnable() { SceneManager.activeSceneChanged += OnSceneChanged; nextSave = Time.unscaledTime + 15f; }
    private void OnDisable() { SceneManager.activeSceneChanged -= OnSceneChanged; }
    private void OnSceneChanged(Scene before, Scene after) { LunaRuntimeData.SaveNow(); }
    private void Update()
    {
        if (Time.unscaledTime < nextSave) return;
        nextSave = Time.unscaledTime + 15f;
        LunaRuntimeData.SaveNow();
    }
    private void OnApplicationPause(bool paused) { if (paused) LunaRuntimeData.SaveNow(); }
    private void OnApplicationQuit() { LunaRuntimeData.SaveNow(); }
    // The bootstrap owns teardown order; this host must not destroy data while a
    // Procedure is still settling rewards or saving its final progress.
}

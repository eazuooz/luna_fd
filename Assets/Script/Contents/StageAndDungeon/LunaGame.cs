using System;
using System.Collections.Generic;
using System.Threading;
using GameFramework.Event;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>A single round owned and updated by LunaBattleProcedure. No Unity message or coroutine drives round rules.</summary>
public abstract class LunaGame
{
    public enum EndReason { None, Victory, TimeExpired, TowerDestroyed, PlayerDefeated }
    public EndReason ResultReason { get; private set; }
    public float RemainingObjectiveHealth { get; private set; }
    public int SkillsUsed { get; private set; }
    public bool HasHealed { get; private set; }
    public float FirstSkillTime { get; private set; } = -1f;
    public bool HasRetreated { get; private set; }
    protected virtual UnitLogic Objective => null;
    protected virtual EndReason DeathDefeatReason => EndReason.PlayerDefeated;
    public virtual string GuidanceText => row.Tip;

    internal void RecordSkillUse(string skill)
    {
        SkillsUsed++;
        if (skill == "Healing") HasHealed = true;
        if (FirstSkillTime < 0f) FirstSkillTime = StageCount;
    }
    internal void RecordRetreat() => HasRetreated = true;
    private static int roundSerial;
    private readonly HashSet<int> pending = new HashSet<int>();
    private struct EarnedLoot { public int ItemId, Count, CoinValue; public bool IsCoin; }
    private readonly Dictionary<int, EarnedLoot> earnedLoot = new Dictionary<int, EarnedLoot>();
    private EventComponent events;
    private int playerId;
    private int playerTowerId;
    private bool respawnPending;
    private float respawnTimer;
    private int graveId, resurrectionId;
    private float resultAge;
    private bool resultStarted, resultOpened;
    private float shakeTime;
    private Vector3 dragPosition;
    private int musicSerial;
    protected LunaStageDefinition definition;
    protected LunaStageRow row;
    protected Camera stageCamera;
    protected bool IntroComplete;
    public int RoundId { get; private set; }
    public bool IsActive { get; private set; }
    public int PendingSpawnCount => pending.Count + (Roster?.PendingCount ?? 0);
    public int PendingLootCount => earnedLoot.Count;
    public LunaBattleRoster Roster { get; private set; }
    public int PlayerEntityId => playerId;
    public bool PlayerRespawnPending => respawnPending;
    public string FailureMessage { get; private set; }
    public string stageName => row.DisplayName;
    public string StageKey => row.Key;
    public float TimeLimit => row.TimeLimit;
    public LunaStageDefinition Definition => definition;
    public bool isStageStop { get; set; }
    public bool isStageWin { get; set; }
    public float StageCount { get; protected set; }
    public bool AttachCamera { get; set; }
    public bool FollowCamera { get; set; }
    public bool ShakeOn => shakeTime > 0f;
    public UnitLogic Player => LunaSpawn.Find<UnitLogic>(playerId);
    protected bool IsFinishing => resultStarted;
    private bool firstClear;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSerial() => roundSerial = 0;
    public virtual void Initialize(LunaStageDefinition source)
    {
        definition = source ?? throw new ArgumentNullException(nameof(source));
        row = definition.Row;
        RoundId = Interlocked.Increment(ref roundSerial);
        IsActive = true; isStageStop = true; isStageWin = false;
        StageCount = 0f; IntroComplete = false;
        stageCamera = LunaBootstrap.GameplayCamera;
        if (stageCamera == null) throw new InvalidOperationException("Battle camera is missing.");
        LunaSession.Current.LunaPlayer = null;
        LunaSession.Current.pickUpData.Clear();
        LunaDataTable.Instance.playerData.RewardGold = 0;
        events = GameEntry.GetComponent<EventComponent>();
        events.Subscribe(PlayerDiedEventArgs.EventId, OnPlayerDied);
        events.Subscribe(UnitDiedEventArgs.EventId, OnUnitDied);
        Roster = new LunaBattleRoster(this);
        var options = GameFramework.Sound.PlaySoundParams.Create(); options.Loop = true; options.SpatialBlend = 0f;
        musicSerial = GameEntry.GetComponent<SoundComponent>().PlaySound("Luna/Audio/Battle", "LunaMusic", options);
    }
    public void Update(float elapsed, float realElapsed)
    {
        if (!IsActive || FailureMessage != null) return;
        Roster.Tick(elapsed);
        TickRespawn(elapsed);
        if (resultStarted) TickResult(elapsed);
        else TickRound(elapsed, realElapsed);
        if (shakeTime > 0f)
        {
            shakeTime = Mathf.Max(0f, shakeTime - elapsed);
            var p = stageCamera.transform.position; p.x += Mathf.Sin(shakeTime * 220f) * 0.1f;
            stageCamera.transform.position = p;
        }
    }
    protected abstract void TickRound(float elapsed, float realElapsed);
    public virtual void Shutdown()
    {
        if (!IsActive) return;
        SettleLoot();
        if (LunaRuntimeData.IsInitialized) LunaRuntimeData.SaveNow();
        IsActive = false; // Hide can fire a final death event; this round must already reject it.
        if (events != null)
        {
            events.Unsubscribe(PlayerDiedEventArgs.EventId, OnPlayerDied);
            events.Unsubscribe(UnitDiedEventArgs.EventId, OnUnitDied);
        }
        Roster?.Shutdown();
        LunaSpawn.CancelRound(RoundId);
        pending.Clear(); respawnPending = false;
        if (musicSerial != 0)
        {
            var sound = GameEntry.GetComponent<SoundComponent>();
            if (sound != null) sound.StopSound(musicSerial);
        }
        // Subsystem registration order is unspecified when domain reload is disabled.
        // The prior native Procedure may shut down after the session static was reset.
        if (LunaSession.Current != null)
        {
            LunaSession.Current.LunaPlayer = null;
            LunaSession.Current.doNotSummonMode = false;
        }
        SetResultAlpha(0f);
    }
    protected int Show<T>(GameObject prefab, LunaEntityData data, Action<T> shown = null) where T : EntityLogicBase
    {
        data.RoundId = RoundId;
        int id = data.Id;
        if (data is LootData loot)
            earnedLoot.Add(id, new EarnedLoot { ItemId = loot.ItemId, Count = loot.Count, CoinValue = loot.CoinValue,
                IsCoin = typeof(DropCoinLogic).IsAssignableFrom(typeof(T)) });
        pending.Add(id); // Cached native loads may complete inside Show.
        LunaSpawn.Show<T>(prefab, data, actor =>
        {
            pending.Remove(id);
            if (!IsActive || actor.SpawnData == null || actor.SpawnData.RoundId != RoundId) { actor.SafeHide(); return; }
            if (isStageStop && actor is UnitLogic unit)
            { unit.unitState.IsLobbyMove = true; unit.Data.InitialMode = UnitSpawnMode.Staged; }
            shown?.Invoke(actor);
        }, error =>
        {
            pending.Remove(id);
            if (!IsActive) return;
            FailureMessage = "Stage " + StageKey + " could not spawn " + (prefab != null ? prefab.name : "null") + ": " + error;
            Debug.LogError(FailureMessage);
        });
        return id;
    }
    public int SpawnUnit(GameObject prefab, Vector3 position, UnitSpawnMode mode = UnitSpawnMode.Battle, int level = 0,
        int allyTowerId = 0, float healthMultiplier = 1f, Action<UnitLogic> shown = null, float attackMultiplier = 1f)
    {
        var view = prefab.GetComponent<UnitView>();
        UnitData data = view != null && view.Definition != null && view.Definition.UnitType == eUnitType.Tower
            ? (UnitData)TowerData.CreateFrom(view, position) : UnitData.CreateFrom(view, position);
        data.Rotation = Quaternion.identity; data.InitialMode = mode; data.AllyTowerId = allyTowerId; data.HealthMultiplier = healthMultiplier;
        data.AttackMultiplier = attackMultiplier;
        if (level > 0) data.Level = level;
        return Show(prefab, data, shown);
    }
    protected void SpawnFormation(LunaStageWaveRow wave, Vector3 anchor, UnitSpawnMode mode, int levelBonus = 0,
        float healthMultiplier = 1f, float attackMultiplier = 1f, bool randomY = false)
    {
        var prefab = LunaStageTables.RequireAsset<GameObject>(wave.Prefab);
        for (int i = 0; i < wave.Count; i++)
        {
            float y = randomY ? UnityEngine.Random.Range(-0.2f, 0.45f) : 0f;
            var position = anchor + new Vector3(-i * 0.36f, y, y + (wave.Id % 1000) * 0.0001f);
            SpawnUnit(prefab, position, mode, wave.Level + levelBonus, healthMultiplier: healthMultiplier, attackMultiplier: attackMultiplier);
        }
    }
    protected int SpawnPlayer(Vector3 position, int towerId, bool staged)
    {
        playerTowerId = towerId;
        return SpawnUnit(LunaDataTable.Instance.playerData.CurPlayer(), position,
            staged ? UnitSpawnMode.Staged : UnitSpawnMode.Battle, allyTowerId: towerId, shown: actor =>
            {
                if (resultStarted) { actor.SafeHide(); return; }
                playerId = actor.ID;
                LunaSession.Current.LunaPlayer = actor.gameObject;
                respawnPending = false;
                OnPlayerShown(actor);
            });
    }
    protected virtual void OnPlayerShown(UnitLogic actor) { }
    protected int SpawnBackdrop(GameObject prefab, Vector3 position, Action<LunaStageBackdropLogic> shown = null)
    {
        var data = LunaEntityData.Create<LunaStageEntityData>();
        data.Position = position; data.Rotation = prefab.transform.rotation;
        return Show(prefab, data, shown);
    }
    protected int SpawnEffect(string resource, Vector3 position, float lifetime, string animation = "normal", bool loop = false)
    {
        var prefab = LunaStageTables.RequireAsset<GameObject>(resource);
        var data = EffectData.Create(position, roundId: RoundId);
        data.Rotation = prefab.transform.rotation; data.Lifetime = lifetime; data.Animation = animation; data.Loop = loop;
        return Show<EffectLogic>(prefab, data);
    }
    protected List<UnitLogic> Units()
    {
        var result = new List<UnitLogic>();
        var entities = GameEntry.GetComponent<EntityComponent>();
        if (entities == null) return result;
        foreach (var entity in entities.GetAllLoadedEntities())
            if (entity.Logic is UnitLogic unit && unit.Available && !unit.IsHiding && unit.SpawnData != null && unit.SpawnData.RoundId == RoundId)
                result.Add(unit);
        return result;
    }
    protected static bool Alive(UnitLogic unit) => unit != null && unit.Available && !unit.IsHiding && unit.stat.Hp > 0f;
    protected void StageUnits(bool staged)
    {
        foreach (var unit in Units())
        {
            unit.unitState.IsLobbyMove = staged;
            unit.Data.InitialMode = staged ? UnitSpawnMode.Staged : UnitSpawnMode.Battle;
        }
    }
    protected void StartCombat(bool minimap)
    {
        StageUnits(false); IntroComplete = true; isStageStop = false;
        Time.timeScale = 1.5f;
        Roster.StartCombat();
        LunaUI.Open(UI_TYPE.GAMEOPTION); LunaUI.Open(UI_TYPE.PLAYER); LunaUI.Open(UI_TYPE.SKILL);
        if (minimap) LunaUI.Open(UI_TYPE.PROGRESSBAR);
    }
    protected void Finish(bool won, EndReason reason = EndReason.PlayerDefeated)
    {
        if (resultStarted) return;
        ResultReason = won ? EndReason.Victory : reason;
        RemainingObjectiveHealth = Objective != null
            ? Mathf.Clamp01(Objective.stat.Hp / Mathf.Max(1f, Objective.HealthBarHpPoint)) : 0f;
        resultStarted = true; isStageStop = true; isStageWin = won; resultAge = 0f;
        respawnPending = false;
        LunaSpawn.Cancel(graveId); LunaSpawn.Cancel(resurrectionId);
        if (Player is PlayerLogic player) player.NMove();
        StageUnits(true);
        if (won)
        {
            var progress = LunaDataTable.Instance;
            if (progress.StageTable.TryGetValue(StageKey, out var campaign))
            {
                firstClear = !campaign.isClear;
                campaign.isClear = true;
            }
            else if (this is BossDungeon)
            {
                firstClear = !progress.playerData.ClearedDungeons.Contains(StageKey);
                if (firstClear) progress.playerData.ClearedDungeons.Add(StageKey);
            }
        }
        OnFinished(won);
        if (won) DropItem();
    }
    protected virtual void OnFinished(bool won) { }

    // A reward belongs to the round when it drops. Its visual load/pickup may finish
    // later; scene exit and result presentation must not discard or duplicate it.
    public bool TryCollectLoot(int entityId)
    {
        if (!earnedLoot.TryGetValue(entityId, out var loot)) return false;
        earnedLoot.Remove(entityId);
        if (loot.IsCoin)
        {
            var player = LunaDataTable.Instance.playerData;
            LunaEconomy.GrantGold(player, loot.Count, loot.CoinValue);
        }
        else
        {
            if (!LunaDataTable.Instance.itemTable.TryGetValue(loot.ItemId, out var item))
                throw new InvalidOperationException("Round reward has no item definition: " + loot.ItemId);
            item.itemCount += loot.Count;
            var picked = LunaSession.Current.pickUpData;
            picked.TryGetValue(loot.ItemId, out int count); picked[loot.ItemId] = count + loot.Count;
        }
        return true;
    }
    public void SettleLoot()
    {
        if (!LunaRuntimeData.IsInitialized || LunaSession.Current == null) { earnedLoot.Clear(); return; }
        foreach (int id in new List<int>(earnedLoot.Keys)) TryCollectLoot(id);
    }
    protected void LeaveResultPresentation()
    {
        resultStarted = resultOpened = false; SetResultAlpha(0f);
    }
    protected virtual float ResultHoldDuration => 0.5f;
    private void TickResult(float elapsed)
    {
        resultAge += elapsed;
        if (resultAge < 1f) return;
        float alpha = 1f - Mathf.Clamp01(resultAge - 1f - ResultHoldDuration);
        SetResultAlpha(alpha);
        if (alpha > 0f || resultOpened) return;
        resultOpened = true;
        SettleLoot();
        LunaRuntimeData.SaveNow();
        LunaUI.Close(UI_TYPE.PROGRESSBAR); LunaUI.Close(UI_TYPE.SKILL); LunaUI.Close(UI_TYPE.PLAYER); LunaUI.Close(UI_TYPE.GAMEOPTION);
        LunaUI.Open(UI_TYPE.RESULT);
    }
    private void SetResultAlpha(float alpha)
    {
        if (stageCamera == null) return;
        var label = stageCamera.transform.Find("WinLoseText");
        if (label == null) return;
        var renderer = label.GetComponent<SpriteRenderer>(); var sprites = label.GetComponent<WinLoseText>();
        if (renderer == null) return;
        if (sprites != null) renderer.sprite = isStageWin ? sprites.winText : sprites.loseText;
        var color = renderer.color; color.a = alpha; renderer.color = color;
    }
    private void OnPlayerDied(object sender, GameEventArgs args)
    {
        var death = (PlayerDiedEventArgs)args;
        if (!IsActive || resultStarted || death.RoundId != RoundId || death.EntityId != playerId || respawnPending) return;
        if (!CanRespawnPlayer) { LunaSession.Current.LunaPlayer = null; Finish(false, DeathDefeatReason); return; }
        respawnPending = true; playerTowerId = death.AllyTowerId;
        float reduction = Defined.lunaReviveTimeValue * LunaDataTable.Instance.playerData.LunaReviveTimeLevel;
        respawnTimer = Mathf.Max(0f, death.RespawnDelay * (1f - reduction));
        LunaSession.Current.LunaPlayer = null;
        graveId = SpawnEffect("Prefabs/06Effects/Grave_tmp", Defined.LunaStartPosition, respawnTimer + 1f, loop: true);
        resurrectionId = SpawnEffect("Prefabs/06Effects/Skill/ResurrectPlayer", new Vector3(-8f, 0.6f), respawnTimer + 1f, loop: true);
    }
    protected virtual bool CanRespawnPlayer => false;
    private void OnUnitDied(object sender, GameEventArgs args)
    {
        var death = (UnitDiedEventArgs)args;
        if (this is BasicDungeon && IsActive && !IsFinishing && death.RoundId == RoundId &&
            death.Side == eUnitSide.Enemy && death.Priority != eHitPriority.TOWER)
            DropItemMonster(death.Position);
    }
    private void TickRespawn(float elapsed)
    {
        if (!respawnPending || resultStarted) return;
        respawnTimer -= elapsed;
        if (respawnTimer > 0f) return;
        respawnTimer = float.PositiveInfinity; // Only one asynchronous request per death.
        LunaSpawn.Cancel(graveId); LunaSpawn.Cancel(resurrectionId);
        SpawnEffect("Prefabs/06Effects/RebirthStoneEffect", new Vector3(-8f, 0.6f), 2f, "rebirth");
        SpawnPlayer(Defined.AllyTowerPosition, playerTowerId, !IntroComplete);
    }
    public void ShakeCameraOn(int duration)
    {
        if (shakeTime > 0f) return;
        AttachCamera = false; shakeTime = Mathf.Max(0f, duration * 0.03f);
    }
    protected void TickCamera(float elapsed, float width)
    {
        if (stageCamera == null || Player == null) return;
        var p = stageCamera.transform.position;
        if (FollowCamera)
        {
            p.x = Mathf.MoveTowards(p.x, Player.CachedTransform.position.x + 0.5f, (Mathf.Abs(Player.CachedTransform.position.x + 0.5f - p.x) * 2.5f + 1f) * elapsed);
            if (Mathf.Abs(p.x - Player.CachedTransform.position.x - 0.5f) < 0.1f) { FollowCamera = false; AttachCamera = true; }
        }
        else if (AttachCamera) p.x = Player.CachedTransform.position.x + 0.5f;
        var touch = Input.mousePosition;
        if (touch.y > Screen.height * 0.2f && touch.y < Screen.height * 0.9f)
        {
            if (Input.GetMouseButtonDown(0)) dragPosition = touch;
            else if (Input.GetMouseButton(0)) { AttachCamera = false; p.x -= (touch.x - dragPosition.x) * elapsed; dragPosition = touch; }
        }
        p.x = Mathf.Clamp(p.x, -width, width); stageCamera.transform.position = p;
    }
    public virtual void DropItem()
    {
        var player = LunaDataTable.Instance.playerData;
        int gold = row.Gold;
        if (player.SkinType == PlayerData.eLunaSkin.Rich && Defined.CanProbability(50)) gold *= 2;
        AwardGold(gold + (firstClear ? row.FirstClearGold : 0));
        var rewards = LunaStageTables.GetRewards(row.Id, "Victory");
        foreach (var reward in rewards)
            if (Defined.CanProbability(reward.Probability * 100f)) SpawnReward(reward, Defined.EnemyTowerPosition);
        if (firstClear)
        {
            AwardStones(row.FirstClearStones);
            foreach (var reward in LunaStageTables.GetRewards(row.Id, "FirstClear"))
                if (Defined.CanProbability(reward.Probability * 100f)) SpawnReward(reward, Defined.EnemyTowerPosition);
        }
    }
    protected void AwardGold(int amount)
    {
        if (amount <= 0) return;
        var common = LunaStageTables.RequireAsset<GameObject>("LunaAssetBundle").GetComponent<LunaAssetBundle>();
        var data = LootData.Create(0, 1, Defined.EnemyTowerPosition, roundId: RoundId);
        data.CoinValue = amount;
        data.Rotation = common.Coin.transform.rotation;
        Show<DropCoinLogic>(common.Coin, data);
    }
    public int RewardStones { get; private set; }
    protected void AwardStones(int amount)
    {
        var player = LunaDataTable.Instance.playerData;
        int credit = Mathf.Clamp(amount, 0, int.MaxValue - player.StarStone);
        player.StarStone += credit;
        RewardStones += credit;
    }
    public virtual void DropItemMonster(Vector3 position)
    {
        var rewards = LunaStageTables.GetRewards(row.Id, "Monster");
        if (rewards.Count != 1) return; // Preserve the authored single-roll monster rule.
        if (Defined.CanProbability(rewards[0].Probability * 100f)) SpawnReward(rewards[0], position, false);
    }
    protected void SpawnReward(LunaStageRewardRow reward, Vector3 position, bool scatter = true, int count = 1)
    {
        var item = LunaStageTables.RequireAsset<ItemData>(reward.Item);
        if (scatter) position += Vector3.right * UnityEngine.Random.Range(-2f, 2f);
        var data = LootData.Create(item.itemNumber, count, position, roundId: RoundId);
        data.Rotation = item.cunnectPrefab.transform.rotation;
        Show<DropItemLogic>(item.cunnectPrefab, data);
    }
}

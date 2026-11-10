using System;
using System.Collections.Generic;
using Luna.Gameplay;
using UnityEngine;

/// <summary>Main campaign round. Procedure.Update advances intro, waves, result and floor transition.</summary>
public sealed class BasicDungeon : LunaGame
{
    private enum Phase { Loading, IntroWait, IntroSpawns, IntroRight, IntroHold, IntroLeft, Combat, NextLoading, NextWait, NextRise, NextWalk }
    private Phase phase;
    private float timer;
    private int enemyTowerId, allyTowerId, backgroundId;
    private int nextBackgroundId, nextPlayerId;
    private int waveIndex;
    private bool skipIntro;
    private List<LunaStageWaveRow> timedWaves;
    public GameObject gameEnemyTower => LunaSpawn.Find<UnitLogic>(enemyTowerId)?.gameObject;
    public GameObject gameAllyTower => LunaSpawn.Find<UnitLogic>(allyTowerId)?.gameObject;
    public GameObject gamePlayer => Player != null ? Player.gameObject : null;
    public float stageWidth => row.Width;
    public bool isBossStage => row.IsBossStage;
    public bool HasNextStage => row.Id < 30;
    public float SecondsUntilWave => waveIndex < timedWaves.Count ? Mathf.Max(0f, timedWaves[waveIndex].At - StageCount) : float.PositiveInfinity;
    public string PhaseName => phase.ToString();
    public override void Initialize(LunaStageDefinition source)
    {
        base.Initialize(source);
        Defined.OriginPosition = Vector3.zero;
        Defined.AllyTowerPosition = new Vector3(-8f, 0f, 0f);
        Defined.EnemyTowerPosition = new Vector3(8f, 0f, 0f);
        Defined.LunaStartPosition = new Vector3(-5f, 0f, 0f);
        Defined.StayMonsterPosition = new Vector3(5f, 0f, 0f);
        phase = Phase.Loading;
        timedWaves = LunaStageTables.GetWaves(row.Id, "Timed");
        backgroundId = SpawnBackdrop(definition.backgroundImage, Vector3.zero);
        enemyTowerId = SpawnUnit(LunaStageTables.RequireAsset<GameObject>(row.EnemyTower), Defined.EnemyTowerPosition,
            UnitSpawnMode.Staged, healthMultiplier: row.ObjectiveHealth, attackMultiplier: row.ObjectiveAttack);
        allyTowerId = SpawnUnit(LunaStageTables.RequireAsset<GameObject>(row.AllyTower), Defined.AllyTowerPosition, UnitSpawnMode.Staged,
            healthMultiplier: LunaEconomy.TowerHealthMultiplier(LunaDataTable.Instance.playerData.TowerHPLevel),
            shown: tower => SpawnPlayer(Defined.LunaStartPosition, tower.ID, true));
        var p = stageCamera.transform.position; p.x = -stageWidth + 0.01f; p.y = 1.865f; stageCamera.transform.position = p;
    }
    protected override void OnPlayerShown(UnitLogic actor) { if (IntroComplete) FollowCamera = true; }
    protected override bool CanRespawnPlayer => Alive(LunaSpawn.Find<UnitLogic>(allyTowerId));
    protected override void OnFinished(bool won)
    {
        if (!won) return;
        var player = LunaDataTable.Instance.playerData;
        player.UnlockedFloor = Mathf.Max(player.UnlockedFloor, Mathf.Min(30, row.Id + 1));
        if (!HasNextStage) player.CampaignComplete = true;
        player.SaveStageName = HasNextStage ? LunaSession.Current.NextStageString() : StageKey;
    }
    public bool PlayerSkillMPCheck()
    {
        if (Player == null || Player.stat.Mp < 100f) return false;
        Player.stat.Mp = 0f; return true;
    }
    protected override void TickRound(float elapsed, float realElapsed)
    {
        if (!IntroComplete && Input.GetMouseButtonDown(0)) skipIntro = true;
        switch (phase)
        {
            case Phase.Loading:
                if (PendingSpawnCount == 0 && Player != null && gameAllyTower != null && gameEnemyTower != null)
                { timer = 0.8f; phase = Phase.IntroWait; }
                break;
            case Phase.IntroWait:
                timer -= elapsed;
                if (timer > 0f) break;
                foreach (var wave in LunaStageTables.GetWaves(row.Id, "Start"))
                    SpawnFormation(wave, Defined.StayMonsterPosition + Vector3.right * (wave.Order * 0.45f), UnitSpawnMode.Staged,
                        healthMultiplier: row.WaveHealth, attackMultiplier: row.WaveAttack);
                phase = Phase.IntroSpawns;
                break;
            case Phase.IntroSpawns:
                if (PendingSpawnCount == 0) phase = Phase.IntroRight;
                break;
            case Phase.IntroRight:
                if (skipIntro || MoveCameraX(stageWidth - 0.01f, 4f * elapsed)) { timer = 1f; phase = Phase.IntroHold; }
                break;
            case Phase.IntroHold:
                timer -= elapsed;
                if (skipIntro || timer <= 0f) phase = Phase.IntroLeft;
                break;
            case Phase.IntroLeft:
                if (skipIntro || MoveCameraX(-stageWidth + 0.01f, 4f * elapsed))
                {
                    var p = stageCamera.transform.position; p.x = -stageWidth + 0.01f; stageCamera.transform.position = p;
                    StartCombat(true); phase = Phase.Combat;
                }
                break;
            case Phase.Combat:
                if (isStageStop) break;
                StageCount += elapsed;
                if (!Alive(LunaSpawn.Find<UnitLogic>(allyTowerId))) { Finish(false); break; }
                if (!Alive(LunaSpawn.Find<UnitLogic>(enemyTowerId))) { Finish(true); break; }
                if (StageCount >= row.TimeLimit) { Finish(false); break; }
                while (waveIndex < timedWaves.Count && StageCount >= timedWaves[waveIndex].At)
                {
                    var wave = timedWaves[waveIndex++];
                    SpawnFormation(wave, Defined.EnemyTowerPosition - Vector3.right * (wave.Order * 0.24f), UnitSpawnMode.Battle,
                        healthMultiplier: row.WaveHealth, attackMultiplier: row.WaveAttack);
                }
                TickCamera(elapsed, stageWidth - 0.01f);
                break;
            case Phase.NextLoading:
                if (PendingSpawnCount == 0) { timer = 1.8f; phase = Phase.NextWait; }
                break;
            case Phase.NextWait:
                timer -= elapsed;
                if (timer <= 0f) phase = Phase.NextRise;
                break;
            case Phase.NextRise:
                var next = LunaSpawn.Find<LunaStageBackdropLogic>(nextBackgroundId);
                var old = LunaSpawn.Find<LunaStageBackdropLogic>(backgroundId);
                if (next == null) throw new InvalidOperationException("Next floor scenery was hidden during its transition.");
                float rise = Mathf.Min(elapsed * 3f, -next.transform.position.y);
                next.transform.position += Vector3.up * rise;
                if (old != null) old.transform.position += Vector3.up * rise;
                foreach (var unit in Units()) unit.CachedTransform.position += Vector3.up * rise;
                if (next.transform.position.y >= -0.001f) phase = Phase.NextWalk;
                break;
            case Phase.NextWalk:
                var preview = LunaSpawn.Find<UnitLogic>(nextPlayerId);
                if (preview == null) throw new InvalidOperationException("Next floor player was hidden during its transition.");
                float target = stageCamera.transform.position.x + stageWidth - 0.01f + Defined.LunaStartPosition.x;
                var position = preview.CachedTransform.position;
                position.x = Mathf.MoveTowards(position.x, target, elapsed * 5f); preview.CachedTransform.position = position;
                if (Mathf.Abs(position.x - target) < 0.001f) LunaScenes.Load("Game");
                break;
        }
    }
    private bool MoveCameraX(float target, float step)
    {
        var p = stageCamera.transform.position; p.x = Mathf.MoveTowards(p.x, target, step); stageCamera.transform.position = p;
        return Mathf.Abs(p.x - target) < 0.001f;
    }
    public void ToNextStage()
    {
        if (!IsActive || !isStageWin || !HasNextStage || phase >= Phase.NextLoading) return;
        LunaUI.Close(UI_TYPE.RESULT); LeaveResultPresentation(); isStageStop = true;
        string nextKey = LunaSession.Current.NextStageString();
        LunaSession.Current.CurrentStageName = nextKey; LunaSession.Current.SetStagePrefab(nextKey);
        var next = LunaStageTables.Get(nextKey);
        float offset = stageCamera.transform.position.x + stageWidth - 0.01f;
        nextBackgroundId = SpawnBackdrop(LunaStageTables.RequireAsset<GameObject>(next.Background), new Vector3(offset, -15.265f, 0));
        SpawnUnit(LunaStageTables.RequireAsset<GameObject>(next.AllyTower), Defined.AllyTowerPosition + new Vector3(offset, -15.265f, 0), UnitSpawnMode.Staged);
        nextPlayerId = SpawnUnit(LunaDataTable.Instance.playerData.CurPlayer(), new Vector3(offset - 13f, -15.265f, 0), UnitSpawnMode.Staged);
        phase = Phase.NextLoading;
    }
}

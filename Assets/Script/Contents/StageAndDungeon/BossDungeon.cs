using Luna.Gameplay;
using UnityEngine;
using System.Collections.Generic;

public sealed class BossDungeon : LunaGame
{
    private int bossId;
    private float introTimer = 1f;
    private List<LunaStageWaveRow> phases;
    private int phaseIndex;
    public UnitLogic Boss => LunaSpawn.Find<UnitLogic>(bossId);
    protected override UnitLogic Objective => Boss;
    public float BossHealth => Boss != null ? Mathf.Clamp01(Boss.stat.Hp / Mathf.Max(1f, Boss.HealthBarHpPoint)) : 0f;
    public GameObject GamePlayer => Player != null ? Player.gameObject : null;
    public string DungeonName => row.DisplayName;
    public float dungeonCount { get; private set; }
    public override void Initialize(LunaStageDefinition source)
    {
        base.Initialize(source);
        Defined.OriginPosition = Vector3.zero; Defined.LunaStartPosition = new Vector3(-5f, 0f, 0f);
        Defined.EnemyTowerPosition = new Vector3(5f, 0f, 0f); Defined.AllyTowerPosition = new Vector3(-7f, 0f, 0f);
        dungeonCount = row.TimeLimit;
        phases = LunaStageTables.GetWaves(row.Id, "BossPhase");
        SpawnBackdrop(definition.backgroundImage, Vector3.zero);
        bossId = SpawnUnit(definition.bossMonster, Defined.EnemyTowerPosition, UnitSpawnMode.Staged,
            healthMultiplier: row.ObjectiveHealth, attackMultiplier: row.ObjectiveAttack);
        SpawnPlayer(Defined.LunaStartPosition, 0, true);
        var p = stageCamera.transform.position; p.x = 0f; p.y = 1.865f; stageCamera.transform.position = p;
    }
    protected override float ResultHoldDuration => 1f;
    protected override void TickRound(float elapsed, float realElapsed)
    {
        if (!IntroComplete)
        {
            if (PendingSpawnCount != 0 || Player == null || Boss == null) return;
            introTimer -= elapsed;
            if (introTimer <= 0f) StartCombat(false);
            return;
        }
        if (isStageStop) return;
        dungeonCount = Mathf.Max(0f, dungeonCount - elapsed); StageCount += elapsed;
        if (!Alive(Boss)) { Finish(true); return; }
        if (dungeonCount <= 0f) { Finish(false, EndReason.TimeExpired); return; }
        if (Player == null && !PlayerRespawnPending && PendingSpawnCount == 0)
        { Finish(false, EndReason.PlayerDefeated); return; }
        while (phaseIndex < phases.Count && BossHealth <= phases[phaseIndex].At)
        {
            var wave = phases[phaseIndex++];
            SpawnFormation(wave, Defined.EnemyTowerPosition - Vector3.right * (wave.Order * 0.35f), UnitSpawnMode.Battle,
                healthMultiplier: row.WaveHealth, attackMultiplier: row.WaveAttack);
        }
    }
}

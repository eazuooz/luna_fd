using System.Collections.Generic;
using Luna.Gameplay;
using UnityEngine;

public sealed class EndlessDungeon : LunaGame
{
    private enum Phase { InitialLoading, InitialWait, Combat, NextLoading, NextWait, Shift }
    private Phase phase;
    private int backgroundId, nextBackgroundId;
    private float timer = 1f;
    private List<LunaStageWaveRow> waves;
    public GameObject gamePlayer => Player != null ? Player.gameObject : null;
    public int dungeonLevel { get; set; }
    public bool randomYDistance => row.RandomY;
    public string PhaseName => phase.ToString();
    public override void Initialize(LunaStageDefinition source)
    {
        base.Initialize(source);
        Defined.OriginPosition = Vector3.zero; Defined.LunaStartPosition = new Vector3(-5f, 0f, 0f);
        Defined.EnemyTowerPosition = new Vector3(5f, 0f, 0f); Defined.AllyTowerPosition = new Vector3(-7.5f, 0f, 0f);
        waves = LunaStageTables.GetWaves(row.Id, "Endless");
        backgroundId = SpawnBackdrop(definition.backgroundImage, Vector3.zero);
        SpawnPlayer(Defined.LunaStartPosition, 0, true); SpawnWave(false);
        var p = stageCamera.transform.position; p.x = 0f; p.y = 1.865f; stageCamera.transform.position = p;
    }
    protected override float ResultHoldDuration => 1f;
    private void SpawnWave(bool next)
    {
        foreach (var wave in waves)
            for (int i = 0; i < wave.Count; i++)
            {
                float y = randomYDistance ? Random.Range(-0.3f, 1f) : 0f;
                var position = Defined.EnemyTowerPosition + new Vector3(next ? 17.07f : 0f, y, y);
                SpawnUnit(LunaStageTables.RequireAsset<GameObject>(wave.Prefab), position, UnitSpawnMode.Staged, wave.Level);
            }
    }
    protected override void TickRound(float elapsed, float realElapsed)
    {
        switch (phase)
        {
            case Phase.InitialLoading:
                if (PendingSpawnCount == 0 && Player != null) phase = Phase.InitialWait;
                break;
            case Phase.InitialWait:
                timer -= elapsed;
                if (timer <= 0f) { StartCombat(false); phase = Phase.Combat; }
                break;
            case Phase.Combat:
                if (isStageStop) return;
                StageCount += elapsed;
                int enemies = 0;
                foreach (var unit in Units()) if (unit.UnitSide == eUnitSide.Enemy && Alive(unit)) enemies++;
                if (enemies == 0 && PendingSpawnCount == 0)
                {
                    dungeonLevel++;
                    StageUnits(true); isStageStop = true;
                    nextBackgroundId = SpawnBackdrop(definition.backgroundImage, new Vector3(17.07f, 0f, 0f));
                    SpawnWave(true); phase = Phase.NextLoading;
                }
                else if (Player == null && !PlayerRespawnPending && PendingSpawnCount == 0) Finish(false);
                break;
            case Phase.NextLoading:
                if (PendingSpawnCount == 0) { timer = 0.5f; phase = Phase.NextWait; }
                break;
            case Phase.NextWait:
                timer -= elapsed;
                if (timer > 0f) return;
                foreach (var unit in Units())
                    if (unit.UnitSide == eUnitSide.Allied)
                        unit.LobbyDistance = Mathf.Min(0f, Defined.LunaStartPosition.x - unit.transform.position.x + Random.Range(-0.5f, 0.5f));
                phase = Phase.Shift;
                break;
            case Phase.Shift:
                var next = LunaSpawn.Find<LunaStageBackdropLogic>(nextBackgroundId);
                if (next == null) return;
                float distance = Mathf.Min(next.transform.position.x, 5f * elapsed);
                next.transform.position -= Vector3.right * distance;
                var old = LunaSpawn.Find<LunaStageBackdropLogic>(backgroundId);
                if (old != null) old.transform.position += new Vector3(-distance, 0f, elapsed);
                foreach (var unit in Units())
                    if (unit.UnitSide == eUnitSide.Enemy) unit.CachedTransform.position -= Vector3.right * distance;
                    else unit.CachedTransform.position += Vector3.right * elapsed * unit.LobbyDistance * 0.25f;
                if (next.transform.position.x > 0.001f) return;
                next.transform.position = Vector3.zero;
                foreach (var unit in Units())
                    if (unit.UnitSide == eUnitSide.Enemy)
                    { var pos = unit.transform.position; pos.x = Defined.EnemyTowerPosition.x; unit.transform.position = pos; }
                LunaSpawn.Cancel(backgroundId); backgroundId = nextBackgroundId;
                StageUnits(false); isStageStop = false; phase = Phase.Combat;
                break;
        }
    }
    public override void DropItem()
    {
        var selected = new List<LunaStageRewardRow>();
        var bosses = LunaStageTables.GetRewards(row.Id, "EndlessBoss");
        if (bosses.Count > 0) selected.Add(bosses[0]);
        var rewards = LunaStageTables.GetRewards(row.Id, "Victory");
        for (int i = 0; i < rewards.Count; i++)
        {
            var row = rewards[Random.Range(0, rewards.Count)];
            if (Defined.CanProbability(row.Probability * 100f)) selected.Add(row);
        }
        for (int i = 0; i < dungeonLevel; i++) foreach (var reward in selected) SpawnReward(reward, Defined.EnemyTowerPosition);
    }
    protected override void OnFinished(bool won)
    {
        base.OnFinished(won);
        if (!won && dungeonLevel > 0) DropItem();
    }
}

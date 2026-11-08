using Luna.Gameplay;
using UnityEngine;

public sealed class BossDungeon : LunaGame
{
    private int bossId;
    private float introTimer = 1f;
    public GameObject GamePlayer => Player != null ? Player.gameObject : null;
    public string DungeonName => row.DisplayName;
    public float dungeonCount { get; private set; }
    public override void Initialize(LunaStageDefinition source)
    {
        base.Initialize(source);
        Defined.OriginPosition = Vector3.zero; Defined.LunaStartPosition = new Vector3(-5f, 0f, 0f);
        Defined.EnemyTowerPosition = new Vector3(5f, 0f, 0f); Defined.AllyTowerPosition = new Vector3(-7f, 0f, 0f);
        dungeonCount = row.TimeLimit;
        SpawnBackdrop(definition.backgroundImage, Vector3.zero);
        bossId = SpawnUnit(definition.bossMonster, Defined.EnemyTowerPosition, UnitSpawnMode.Staged);
        SpawnPlayer(Defined.LunaStartPosition, 0, true);
        var p = stageCamera.transform.position; p.x = 0f; p.y = 1.865f; stageCamera.transform.position = p;
    }
    protected override float ResultHoldDuration => 1f;
    protected override void TickRound(float elapsed, float realElapsed)
    {
        if (!IntroComplete)
        {
            if (PendingSpawnCount != 0 || Player == null) return;
            introTimer -= elapsed;
            if (introTimer <= 0f) StartCombat(false);
            return;
        }
        if (isStageStop) return;
        dungeonCount = Mathf.Max(0f, dungeonCount - elapsed); StageCount += elapsed;
        if (!Alive(LunaSpawn.Find<UnitLogic>(bossId))) Finish(true);
        else if (dungeonCount <= 0f || (Player == null && !PlayerRespawnPending && PendingSpawnCount == 0)) Finish(false);
    }
}

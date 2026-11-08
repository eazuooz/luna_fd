using UnityEngine;
using Luna.Gameplay;
public class DwarfLogic : UnitLogic
{
    public override bool HasAutomaticSkill => stat.Stamina > 0f;
    protected override void ConfigureUnitForSpawn() { base.ConfigureUnitForSpawn(); unitState.IsTargetingSkill = true; }
    public override void BeginAttack() { SetAnimation("defense", true); }
    public override void BeginSkill()
    {
        stat.Mp = 0f;
        if (Animation.Skeleton.Data.FindAnimation("defense_ready") != null) SetAnimation("defense_ready");
        else SetAnimation("defense", true);
    }
    protected override void OnAnimationComplete(Spine.TrackEntry track)
    {
        if (track.Animation.Name == "defense_ready") SetAnimation("defense", true);
        else base.OnAnimationComplete(track);
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        if (Data == null || IsHiding || unitState.IsLobbyMove || stat.Hp <= 0f) return;
        if (Target == null && Animation.AnimationName == "defense") SetAnimation("idle", true);
        if (Data.TauntRadius <= 0f) return;
        IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).AttackTarget = gameObject,
            IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
            unit => IdentifyFunc.IsInDistance(gameObject, unit, Data.TauntRadius));
    }
}

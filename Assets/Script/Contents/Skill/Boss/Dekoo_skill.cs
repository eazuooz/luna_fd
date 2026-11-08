using UnityEngine;
using Luna.Gameplay;

public class Dekoo_skill : EnemySkillLogic
{
    public string SkillDescription => View.GetText("skillMatter");
    public GameObject effect => View.GetPrefab("effect");
    public float skillDamage => View.GetFloat("skillDamage", 500f);
    public float splashRange => View.GetFloat("splashRange", 2f);

    protected override void ConfigureUnitForSpawn() { unitState.IsTargetingSkill = true; }
    protected override void ExecuteSkill()
    {
        var target = Target;
        if (target == null || !LunaCombat.IsAlive(target.gameObject)) return;
        Vector3 center = target.transform.position;
        if (effect != null) Defined.EffectCreate(effect, center, Data.RoundId);
        IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).Attack(gameObject, skillDamage, false, true),
            unit => UnitLogic.From(unit).UnitSide != UnitSide,
            unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
            unit => Mathf.Abs(unit.transform.position.x - center.x) <= splashRange);
    }
}

using System.Collections;
using UnityEngine;
using Luna.Gameplay;

public class Deus_skill : EnemySkillLogic
{
    public string SkillDescription => View.GetText("skillMatter");
    public GameObject effect => View.GetPrefab("effect");
    public float skillDamage => View.GetFloat("skillDamage", 200f);
    public float splashRange => View.GetFloat("splashRange", 4f);

    protected override void ConfigureUnitForSpawn() { unitState.IsTargetingSkill = true; }
    protected override void ExecuteSkill() { StartCoroutine(InstantSkill()); }
    private IEnumerator InstantSkill()
    {
        Vector3 center = transform.position + Vector3.right * stat.AttackRange * (UnitSide == eUnitSide.Enemy ? -1f : 1f);
        for (int i = 0; i < 3; i++)
        {
            if (effect != null) Defined.EffectCreate(effect, center, Data.RoundId);
            IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).Attack(gameObject, skillDamage, true, true),
                unit => UnitLogic.From(unit).UnitSide != UnitSide,
                unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
                unit => Mathf.Abs(unit.transform.position.x - center.x) <= splashRange);
            yield return new WaitForSeconds(0.25f);
        }
    }
}

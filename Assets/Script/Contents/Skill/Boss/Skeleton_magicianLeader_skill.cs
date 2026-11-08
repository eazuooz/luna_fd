using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton_magicianLeader_skill : EnemySkillLogic
{
    [SerializeField] private string skillMatter = "¸Ê ÀüÃ¼ °ø°Ý";
    public string SkillDescription => skillMatter;
    public GameObject effect => View.GetPrefab("effect");
    public float skillDamage => View.GetFloat("skillDamage", 20.0f);

    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;
    }

    

    protected override void ExecuteSkill() { StartCoroutine(InstantSkill()); }

    IEnumerator InstantSkill()
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(effect, unit);

            UnitLogic.From(unit).Attack(gameObject, skillDamage, true, true);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);

        yield return null;
    }
}

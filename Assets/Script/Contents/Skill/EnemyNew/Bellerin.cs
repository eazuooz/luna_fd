using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.SceneManagement;

public class Bellerin : EnemySkillLogic
{

    public GameObject skillEffect_1 => View.GetPrefab("skillEffect_1");

    // Start is called before the first frame update
    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;


    }

    // Update is called once per frame
    

    protected override void ExecuteSkill()
    {
        if(status.UnitSide == eUnitSide.Enemy)
            StartCoroutine(Skill_1());
        else
            StartCoroutine(Skill_2());
    }

    IEnumerator Skill_1()
    {
        return Wave(-1f, stat.AttackDamage, true);
    }

    IEnumerator Skill_2()
    {
        return Wave(1f, 100f, false);
    }
    private IEnumerator Wave(float direction, float damage, bool targetsAllies)
    {
        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(0.3f);
            Vector3 point = transform.position + Vector3.right * direction * (i * 2f + 2f);
            LunaEffects.Show(skillEffect_1, point, roundId: Data.RoundId);
            IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).TakeBleeding(damage, 5f),
                targetsAllies ? IdentifyFunc.IsAlly : IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower,
                unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
                unit => IdentifyFunc.IsInDistance(point, unit, 2f));
        }
    }
}

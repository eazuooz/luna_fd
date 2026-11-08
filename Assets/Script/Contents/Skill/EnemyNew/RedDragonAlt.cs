using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.SceneManagement;

public class RedDragonAlt : EnemySkillLogic
{
    private int skillControlCounter;

    public GameObject skillEffect_1 => View.GetPrefab("skillEffect_1");
    public GameObject skillEffect_2 => View.GetPrefab("skillEffect_2");

    // Start is called before the first frame update
    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;
        skillControlCounter = 0;
    }

    // Update is called once per frame
    

    protected override void ExecuteSkill()
    {
        skillControlCounter++;

        if (skillControlCounter % 4 == 0)
        {
            Skill_2();
        }
        else
        {
            Skill_1();
        }
    }

    void Skill_1()
    {
        float skillDamage = 400.0f;

        IdentifyFunc.PickingUnit((unit) =>
        {
            Defined.EffectCreate(skillEffect_1, unit, Defined.eEffectPosition.UNDERFEET);

            IdentifyFunc.PickingUnits((unit2) =>
            {
                if (IdentifyFunc.IsInDistance(unit, unit2, 2.0f))
                {
                    UnitLogic.From(unit2).Attack(gameObject, skillDamage, false, true);
                }
            }, IdentifyFunc.IsAlly);

        }, IdentifyFunc.IsAlly);
    }

    void Skill_2()
    {
        StartCoroutine(DuringSkill());
    }

    IEnumerator DuringSkill()
    {
        float skillDamage = 100.0f;

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.2f);

            IdentifyFunc.PickingUnit((unit) =>
            {
                Defined.EffectCreate(skillEffect_2, unit, Defined.eEffectPosition.TARGET);

                UnitLogic.From(unit).Attack(gameObject, skillDamage, false, true);

            }, IdentifyFunc.IsAlly);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.SceneManagement;

public class RedDragon : EnemySkillLogic
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
        StartCoroutine(DuringSkill1());
    }

    void Skill_2()
    {
        StartCoroutine(DuringSkill2());
    }

    IEnumerator DuringSkill1()
    {
        float skillDamage = 40.0f;

        GameObject tmp = null;

        

        IdentifyFunc.PickingUnit((pickedUnit) =>
        {
            tmp = pickedUnit;

        }, IdentifyFunc.IsAlly);

        if(tmp != null)
        {
            int targetId = UnitLogic.From(tmp).ID;
            for (int i = 0; i < 3; i++)
            {
                var target = UnitLogic.Find(targetId);
                if (target == null || target.stat.Hp <= 0f) yield break;
                target.Attack(gameObject, skillDamage, false, true);

                Defined.EffectCreate(skillEffect_1, tmp, Defined.eEffectPosition.UNDERFEET);

                yield return new WaitForSeconds(0.45f);
            }
        }        
    }

    IEnumerator DuringSkill2()
    {
        float skillDamage = 100.0f;

        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(1.0f);

            GameObject tmp = null;

            IdentifyFunc.PickingUnit((unit) =>
            {
                tmp = unit;

            }, IdentifyFunc.IsAlly);

            if (tmp != null)
            {
                IdentifyFunc.PickingUnits((unit2) =>
                {
                    if (IdentifyFunc.IsInDistance(tmp, unit2, 1.0f))
                    {
                        UnitLogic.From(unit2).Attack(gameObject, skillDamage, false, true);
                    }
                }, IdentifyFunc.IsAlly);

                Defined.EffectCreate(skillEffect_2, tmp, Defined.eEffectPosition.UNDERFEET);
            }
        }
    }
}

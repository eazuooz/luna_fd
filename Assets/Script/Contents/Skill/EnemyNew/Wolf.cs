using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.SceneManagement;

public class Wolf : EnemySkillLogic
{
    private int skillControlCounter;

    public GameObject skillEffect_1 => View.GetPrefab("skillEffect_1");
    public GameObject skillEffect_2 => View.GetPrefab("skillEffect_2");
    public GameObject skillEffect_3 => View.GetPrefab("skillEffect_3");

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

        if(skillControlCounter % 9 == 0)
        {
            Skill_3();
        }
        else if (skillControlCounter % 3 == 0)
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
        float skillDamage = 350.0f;

        Vector3 targetPos = gameObject.transform.position + new Vector3(-3.0f, 0.0f, 0.0f);

        Defined.EffectCreate(skillEffect_1, targetPos, Data.RoundId);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if ((IdentifyFunc.IsInDistanceLeft(gameObject, unit, 8.0f)) && !(IdentifyFunc.IsInDistanceLeft(gameObject, unit, 3.0f)))
            {
                UnitLogic.From(unit).Attack(gameObject, skillDamage, false, true);
            }
        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }

    void Skill_2()
    {
        Vector3 targetPos = gameObject.transform.position + new Vector3(-2.0f, 1.5f, 0.0f);

        Defined.EffectCreate(skillEffect_2, targetPos, Data.RoundId);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistanceLeft(gameObject, unit, 3.0f))
            {
                UnitLogic.From(unit).TakeStun(4.0f);
            }
        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }

    void Skill_3()
    {
        StartCoroutine(DuringSkill());
    }

    IEnumerator DuringSkill()
    {
        float skillDamage = 150.0f;

        Vector3 targetPos = gameObject.transform.position + new Vector3(-2.5f, 0.0f, 0.0f);

        int tmpEffect = Defined.EffectCreate(skillEffect_3, targetPos, Data.RoundId);

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1.0f);

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistanceLeft(gameObject, unit, 5.0f))
                {
                    UnitLogic.From(unit).Attack(gameObject, skillDamage, false, true);
                }
            }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
        }

        yield return new WaitForSeconds(0.2f);

        LunaSpawn.Cancel(tmpEffect);
    }
}

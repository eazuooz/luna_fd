using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.SceneManagement;

public class Rugani : EnemySkillLogic
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
        StartCoroutine(Storm());

        if (skillControlCounter % 6 == 0)
        {
            Skill_3();
        }
        else if (skillControlCounter % 2 == 0)
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
        float skillDamage = 100.0f;

        Defined.EffectCreate(skillEffect_1, gameObject, Defined.eEffectPosition.UNDERFEET);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if ((IdentifyFunc.IsInDistanceLeft(gameObject, unit, 5.0f)))
            {
                UnitLogic.From(unit).TakePoisoning(skillDamage, 4.0f);
            }
        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }

    void Skill_2()
    {
        float skillDamage = 100.0f;

        IdentifyFunc.PickingUnit((unit) =>
        {
            Defined.EffectCreate(skillEffect_2, unit, Defined.eEffectPosition.TARGET);

            IdentifyFunc.PickingUnits((unit2) =>
            {
                if (IdentifyFunc.IsInDistance(unit, unit2, 2.5f))
                {
                    UnitLogic.From(unit2).TakePoisoning(skillDamage, 4.0f);
                }
            }, IdentifyFunc.IsAlly, unit2 => UnitLogic.From(unit2).Data.RoundId == Data.RoundId);

        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }

    void Skill_3()
    {
        float skillDamage = 150.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(skillEffect_3, unit, Defined.eEffectPosition.TARGET);

            UnitLogic.From(unit).TakePoisoning(skillDamage, 5.0f);

        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }
    private IEnumerator Storm()
    {
        var cloud = View.GetPrefab("Rugani1.skillEffect_2");
        var bolt = View.GetPrefab("Rugani1.skillEffect_2_1");
        if (cloud != null) LunaEffects.Show(cloud, new Vector3(0f, 3f, 0f), roundId: Data.RoundId);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1f);
            IdentifyFunc.PickingUnits(unit =>
            {
                if (!Defined.CanProbability(35f)) return;
                if (bolt != null) LunaEffects.Show(bolt, unit.transform.position, roundId: Data.RoundId);
                UnitLogic.From(unit).TakeStun(2f);
            }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
        }
    }
}

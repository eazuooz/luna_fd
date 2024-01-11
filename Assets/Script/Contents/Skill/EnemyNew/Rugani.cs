using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class Rugani : MonoBehaviour
{
    public Status status { get; set; }
    private int skillControlCounter;

    public GameObject skillEffect_1;
    public GameObject skillEffect_2;
    public GameObject skillEffect_3;

    // Start is called before the first frame update
    void Start()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = false;
        skillControlCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (status.unitState.IsSkillActivaion == true)
        {
            status.unitState.IsSkillActivaion = false;
            Skill();
        }
    }

    void Skill()
    {
        skillControlCounter++;

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
                unit.GetComponent<Status>().TakePoisoning(skillDamage, 4.0f);
            }
        }, IdentifyFunc.IsAlly);
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
                    unit2.GetComponent<Status>().TakePoisoning(skillDamage, 4.0f);
                }
            }, IdentifyFunc.IsAlly);

        }, IdentifyFunc.IsAlly);
    }

    void Skill_3()
    {
        float skillDamage = 150.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(skillEffect_3, unit, Defined.eEffectPosition.TARGET);

            unit.GetComponent<Status>().TakePoisoning(skillDamage, 5.0f);

        }, IdentifyFunc.IsAlly);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class RedDragonAlt : MonoBehaviour
{
    public Status status { get; set; }
    private int skillControlCounter;

    public GameObject skillEffect_1;
    public GameObject skillEffect_2;

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
                    unit2.GetComponent<Status>().Attack(gameObject, skillDamage, false, true);
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

                unit.GetComponent<Status>().Attack(gameObject, skillDamage, false, true);

            }, IdentifyFunc.IsAlly);
        }
    }
}

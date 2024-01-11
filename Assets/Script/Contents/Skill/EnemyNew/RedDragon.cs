using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class RedDragon : MonoBehaviour
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
            for (int i = 0; i < 3; i++)
            {
                tmp.GetComponent<Status>().Attack(gameObject, skillDamage, false, true);

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
                        unit2.GetComponent<Status>().Attack(gameObject, skillDamage, false, true);
                    }
                }, IdentifyFunc.IsAlly);

                Defined.EffectCreate(skillEffect_2, tmp, Defined.eEffectPosition.UNDERFEET);
            }
        }
    }
}

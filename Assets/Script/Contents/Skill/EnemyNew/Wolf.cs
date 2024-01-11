using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class Wolf : MonoBehaviour
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

        Defined.EffectCreate(skillEffect_1, targetPos);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if ((IdentifyFunc.IsInDistanceLeft(gameObject, unit, 8.0f)) && !(IdentifyFunc.IsInDistanceLeft(gameObject, unit, 3.0f)))
            {
                unit.GetComponent<Status>().Attack(gameObject, skillDamage, false, true);
            }
        }, IdentifyFunc.IsAlly);
    }

    void Skill_2()
    {
        Vector3 targetPos = gameObject.transform.position + new Vector3(-2.0f, 1.5f, 0.0f);

        Defined.EffectCreate(skillEffect_2, targetPos);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistanceLeft(gameObject, unit, 3.0f))
            {
                unit.GetComponent<Status>().TakeStun(4.0f);
            }
        }, IdentifyFunc.IsAlly);
    }

    void Skill_3()
    {
        StartCoroutine(DuringSkill());
    }

    IEnumerator DuringSkill()
    {
        float skillDamage = 150.0f;

        Vector3 targetPos = gameObject.transform.position + new Vector3(-2.5f, 0.0f, 0.0f);

        GameObject tmpEffect = Defined.EffectCreate(skillEffect_3, targetPos);

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1.0f);

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistanceLeft(gameObject, unit, 5.0f))
                {
                    unit.GetComponent<Status>().Attack(gameObject, skillDamage, false, true);
                }
            }, IdentifyFunc.IsAlly);
        }

        yield return new WaitForSeconds(0.2f);

        GameObject.DestroyImmediate(tmpEffect);
    }
}

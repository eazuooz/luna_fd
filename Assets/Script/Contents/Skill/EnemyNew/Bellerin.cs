using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class Bellerin : MonoBehaviour
{
    public Status status { get; set; }
    private int skillControlCounter;

    public GameObject skillEffect_1;

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
        if(status.UnitSide == eUnitSide.Enemy)
            StartCoroutine(Skill_1());
        else
            StartCoroutine(Skill_2());
    }

    IEnumerator Skill_1()
    {
        float bleedDamage = status.stat.AttackDamage;

        GameObject tmp;

        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(0.3f);

            tmp = Defined.EffectCreate(skillEffect_1, gameObject.transform.position - new Vector3(i * 2.0f + 2.0f,0,0));

            IdentifyFunc.PickingUnits((unit) =>
            {
                if ((IdentifyFunc.IsInDistance(tmp, unit, 2.0f)))
                {
                    unit.GetComponent<Status>().TakeBleeding(bleedDamage, 5.0f);
                }
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
        }
    }

    IEnumerator Skill_2()
    {
        float bleedDamage = 100.0f;

        GameObject tmp;

        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(0.3f);

            tmp = Defined.EffectCreate(skillEffect_1, gameObject.transform.position + new Vector3(i * 2.0f + 2.0f, 0, 0));

            Vector3 tmpScale = tmp.transform.Find("Sprite").transform.localScale;
            tmpScale.x *= -1;
            tmp.transform.Find("Sprite").transform.localScale = tmpScale;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if ((IdentifyFunc.IsInDistance(tmp, unit, 2.0f)))
                {
                    unit.GetComponent<Status>().TakeBleeding(bleedDamage, 5.0f);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }
}

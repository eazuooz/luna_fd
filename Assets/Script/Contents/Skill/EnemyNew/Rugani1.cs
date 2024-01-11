using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class Rugani1 : MonoBehaviour
{
    public Status status { get; set; }
    private int skillControlCounter;

    public GameObject skillEffect_2;
    public GameObject skillEffect_2_1;

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

        Skill_2();
    }


    void Skill_2()
    {
        GameObject tmpEffect = Defined.EffectCreate(skillEffect_2, new Vector3(0,3.0f,0));

        StartCoroutine(DuringSkill());
    }

    IEnumerator DuringSkill()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1.0f);

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (Defined.CanProbability(35))
                {
                    GameObject tmpEffect = Defined.EffectCreate(skillEffect_2_1, unit);

                    unit.GetComponent<Status>().TakeStun(2.0f);
                }
            }, IdentifyFunc.IsAlly);
        }

        yield return new WaitForSeconds(0.1f);
    }
}

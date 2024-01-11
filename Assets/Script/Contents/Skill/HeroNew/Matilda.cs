using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matilda : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;
    public GameObject minion1;
    public GameObject minion2;
    public GameObject minion3;

    private bool triggerSkill_1;
    private bool triggerSkill_2;
    private bool triggerSkill_3;

    private float counter1;
    private float counter2;

    protected override void Start()
    {
        base.Start();

        counter1 = 0.0f;
        counter2 = 0.0f;
    }

    protected override void Update()
    {
        base.Update();

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        counter1 += Time.deltaTime;
        counter2 += Time.deltaTime;

        if (triggerSkill_1 == true && counter1 > 13.0f)
        {
            counter1 = 0.0f;
            StartCoroutine(Star5Skill());
        }

        if (triggerSkill_3 == true && counter2 > 30.0f)
        {
            counter2 = 0.0f;
            Ult2Skill();
        }
    }
    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;

        triggerSkill_1 = false;
        triggerSkill_2 = false;
        triggerSkill_3 = false;
    }

    public override void STAR3()
    {
        STAR1();

        status.coefficient.HP += 0.05f;
        status.HpAfterReady();
    }

    public override void STAR5()
    {
        STAR3();

        triggerSkill_1 = true;
    }

    public override void ULT1()
    {
        STAR5();

        triggerSkill_2 = true;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_3 = true;
    }

    public override void Skill()
    {
        StartCoroutine(NormalSkill());
    }

    IEnumerator NormalSkill()
    {
        for (int i = 0; i < 4; i++)
        {
            status.PlayOneSound(skillSound);

            GameObject tmpMinion = GameObject.Instantiate(minion1, GetComponent<Transform>().position + new Vector3(-1.0f, 0, 0.0f), minion1.transform.rotation);

            Defined.EffectCreate(skillEffect, tmpMinion, Defined.eEffectPosition.UNDERFEET, true);

            //if(triggerSkill_2 == true)
            //{
            //    tmpMinion.GetComponent<Status>().Invincible(0);
            //}

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator Star5Skill()
    {
        for (int i = 0; i < 2; i++)
        {
            status.PlayOneSound(skillSound);

            GameObject tmpMinion = GameObject.Instantiate(minion2, GetComponent<Transform>().position + new Vector3(-1.0f, 0, 0.0f), minion2.transform.rotation);

            Defined.EffectCreate(skillEffect, tmpMinion, Defined.eEffectPosition.UNDERFEET, true);

            //if (triggerSkill_2 == true)
            //{
            //    tmpMinion.GetComponent<Status>().Invincible(0);
            //}

            yield return new WaitForSeconds(0.2f);
        }
    }

    void Ult2Skill()
    {
        status.PlayOneSound(skillSound);

        GameObject tmpMinion = GameObject.Instantiate(minion3, GetComponent<Transform>().position + new Vector3(-1.0f, 0, 0.0f), minion3.transform.rotation);

        Defined.EffectCreate(skillEffect, tmpMinion, Defined.eEffectPosition.UNDERFEET, true);
    }
}
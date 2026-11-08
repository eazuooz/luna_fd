using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matilda : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");
    public GameObject minion1 => View.GetPrefab("minion1");
    public GameObject minion2 => View.GetPrefab("minion2");
    public GameObject minion3 => View.GetPrefab("minion3");

    private bool triggerSkill_1;
    private bool triggerSkill_3;

    private float counter1;
    private float counter2;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

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
        base.OnUpdate(elapsed, realElapsed);
    }
    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;

        triggerSkill_1 = false;
        triggerSkill_3 = false;
        counter1 = counter2 = 0f;
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

    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_3 = true;
    }

    public override void Skill()
    {
        if (skill_In_Action) return;
        skill_In_Action = true;
        StartCoroutine(NormalSkill());
    }

    IEnumerator NormalSkill()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayOneSound(skillSound);
            SpawnSummon(minion1, transform.position + Vector3.left, data =>
            { data.Lifetime = 30f; data.InvincibleSeconds = ultState >= eUltState.ULT1 ? -1f : 0f; }, skillEffect);
            yield return new WaitForSeconds(0.2f);
        }
        skill_In_Action = false;
    }

    IEnumerator Star5Skill()
    {
        for (int i = 0; i < 2; i++)
        {
            PlayOneSound(skillSound);
            SpawnSummon(minion2, transform.position + Vector3.left, data =>
            { data.Lifetime = 30f; data.InvincibleSeconds = ultState >= eUltState.ULT1 ? -1f : 0f; }, skillEffect);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void Ult2Skill()
    {
        PlayOneSound(skillSound);
        SpawnSummon(minion3, transform.position + Vector3.left, data => data.Lifetime = 16f, skillEffect);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;

public class Elena : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");
    public GameObject minion => View.GetPrefab("minion");

    private bool triggerSkill_1;
    private bool triggerSkill_2;


    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }


        if(skill_In_Action == true && (CountSummons(minion) == 0))
        {
            status.unitState.IsSkillActivaion = false;
            skill_In_Action = false;
        }
        base.OnUpdate(elapsed, realElapsed);
    }
    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        triggerSkill_1 = triggerSkill_2 = false;
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

        triggerSkill_2 = true;
    }

    public override void Skill()
    {
        if (CountSummons(minion) > 0) return;
        PlayOneSound(skillSound);
        SpawnSummon(minion, transform.position + Vector3.left * 0.5f, data =>
        {
            data.TauntRadius = triggerSkill_1 ? 1.5f : 0f;
            data.InvincibleSeconds = triggerSkill_2 && Defined.CanProbability(80f) ? 5f : 0f;
        }, skillEffect);
        skill_In_Action = true;
    }


}
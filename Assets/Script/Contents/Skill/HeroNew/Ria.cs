using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.SceneManagement;
using Luna.Gameplay;

public class Ria : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");
    public GameObject NormalMinion => View.GetPrefab("NormalMinion");
    public GameObject SkillMinion => View.GetPrefab("SkillMinion");
    public GameObject UltMinion => View.GetPrefab("UltMinion");

    private bool triggerSkill_1;
    private bool triggerSkill_2;
    private bool ultUnlocked;

    private SkeletonAnimation ani;
    private float counter;
    private float normalCounter;

    private bool triggerSkill_In_Action;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        normalCounter += elapsed;
        if (normalCounter >= 6f)
        {
            normalCounter -= 6f;
            NormalSkill();
        }

        if (triggerSkill_In_Action == false)
        {
            counter += Time.deltaTime;

            if (ultUnlocked && counter > 33.0f)
            {
                counter = 0.0f;
                UltSkill();
            }
        }
        else
        {
            if (CountSummons(UltMinion) == 0)
                triggerSkill_In_Action = false;
        }
        base.OnUpdate(elapsed, realElapsed);
    }

    

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;

        triggerSkill_1 = false;
        triggerSkill_2 = false;
        ultUnlocked = false;
        counter = 0.0f;
        normalCounter = 0f;
        triggerSkill_In_Action = false;

        if (ani != null) ani.AnimationState.Event -= OnAttackAnimationEvent;
        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        ani.AnimationState.Event += OnAttackAnimationEvent;
    }

    private void OnAttackAnimationEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "attack" && IsRankReady && Luna.Gameplay.LunaCombat.IsInCombat(status) && normalCounter >= 6f) { normalCounter = 0f; NormalSkill(); }
    }

    public override void AfterDeath()
    {
        if (ani != null) ani.AnimationState.Event -= OnAttackAnimationEvent;
        triggerSkill_In_Action = false;
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
        ultUnlocked = true;
    }

    public override void Skill()
    {
        PlayOneSound(skillSound);
        int count = Mathf.Min(triggerSkill_2 ? 3 : 2, 4 - CountSummons(SkillMinion));
        for (int i = 0; i < count; i++) SpawnSummon(SkillMinion, transform.position + Vector3.left * 0.5f, effect: skillEffect);
    }

    void NormalSkill()
    {
        int limit = triggerSkill_2 ? 3 : 2;
        int count = Mathf.Max(0, limit - CountSummons(NormalMinion));
        for (int i = 0; i < count; i++) SpawnSummon(NormalMinion, transform.position + Vector3.left * 0.5f,
            data => data.ArmourMultiplier = triggerSkill_1 ? 1.15f : 1f, skillEffect);
        if (count > 0) PlayOneSound(skillSound);
    }

    void UltSkill()
    {
        if (CountSummons(UltMinion) > 0) return;
        PlayOneSound(skillSound);
        SpawnSummon(UltMinion, transform.position + Vector3.left * 0.5f, data => data.Lifetime = 60f, skillEffect);
        triggerSkill_In_Action = true;
    }
}
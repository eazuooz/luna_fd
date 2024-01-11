using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Elena : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;
    public GameObject minion;

    private float skillDamage;
    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private GameObject curMinion;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        skillDamage = status.stat.AttackDamage * 3.0f;

        if(skill_In_Action == true && (curMinion == null || curMinion.activeSelf == false))
        {
            status.unitState.IsSkillActivaion = false;
            skill_In_Action = false;
        }
    }
    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
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

        Defined.SkillTrigger_Elena = true;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_2 = true;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        GameObject Dwarf = PoolManager.Instance.spawnObject(minion, GetComponent<Transform>().position + new Vector3(-0.5f, 0, 0.0f), minion.transform.rotation);

        Defined.EffectCreate(skillEffect, Dwarf, Defined.eEffectPosition.UNDERFEET, true);

        if (triggerSkill_1 == true)
        {
            Dwarf.GetComponent<DwarfAI>().SkillTrigger = true;
        }

        if (triggerSkill_2 == true && Defined.CanProbability(80))
        {
            Dwarf.GetComponent<Status>().Invincible(5.0f);
        }

        curMinion = Dwarf;
        skill_In_Action = true;
    }

    public override void AfterDeath()
    {
        Defined.SkillTrigger_Elena = false;

        curMinion = null;
    }
}
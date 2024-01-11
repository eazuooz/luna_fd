using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pring : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;

    private float skillDamage;
    private float coefSkill_1;
    private bool triggerSkill_1;

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

        skillDamage = status.stat.AttackDamage * 2;
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        skillDamage = status.stat.AttackDamage * 3;
        coefSkill_1 = 1.0f;
        triggerSkill_1 = false;
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

        coefSkill_1 = 4.0f;
    }

    public override void ULT1() 
    {
        STAR5();

        status.stat.DebuffAvoidance += 70.0f;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_1 = true;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        float healingRange = 7.0f;
        if (triggerSkill_1 == true && Defined.CanProbability(20))
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, healingRange))
                {
                    Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.TARGET, true);
                    unit.GetComponent<Status>().Heal(skillDamage * coefSkill_1);
                }
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsHero);
        }
        else
        {
            IdentifyFunc.PickingUnit((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, healingRange))
                {
                    Defined.EffectCreate(skillEffect, unit,Defined.eEffectPosition.TARGET, true);
                    unit.GetComponent<Status>().Heal(skillDamage * coefSkill_1);
                }
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
        }
    }
}

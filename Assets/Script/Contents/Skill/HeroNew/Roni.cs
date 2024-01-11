using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Roni : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;

    private float healPoint;
    private float coefSkill_1;

    private SkeletonAnimation SkeletonAnimation;

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

        healPoint = status.stat.AttackDamage * 2;
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        healPoint = status.stat.AttackDamage * 3;
        coefSkill_1 = 1.0f;
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

        status.coefficient.Armour += 0.12f;
        status.StatusAfterReady();
    }

    public override void ULT1() 
    {
        STAR5();

        coefSkill_1 = 2.5f;
    }

    public override void ULT2()
    {
        ULT1();

        Defined.SkillTrigger_Roni = true;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        float healingRange = 3.5f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, healingRange))
            {
                Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.TARGET, true);

                unit.GetComponent<Status>().Heal(healPoint * coefSkill_1);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsHero);
    }

    public override void AfterDeath()
    {
        Defined.SkillTrigger_Roni = false;
    }
}

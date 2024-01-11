using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miu : HeroSkillControl
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

        skillDamage = status.stat.AttackDamage * 2.0f;
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        skillDamage = status.stat.AttackDamage * 3.5f;
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

        triggerSkill_1 = true;
    }

    public override void ULT1() 
    {
        STAR5();

        coefSkill_1 = 1.5f;
    }

    public override void ULT2()
    {
        ULT1();

        status.stat.SkillAvoidance = 1000.0f;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        float healingRange = 5.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, healingRange))
            {
                Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.TARGET, true);
                unit.GetComponent<Status>().Heal(skillDamage * coefSkill_1);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }
}

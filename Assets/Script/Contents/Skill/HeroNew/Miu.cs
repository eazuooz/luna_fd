using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miu : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;

    private float skillDamage;
    private float coefSkill_1;

    protected override void OnEntityInitialize()
    {
        base.OnEntityInitialize();
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        base.OnEntityUpdate(elapsed, realElapsed);

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

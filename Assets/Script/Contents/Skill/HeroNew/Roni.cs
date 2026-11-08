using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Roni : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");

    private float healPoint => status.stat.AttackDamage * 2f;
    private float coefSkill_1;

    private SkeletonAnimation SkeletonAnimation;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;

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

                UnitLogic.From(unit).Heal(healPoint * coefSkill_1);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsHero);
    }


}

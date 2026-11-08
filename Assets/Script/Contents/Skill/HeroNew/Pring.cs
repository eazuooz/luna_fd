using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pring : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");

    private float skillDamage;
    private float coefSkill_1;
    private bool triggerSkill_1;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        skillDamage = 1380f;
        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        skillDamage = 1380f;
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
                if (IdentifyFunc.IsInDistance(gameObject, unit, healingRange))
                {
                    Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.TARGET, true);
                    UnitLogic.From(unit).Heal(skillDamage * coefSkill_1);
                }
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsHero);
        }
        else
        {
            IdentifyFunc.PickingUnit((unit) =>
            {
                if (IdentifyFunc.IsInDistance(gameObject, unit, healingRange))
                {
                    Defined.EffectCreate(skillEffect, unit,Defined.eEffectPosition.TARGET, true);
                    UnitLogic.From(unit).Heal(skillDamage * coefSkill_1);
                }
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower, unit => IdentifyFunc.IsInDistance(gameObject, unit, healingRange) && UnitLogic.From(unit).stat.Hp < UnitLogic.From(unit).HealthBarHpPoint);
        }
    }
}

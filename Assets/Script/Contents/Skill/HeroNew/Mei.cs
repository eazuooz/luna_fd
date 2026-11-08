using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mei : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");

    private float skillDamage => status.stat.AttackDamage * 5f * skillMultiplier;
    private float skillMultiplier;
    private bool triggerSkill_1;

    

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
        status.unitState.IsTargetingSkill = true;
        skillMultiplier = 1f;

        triggerSkill_1 = false;
    }

    public override void STAR3()
    {
        STAR1();

        skillMultiplier = 1.05f;
        status.StatusAfterReady();

    }

    public override void STAR5() 
    {
        STAR3();

        skillMultiplier *= 4f;

    }

    public override void ULT1() 
    {
        STAR5();

        status.stat.StunResistance += 20f;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_1 = true;
    }

    public override void Skill()
    {
        float splashRange;

        if (IdentifyFunc.IsAlive(status.AttackTarget))
        {
            splashRange = 4.0f;

            Defined.EffectCreate(skillEffect, status.AttackTarget, Defined.eEffectPosition.UNDERFEET);

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, splashRange))
                {
                    UnitLogic.From(unit).Attack(gameObject, skillDamage, true, true);

                    if (triggerSkill_1 == true && Defined.CanProbability(40))
                    {
                        UnitLogic.From(unit).TakeStun(3.5f);
                    }
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }
}

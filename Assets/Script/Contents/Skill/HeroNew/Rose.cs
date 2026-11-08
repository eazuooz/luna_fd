using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rose : HeroUnitLogic
{
    private float skillDamage => status.stat.AttackDamage;
    private float coefSkill_1;
    private float coefSkill_2;

    

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

        coefSkill_1 = 5.0f;
        coefSkill_2 = 5.0f;
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

        coefSkill_1 += 2.0f;
    }

    public override void ULT1() 
    {
        STAR5();

        coefSkill_2 += 2.0f;
    }

    public override void ULT2()
    {
        ULT1();

    }

    public override void Skill()
    {
        float skillRange = 5.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsAlly(unit) && IdentifyFunc.IsMelee(unit) && Defined.CanProbability(80))
            {
                UnitLogic.From(unit).TakeBerserkMode(coefSkill_1);
            }

            if (IdentifyFunc.IsEnemy(unit) && IdentifyFunc.IsInDistance(gameObject, unit, skillRange) && Defined.CanProbability(50))
            {
                UnitLogic.From(unit).TakeBleeding(skillDamage, coefSkill_2);
            }
        }, IdentifyFunc.IsNotTower);
    }


}

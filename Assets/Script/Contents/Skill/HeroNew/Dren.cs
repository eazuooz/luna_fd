using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dren : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");

    private float skillDamage => status.stat.AttackDamage * 3f;
    private float coefSkill_1;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;

        coefSkill_1 = 1.0f;
    }


    public override void STAR3()
    {
        STAR1();

        status.coefficient.AttackDamage += 0.15f;
        status.StatusAfterReady();
    }

    public override void STAR5() 
    {
        STAR3();

        coefSkill_1 = 1.2f;
    }

    public override void ULT1() 
    {
        STAR5();

        //드래곤에게 받는 대미지 감소
    }

    public override void ULT2()
    {
        ULT1();

        
    }

    public override void Skill()
    {
        Vector3 attackPos;
        float splashRange;

        if (IdentifyFunc.IsAlive(status.AttackTarget))
        {
            Defined.EffectCreate(skillEffect, status.AttackTarget, Defined.eEffectPosition.UNDERFEET);

            attackPos = status.AttackTarget.transform.position;
            splashRange = 1.8f;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, splashRange))
                {
                    UnitLogic.From(unit).Attack(gameObject, skillDamage * coefSkill_1, true, true);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }

}

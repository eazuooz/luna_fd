using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clare : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");

    private float skillDamage => status.stat.AttackDamage * 3f;

    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private float skillCount;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }


        if (triggerSkill_2 == true)
        {
            skillCount -= Time.deltaTime;

            if (skillCount < 0.0f)
            {
                newSkill();
            }
        }
        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;

        triggerSkill_1 = false;
        triggerSkill_2 = false;

        skillCount = 13.0f;
    }

    public override void STAR3()
    {
        STAR1();

        status.coefficient.AttackDamage += 0.05f;
        status.StatusAfterReady();
    }

    public override void STAR5() 
    {
        STAR3();

        status.StartWarf(3.0f);
    }

    public override void ULT1() 
    {
        STAR5();

        triggerSkill_1 = true;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_2 = true;
    }

    public override void Skill()
    {
        if (!Defined.CanProbability(60f)) return;
        if (triggerSkill_1 && Defined.CanProbability(70f))
            IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).TakeBleeding(skillDamage, 5f),
                IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower, unit => IdentifyFunc.IsInDistance(gameObject, unit, 4f));
        else if (IdentifyFunc.IsAlive(status.AttackTarget) && IdentifyFunc.IsInDistance(gameObject, status.AttackTarget, 4f))
            UnitLogic.From(status.AttackTarget).TakeBleeding(skillDamage, 5f);
    }

    void newSkill()
    {
        Defined.EffectCreate(skillEffect, gameObject, Defined.eEffectPosition.TARGET, true);

        status.Stealth(3.5f);

        skillCount = 13.0f;
    }
}

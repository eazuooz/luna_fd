using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tira : HeroUnitLogic
{
    // public AudioClip skillSound => View.GetSound("skillSound");

    private float durationEnemy;

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

        if (triggerSkill_1 == true)
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
        status.unitState.IsTargetingSkill = false;
        durationEnemy = 4.0f;
        triggerSkill_1 = false;
        triggerSkill_2 = false;

        skillCount = 20.0f;
    }

    public override void STAR3()
    {
        STAR1();

    }

    public override void STAR5()
    {
        STAR3();

        triggerSkill_1 = true;
    }

    public override void ULT1()
    {
        STAR5();

        triggerSkill_2 = true;
    }

    public override void ULT2()
    {
        ULT1();


    }

    public override void Skill()
    {
        //status.PlayOneSound(skillSound);

        if (triggerSkill_2 == true)
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                UnitLogic.From(unit).GrowUp(5.0f);
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower, IdentifyFunc.IsMelee);
        }
        else
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                UnitLogic.From(unit).GrowUp(5.0f);
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsPlayer);
        }
    }

    void newSkill()
    {
        if (status.AttackTarget != null)
        {
            //   status.PlayOneSound(skillSound);
            UnitLogic.From(status.AttackTarget).GrowDownSkill(durationEnemy);

            skillCount = 20.0f;
        }
    }
}

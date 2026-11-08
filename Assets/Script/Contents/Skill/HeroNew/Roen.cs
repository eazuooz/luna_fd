using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roen : HeroUnitLogic
{
    //public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");


    

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

    }

    public override void ULT1()
    {
        STAR5();

    }

    public override void ULT2()
    {
        ULT1();

    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        float skillRange = 4.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, skillRange))
            {
                UnitLogic.From(unit).RoenSkill(3.0f);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }


}

using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pochi : HeroUnitLogic
{
    public AudioClip skillSound => View.GetSound("skillSound");

    private float duration;

    private float coefSkill_1;

    

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
        duration = 3.0f;
        coefSkill_1 = 0.0f;
    }

    public override void STAR3()
    {
        STAR1();

        coefSkill_1 = 2.0f;
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
        float reflect = status.ultState >= eUltState.STAR5 ? 0.3f : 0f;
        IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).TakePochiBarrier(duration + coefSkill_1, reflect),
            IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }


}

using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goldy : HeroUnitLogic
{
    

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
        //status.skillTargetNeed = false;
        status.unitState.IsTargetingSkill = false;
    }

    public override void STAR3()
    {
        STAR1();

        status.unitState.IsTargetingSkill = false;
    }

    public override void STAR5() 
    {
        STAR3();

        status.unitState.IsTargetingSkill = false;
    }

    public override void ULT1() 
    {
        STAR5();

        status.unitState.IsTargetingSkill = false;
    }

    public override void ULT2()
    {
        ULT1();

        status.unitState.IsTargetingSkill = false;
    }

    public override void Skill()
    {
    }
}

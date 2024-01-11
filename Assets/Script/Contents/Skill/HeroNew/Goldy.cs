using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goldy : HeroSkillControl
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }
    }

    public override void STAR1()
    {
        //status.skillTargetNeed = false;
        Defined.SkillTrigger_Goldy = 6.0f;
    }

    public override void STAR3()
    {
        STAR1();

        Defined.SkillTrigger_Goldy = 7.0f;
    }

    public override void STAR5() 
    {
        STAR3();

        Defined.SkillTrigger_Goldy = 8.0f;
    }

    public override void ULT1() 
    {
        STAR5();

        Defined.SkillTrigger_Goldy = 9.0f;
    }

    public override void ULT2()
    {
        ULT1();

        Defined.SkillTrigger_Goldy = 10.0f;
    }

    public override void Skill()
    {
    }
}

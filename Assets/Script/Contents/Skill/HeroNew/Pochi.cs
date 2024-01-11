using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pochi : HeroSkillControl
{
    public AudioClip skillSound;

    private float duration;

    private float coefSkill_1;
    private bool triggerSkill_1;

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
        status.unitState.IsTargetingSkill = false;
        duration = 5.0f;
        coefSkill_1 = 0.0f;
        triggerSkill_1 = false;
    }

    public override void STAR3()
    {
        STAR1();

        coefSkill_1 = 2.0f;
    }

    public override void STAR5() 
    {
        STAR3();

        Defined.SkillTrigger_Pochi = true;
    }

    public override void ULT1() 
    {
        STAR5();

        triggerSkill_1 = true;
    }

    public override void ULT2()
    {
        ULT1();

        
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        bool AllSkill = (triggerSkill_1 && Defined.CanProbability(20));
        bool OnceSkill = true;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (OnceSkill == true)
            {
                unit.GetComponent<Status>().TakePochiBarrier(duration + coefSkill_1);

                OnceSkill = AllSkill;
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }

    public override void AfterDeath()
    {
        Defined.SkillTrigger_Pochi = false;
    }
}

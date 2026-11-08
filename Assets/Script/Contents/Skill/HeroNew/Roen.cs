using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roen : HeroSkillControl
{
    //public GameObject skillEffect;
    public AudioClip skillSound;

    private float skillDamage;

    protected override void OnEntityInitialize()
    {
        base.OnEntityInitialize();
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        base.OnEntityUpdate(elapsed, realElapsed);

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        skillDamage = status.stat.AttackDamage * 5;
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        skillDamage = status.stat.AttackDamage* 5;
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

        Defined.SkillTrigger_Roen1 = true;
    }

    public override void ULT1()
    {
        STAR5();

        Defined.SkillTrigger_Roen2 = true;
    }

    public override void ULT2()
    {
        ULT1();

        Defined.SkillTrigger_Roen3 = true;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        float skillRange = 4.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, skillRange))
            {
                unit.GetComponent<Status>().RoenSkill(10.0f);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsSoldier);
    }

    public override void AfterDeath()
    {
        Defined.SkillTrigger_Roen3 = false;
        Defined.SkillTrigger_Roen2 = false;
        Defined.SkillTrigger_Roen1 = false;
    }
}

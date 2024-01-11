using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rose : HeroSkillControl
{
    private float skillDamage;
    private float coefSkill_1;
    private float coefSkill_2;

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

        skillDamage = status.stat.AttackDamage;
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;
        skillDamage = status.stat.AttackDamage * 1.1f;
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

        Defined.SkillTrigger_Rose = true;
    }

    public override void Skill()
    {
        float skillRange = 5.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsAlly(unit) && Defined.CanProbability(/*80*/100))
            {
                unit.GetComponent<Status>().TakeBerserkMode(coefSkill_1);
            }

            if (IdentifyFunc.IsEnemy(unit) && IdentifyFunc.IsInDistance(gameObject, unit, skillRange) && Defined.CanProbability(/*80*/100))
            {
                unit.GetComponent<Status>().TakeBleeding(skillDamage, coefSkill_2);
            }
        }, IdentifyFunc.IsSoldier);
    }

    public override void AfterDeath()
    {
        Defined.SkillTrigger_Rose = false;
    }
}

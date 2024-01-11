using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mei : HeroSkillControl
{
    public GameObject skillEffect;

    private float skillDamage;
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

        skillDamage = status.stat.AttackDamage * 5;
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;
        skillDamage = status.stat.AttackDamage * 5;//18;
        triggerSkill_1 = false;
    }

    public override void STAR3()
    {
        STAR1();

        //status.stat.CoefficientSkillPoint += 0.05f;
        status.StatusAfterReady();
        skillDamage = status.stat.AttackDamage * 5;
    }

    public override void STAR5() 
    {
        STAR3();

        skillDamage += status.stat.AttackDamage * 8;
    }

    public override void ULT1() 
    {
        STAR5();

        /*
        status.stunResist += 0.2f;
        status.StatusAfterReady();
        */
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_1 = true;
    }

    public override void Skill()
    {
        float splashRange;

        if (status.AttackTarget != null)
        {
            splashRange = 4.0f;

            Defined.EffectCreate(skillEffect, status.AttackTarget, Defined.eEffectPosition.UNDERFEET);

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistanceRight(status.AttackTarget, unit, splashRange))
                {
                    unit.GetComponent<Status>().Attack(gameObject, skillDamage, true, true);

                    if (triggerSkill_1 == true && Defined.CanProbability(40))
                    {
                        unit.GetComponent<Status>().TakeStun(3.5f);
                    }
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }
}

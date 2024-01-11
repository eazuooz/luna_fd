using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clare : HeroSkillControl
{
    public GameObject skillEffect;

    private float skillDamage;

    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private float skillCount;

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

        skillDamage = status.stat.AttackDamage * 3.0f;

        if (triggerSkill_2 == true)
        {
            skillCount -= Time.deltaTime;

            if (skillCount < 0.0f)
            {
                newSkill();
            }
        }
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;
        skillDamage = status.stat.AttackDamage * 3.0f;
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
        float skillRange = 4.0f;
        bool tmpSkillTrigger = triggerSkill_1 && Defined.CanProbability(70);


        if (triggerSkill_1 && Defined.CanProbability(70))
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistanceRight(status.AttackTarget, unit, skillRange))
                {
                    unit.GetComponent<Status>().TakeBleeding(skillDamage, 5.0f);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
        else
        {
            if (status.AttackTarget != null)
            {
                status.AttackTarget.GetComponent<Status>().TakeBleeding(skillDamage, 5.0f);
            }
        }
    }

    void newSkill()
    {
        Defined.EffectCreate(skillEffect, gameObject, Defined.eEffectPosition.TARGET, true);

        status.Stealth(3.5f);

        skillCount = 13.0f;
    }
}

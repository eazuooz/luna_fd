using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dren : HeroSkillControl
{
    public GameObject skillEffect;

    private float skillDamage;
    private float coefSkill_1;
    private float coefSkill_2;
    private float coefSkill_3;
    private float coefSkill_4;
    private bool triggerSkill_1;
    private bool triggerSkill_2;

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

        if (triggerSkill_1 == true)
        {
            if (triggerSkill_2 == false)
            {
                if (status.AttackTarget != null/* && 공중 적군*/)
                {
                    triggerSkill_2 = true;

                    status.coefficient.AttackDamage += 0.3f;
                    status.StatusAfterReady();
                }
            }
            else
            {
                if (status.AttackTarget == null/* || 공중 적군X*/)
                {
                    triggerSkill_2 = false;

                    status.coefficient.AttackDamage -= 0.3f;
                    status.StatusAfterReady();
                }
            }
        }
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;
        skillDamage = status.stat.AttackDamage * 3.0f;
        coefSkill_1 = 1.0f;
        triggerSkill_1 = false;
        triggerSkill_2 = false;
    }


    public override void STAR3()
    {
        STAR1();

        triggerSkill_1 = true;
    }

    public override void STAR5() 
    {
        STAR3();

        coefSkill_1 = 1.2f;
    }

    public override void ULT1() 
    {
        STAR5();

        //드래곤에게 받는 대미지 감소
    }

    public override void ULT2()
    {
        ULT1();

        
    }

    public override void Skill()
    {
        Vector3 attackPos;
        float splashRange;

        if (status.AttackTarget != null)
        {
            Defined.EffectCreate(skillEffect, status.AttackTarget, Defined.eEffectPosition.UNDERFEET);

            attackPos = status.AttackTarget.transform.position;
            splashRange = 1.8f;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, splashRange))
                {
                    unit.GetComponent<Status>().Attack(gameObject, skillDamage * coefSkill_1/* * coefSkill_2*/, true);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }

}

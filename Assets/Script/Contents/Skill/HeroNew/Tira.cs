using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tira : HeroSkillControl
{
    // public AudioClip skillSound;

    private float durationAlly;
    private float durationEnemy;
    private float skillDamageAlly;
    private float skillDamageEnemy;

    private float coefSkill_1;
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

        if (triggerSkill_1 == true)
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
        status.unitState.IsTargetingSkill = false;
        skillDamageAlly = 40.0f;
        skillDamageEnemy = 50.0f;
        durationAlly = 5.0f;
        durationEnemy = 4.0f;
        coefSkill_1 = 0.0f;
        triggerSkill_1 = false;
        triggerSkill_2 = false;

        skillCount = 20.0f;
    }

    public override void STAR3()
    {
        STAR1();

        coefSkill_1 = 10.0f;
    }

    public override void STAR5() 
    {
        STAR3();

        triggerSkill_1 = true;
    }

    public override void ULT1() 
    {
        STAR5();

        triggerSkill_2 = true;
    }

    public override void ULT2()
    {
        ULT1();

        
    }

    public override void Skill()
    {
        //status.PlayOneSound(skillSound);

        if (triggerSkill_2 == true)
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.GetComponent<Status>().GrowUp(5.0f);
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower, IdentifyFunc.IsMelee);
        }
        else
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.GetComponent<Status>().GrowUp(5.0f);
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsPlayer);
        }
    }

    void newSkill()
    {
        if (status.AttackTarget != null)
        {
            //   status.PlayOneSound(skillSound);
            status.AttackTarget.GetComponent<Status>().GrowDownSkill(durationEnemy);

            skillCount = 20.0f;
        }
    }
}

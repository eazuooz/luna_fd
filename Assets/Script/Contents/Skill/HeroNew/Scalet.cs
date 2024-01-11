using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scalet : HeroSkillControl
{
    public GameObject skillEffect;

    private float skillDamage;
    private float coefSkill_1;
    private float coefSkill_2;
    private float coefSkill_3;
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
        skillDamage = status.stat.AttackDamage * 5;
        coefSkill_1 = 1.0f;
        coefSkill_2 = 8.0f;
        coefSkill_3 = 1.0f;
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

        coefSkill_2 = 10.0f;
    }

    public override void ULT1() 
    {
        STAR5();

        //status.SkillResistance += 0.2f;
        status.StatusAfterReady();
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_1 = true;
    }

    public override void Skill()
    {
        Vector3 attackPos;
        float splashRange;

        if (status.AttackTarget != null)
        {
            attackPos = status.AttackTarget.transform.position;
            splashRange = 3.0f;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, splashRange))
                {
                    StartCoroutine(SequanceSkill(unit));
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }

    IEnumerator SequanceSkill(GameObject target)
    {
        yield return new WaitForSeconds(Random.Range(0,0.5f));

        if (target != null)
        {
            Defined.EffectCreate(skillEffect, target);
            target.GetComponent<Status>().Attack(gameObject, skillDamage * coefSkill_1 * coefSkill_2, true, true);

            if (triggerSkill_1 == true && Defined.CanProbability(80))
            {
                if (target != null)
                { // ¿˙¡÷
                //    target.GetComponent<Status>().Curse();
                }
            }
        }
    }
}

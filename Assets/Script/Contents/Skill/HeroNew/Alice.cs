using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alice : HeroSkillControl
{
    public GameObject skillEffect1;
    public GameObject skillEffect2;

    private float skillDamage;
    private float coefSkill_1;
    private float coefSkill_2;
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
    }
    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;
        coefSkill_1 = 1.0f;
        coefSkill_2 = 3.0f;
        triggerSkill_1 = false;
        triggerSkill_2 = false;
    }

    public override void STAR3()
    {
        STAR1();

        coefSkill_1 = 1.05f;
    }

    public override void STAR5() 
    {
        STAR3();

        triggerSkill_1 = true;
    }

    public override void ULT1() 
    {
        STAR5();

        coefSkill_2 = 5.0f;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_2 = true;
    }

    public override void Skill()
    {
        Defined.EffectCreate(skillEffect1, status.AttackTarget, Defined.eEffectPosition.TARGET, true);

        StartCoroutine(DelaySkill());
    }

    IEnumerator DelaySkill()
    {
        yield return new WaitForSeconds(1.0f);

        if (triggerSkill_1 == true && Defined.CanProbability(70))
        {
            StartCoroutine(RepeatSkill(3));
        }
        else
        {
            StartCoroutine(RepeatSkill(1));
        }
    }

    IEnumerator RepeatSkill(int repeatTime)
    {
        for (int i = 0; i < repeatTime; i++)
        {
            SelectSkill();

            yield return new WaitForSeconds(0.3f);
        }
    }

    void SelectSkill()
    {
        if (triggerSkill_2 == true && Defined.CanProbability(25))
        {
            Ult2Skill();
        }
        else
        {
            NormalSkill();
        }
    }

    void NormalSkill()
    {
        if (status.AttackTarget != null)
        {
            Defined.EffectCreate(skillEffect2, status.AttackTarget);

            status.AttackTarget.GetComponent<Status>().Attack(gameObject, skillDamage * coefSkill_1 * coefSkill_2, true, true);
        }
    }

    void Ult2Skill()
    {
        Vector3 attackPos;
        float splashRange;

        if (status.AttackTarget != null)
        {
            Defined.EffectCreate(skillEffect2, status.AttackTarget);

            attackPos = status.AttackTarget.transform.position;
            splashRange = 3.0f;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, splashRange))
                {
                    unit.GetComponent<Status>().Attack(gameObject, skillDamage * coefSkill_1 * coefSkill_2, Defined.CanProbability(status.stat.CriticalActuationProbability), true);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }
}
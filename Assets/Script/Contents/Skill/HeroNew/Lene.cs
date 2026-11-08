using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lene : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;

    private float skillDamage;
    private const float AllyProcChance = 40.0f;
    private float selfProcChance;
    private float coefSkill_2;
    private bool triggerSkill_2;

    private float skillCount;

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
        status.unitState.IsTargetingSkill = false;
        skillDamage = status.stat.AttackDamage * 0.65f;
        selfProcChance = AllyProcChance;
        coefSkill_2 = 10.0f;
        triggerSkill_2 = false;

        skillCount = 3.0f;
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
        selfProcChance += 40.0f;
    }

    public override void ULT1()
    {
        STAR5();

        selfProcChance += 20.0f;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_2 = true;
    }

    public override void Skill()
    {
        if (Defined.CanProbability(selfProcChance))
        {
            status.TakeBerserkMode(coefSkill_2);
        }

        float skillRange = 5.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, skillRange))
            {
                if (Defined.CanProbability(AllyProcChance))
                {
                    unit.GetComponent<Status>().TakeBerserkMode(coefSkill_2);
                }
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsSoldier);
    }

    void newSkill()
    {
        if (status.AttackTarget != null)
        {
            status.PlayOneSound(skillSound);

            Defined.EffectCreate(skillEffect, status.AttackTarget, Defined.eEffectPosition.UNDERFEET);

            StartCoroutine(DelaySkill());

            skillCount = 3.0f;
        }
    }

    IEnumerator DelaySkill()
    {
        yield return new WaitForSeconds(0.36f);

        if (status.AttackTarget != null)
        {
            status.AttackTarget.GetComponent<Status>().Attack(gameObject, skillDamage * 2, true, true);
        }
    }
}

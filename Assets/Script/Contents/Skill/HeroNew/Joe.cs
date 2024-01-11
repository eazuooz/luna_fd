using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Joe : HeroSkillControl
{
    public GameObject skillEffect;

    private float skillDamage;
    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private SkeletonAnimation ani;

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
        skillDamage = status.stat.AttackDamage * 5.0f * 1.2f;
        triggerSkill_1 = false;
        triggerSkill_2 = false;

        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
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

        triggerSkill_1 = true;
    }

    public override void ULT1()
    {
        STAR5();

        ani.state.Start += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "attack")
            {
                if (status.AttackTarget != null && Defined.CanProbability(15))
                {
                    status.isStunAttack = true;
                    status.nextStunDuring = 2.0f;
                }
            }
        };

    }

    public override void ULT2()
    {
        ULT1();

        StartCoroutine(AlterSkill());
    }

    public override void Skill()
    {
        if (status.AttackTarget != null)
        {
            GameObject tmpEffect = Defined.EffectCreate(skillEffect, status.AttackTarget);
            tmpEffect.GetComponent<SkillBomb>().Damage = skillDamage;
        }
    }

    IEnumerator AlterSkill()
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(20.0f);

            status.TakeBerserkMode(4.0f);
        }
    }
}

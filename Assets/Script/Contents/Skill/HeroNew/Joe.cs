using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Joe : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");

    private float skillDamage => status.stat.AttackDamage * 3f;
    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private SkeletonAnimation ani;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }


        if (triggerSkill_1 == true)
        {
            if (triggerSkill_2 == false)
            { 
                if (IdentifyFunc.IsAlive(status.AttackTarget) && IdentifyFunc.IsAirborne(status.AttackTarget))
                {
                    triggerSkill_2 = true;

                    status.coefficient.AttackDamage += 0.3f;
                    status.StatusAfterReady();
                }
            }
            else
            {
                if (!IdentifyFunc.IsAlive(status.AttackTarget) || !IdentifyFunc.IsAirborne(status.AttackTarget))
                {
                    triggerSkill_2 = false;

                    status.coefficient.AttackDamage -= 0.3f;
                    status.StatusAfterReady();
                }
            }
        }
        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;

        triggerSkill_1 = false;
        triggerSkill_2 = false;

        if (ani != null) ani.AnimationState.Start -= OnAttackAnimationStart;
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

        ani.AnimationState.Start -= OnAttackAnimationStart;
        ani.AnimationState.Start += OnAttackAnimationStart;
    }

    private void OnAttackAnimationStart(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == "attack" && status.AttackTarget != null && Defined.CanProbability(15))
        {
            status.isStunAttack = true;
            status.nextStunDuring = 2.0f;
        }
    }

    public override void AfterDeath()
    {
        if (ani != null) ani.AnimationState.Start -= OnAttackAnimationStart;
    }

    public override void ULT2()
    {
        ULT1();

        StartCoroutine(AlterSkill());
    }

    public override void Skill()
    {
        if (IdentifyFunc.IsAlive(status.AttackTarget))
        {
            var data = SkillBombData.Create(this, status.AttackTarget.transform.position, skillDamage);
            LunaSpawn.Show<SkillBomb>(skillEffect, data);
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

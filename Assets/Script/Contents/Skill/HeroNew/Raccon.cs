using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Raccon : HeroSkillControl
{
    public AudioClip skillSound;

    private float skillDamage;
    private bool triggerSkill_1;
    private bool triggerSkill_2;
    private bool triggerSkill_3;

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

        skillDamage = status.stat.AttackDamage;

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

        if (triggerSkill_3 == true)
        {
            if (status.AttackTarget == null)
            {
                transform.Translate(Vector3.right * status.stat.MoveSpeed * Time.deltaTime * 3.0f);
            }
        }
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;
        skillDamage = status.stat.AttackDamage * 1.8f;
        triggerSkill_1 = false;
        triggerSkill_2 = false;
        triggerSkill_3 = false;

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

        //status.stat.CoefficientSkillPoint += 0.15f;
        status.StatusAfterReady();
    }

    public override void ULT2()
    {
        ULT1();

        status.stat.Stamina *= 0.9f;
    }

    public override void Skill()
    {
        StartCoroutine(DuringSkill());
    }

    public override void SkillMove()
    {
        RacconSkillMove();
    }
    private void RacconSkillMove()
    {
        if (gameObject.transform.position.x >= (Defined.EnemyTowerPosition.x - status.stat.AttackRange))
        {
            return;
        }

        float moveSpeed = gameObject.GetComponent<Status>().stat.MoveSpeed;
        gameObject.GetComponent<Transform>().Translate(Vector3.right * moveSpeed * Time.deltaTime);
    }
    IEnumerator DuringSkill()
    {
        float skillRange = 2.0f;

        status.unitState.IsSuperArmour = true;
        status.unitState.IsActionDelay = true;
        status.stat.AttackDelayTime = -100.0f;
        triggerSkill_3 = true;
        skill_In_Move = true;

        ani.AnimationState.SetAnimation(0, "skill_loop", true);

        for (int i = 0; i < 10; i++)
        {
            status.PlayOneSound(skillSound);

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistanceRight(gameObject, unit, skillRange))
                {
                    unit.GetComponent<Status>().Attack(gameObject, skillDamage, Defined.CanProbability(status.stat.CriticalActuationProbability), true);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);

            yield return new WaitForSeconds(0.25f);
        }

        status.unitState.IsSuperArmour = false;
        status.stat.AttackDelayTime = -status.stat.AttackSpeed;
        status.unitState.IsActionDelay = false;
        triggerSkill_3 = false;
        skill_In_Move = false;

        ani.AnimationState.SetAnimation(0, "idle", true);
    }
}

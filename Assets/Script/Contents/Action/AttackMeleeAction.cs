using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using Spine.Unity;

public class AttackMeleeAction : BtAction
{
    private GameObject thisObject;
    private Status status;
    private SkeletonAnimation animation;

    //private bool nextAttackStart;
    private bool isCriticalAttack;
    //
    public AttackMeleeAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        isCriticalAttack = false;

        animation.state.Start += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "attack")
            {
                status.unitState.IsActionDelay = true;
                if (status.stat.AttackSpeed < 1.0f)
                {
                    animation.timeScale = 1.0f / status.stat.AttackSpeed;
                }
            }
            else
            {
                animation.timeScale = 1f;
            }
        };

        animation.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e) 
        {
            if (e.Data.Name == "attack")
            {
                if (status.AttackTarget != null)
                {
                    if (isCriticalAttack == true)
                    {
                        status.AttackTarget.GetComponent<Status>().Attack(thisObject, status.stat.AttackDamage * status.stat.CriticalMultiples, true);
                    }
                    else
                    {
                        status.AttackTarget.GetComponent<Status>().Attack(thisObject, status.stat.AttackDamage, false);
                    }

                    if (status.isStunAttack == true)
                    {
                        status.AttackTarget.GetComponent<Status>().TakeStun(status.nextStunDuring);
                        status.isStunAttack = false;
                        status.nextStunDuring = 0.0f;
                    }

                    if (status.AttackTarget.GetComponent<Status>().hitPriority == eHitPriority.TOWER)
                    {
                        status.unitState.IsAttackingTower = true;
                    }

                    status.GetAttackSound();
                    if (status.AttackSoundClip != null)
                    {
                        status.PlayOneSound(status.AttackSoundClip);
                    }
                    else
                    {
                        status.PlayOneSound(status.GetLunaAssetBundle().sound_hit);
                    }
                }

                isCriticalAttack = false;
            }
        };

    }

    public override eStatus Update()
    {
        if (isCriticalAttack == false)
        {
            isCriticalAttack = Defined.CanProbability(status.stat.CriticalActuationProbability);
        }

        ConfirmWarf();

        if (status.AttackTarget != null)
        {
            if (isCriticalAttack == true && animation.state.Data.SkeletonData.FindAnimation("attack02") != null && animation.AnimationName != "attack02")
            {
                animation.AnimationState.SetAnimation(0, "attack02", false);
            }
            else if (animation.AnimationName != "attack")
            {
                animation.AnimationState.SetAnimation(0, "attack", false);
            }
        }
        else
        {
            animation.AnimationState.SetAnimation(0, "idle", true);
        }

        return eStatus.Success;
    }

    void ConfirmWarf()
    {
        if (status.isWarf == true)
        {
            float distance = status.AttackTarget.GetComponent<Transform>().position.x - thisObject.transform.position.x;

            if (distance > status.warfRange)
            {
                GameObject.Instantiate(status.GetLunaAssetBundle().JumpEffect, status.ProjecttileArrivalLocation.transform.position, status.GetLunaAssetBundle().JumpEffect.transform.rotation);

                thisObject.transform.Translate(Vector3.right * (distance - status.warfRange));
            }
        }
    }

}

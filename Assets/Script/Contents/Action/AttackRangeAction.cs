using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class AttackRangeAction : BtAction
{
    private GameObject thisObject;
    private Status status;
    private GameObject projectile;
    private SkeletonAnimation animation;

    private bool nextAttackStart;
    private bool nextAttackCritical;

    public GameObject Projectile
    {
        get { return projectile; }
        set { projectile = value; }
    }

    public AttackRangeAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        nextAttackStart = false;

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
                    foreach (GameObject firePos in status.ProjecttileCreateLocations)
                    {
                        GameObject bullet 
                            = PoolManager.Instance.spawnObject(projectile, firePos.transform.position, Quaternion.Euler(0, 0, 0));
                        //GameObject bullet = GameObject.Instantiate(projectile, firePos.transform.position, Quaternion.Euler(0, 0, 0));
                        bullet.GetComponent<Projectile>().Attacker = thisObject;
                        bullet.GetComponent<Projectile>().Target = status.AttackTarget;
                        bullet.GetComponent<Projectile>().ProjectilePower = status.stat.AttackDamage;

                        if (nextAttackCritical == true)
                        {
                            bullet.GetComponent<Projectile>().ProjectilePower *= status.stat.CriticalMultiples;
                            bullet.GetComponent<Projectile>().criticalShot = true;
                        }

                        if(status.isStunAttack == true)
                        {
                            bullet.GetComponent<Projectile>().isStun = true;
                            bullet.GetComponent<Projectile>().StunTime = status.nextStunDuring;
                            status.isStunAttack = false;
                            status.nextStunDuring = 0.0f;
                        }
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

                nextAttackStart = false;
            }
        };

    }

    public override eStatus Update()
    {
        if (nextAttackStart == false)
        {
            nextAttackStart = true;
            nextAttackCritical = Defined.CanProbability(status.stat.CriticalActuationProbability);
        }

        if (status.AttackTarget != null)
        {
            if (animation.AnimationName != "attack")
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

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class idleCondition : BtAction
{
    private GameObject thisObject;
    private Status status;
    private SkeletonAnimation animation;

    public idleCondition(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        animation.state.End += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "attack" || trackEntry.Animation.Name == "attack2")
            {
                animation.ClearState();
                animation.skeletonDataAsset.Clear();
                animation.state.SetAnimation(0, "idle", true);
            }
        };

        animation.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name != "idle" 
            && trackEntry.Animation.Name != "skill_loop" 
            && trackEntry.Animation.Name != "defense_ready" 
            && trackEntry.Animation.Name != "defense")
            {
                animation.state.SetAnimation(0, "idle", true);
            }
        };
    }

    public override eStatus Update()
    {
        if(status.isStun == true)
            return eStatus.Success;

        if (status.unitState.IsLobbyMove == true)
            return eStatus.Success;

        if (TargetFreeCheck() == false)
        {
            status.AttackTarget = null;
            SetTarget();
        }

        if (thisObject.GetComponent<LunaStateMachine>() != null 
            && thisObject.GetComponent<LunaStateMachine>().moving != 0)
            return eStatus.Success;

        if (animation.AnimationName == "skill_loop")
        {
            return eStatus.Success;
        }

        return eStatus.Failure;

    }


    bool TargetFreeCheck()
    {
        if (status.AttackTarget == null || status.AttackTarget.GetComponent<Status>().stat.Hp <= 0.0f)
        {
            return false;
        }
        else
        {
            if (status.unitState.IsAttackingTower == true)
            {
                status.AttackTarget = null;
                status.unitState.IsAttackingTower = false;
                return false;
            }

            if (status.AttackTarget.activeSelf == false)
            {
                status.AttackTarget = null;
                return false;
            }

            if (status.AttackTarget.GetComponent<Status>().stat.Hp > 0.0f)
            {
                float distance = status.AttackTarget.GetComponent<Transform>().position.x - thisObject.GetComponent<Transform>().position.x;


                if (status.UnitSide == eUnitSide.Enemy)
                    distance *= -1;


                if (0 > distance || status.stat.AttackRange < distance)
                {
                    status.AttackTarget = null;
                    return false;
                }

                return true;
            }
            else
            {
                status.AttackTarget = null;
                return false;
            }
        }
    }

    void SetTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

        float curDistance = status.stat.AttackRange;

        GameObject tmpTower = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy.GetComponent<Status>().UnitSide != status.UnitSide && enemy.GetComponent<Status>().stat.Hp > 0.0f)
            {
                float distance = enemy.GetComponent<Transform>().position.x - thisObject.GetComponent<Transform>().position.x;

                if (status.UnitSide == eUnitSide.Enemy)
                    distance *= -1;


                if (0 < distance && curDistance >= distance)
                {
                    curDistance = distance;

                    if (enemy.GetComponent<Status>().hitPriority == eHitPriority.TOWER)
                        tmpTower = enemy;
                    else
                        status.AttackTarget = enemy;
                }
            }
        }

        if( status.AttackTarget == null && tmpTower != null)
        {
            status.AttackTarget = tmpTower;
        }
    }

}

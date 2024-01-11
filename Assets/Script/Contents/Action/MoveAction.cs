using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class MoveAction : BtAction
{
    private GameObject thisObject;
    private Status thisStatus;
    private SkeletonAnimation animation;

    public MoveAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        thisStatus = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    public override eStatus Update()
    {
        if (thisStatus.AttackTarget != null)
        {
            return eStatus.Failure;
        }

        float moveSpeed = thisObject.GetComponent<Status>().stat.MoveSpeed;
        //moveSpeed += LunaDataTable.Instance.playerData.coefficient.MoveSpeed;

        float distanceToPlayer 
            = Vector3.Distance(thisObject.transform.position, Game.Instance.dataManager.LunaPlayer.transform.position);

        float addAuroraRangeValue 
            = Defined.AddAuroraRangeValue * LunaDataTable.Instance.playerData.AuroraLevel;
        float auroraRange = 2.0f + addAuroraRangeValue;

        if (distanceToPlayer < auroraRange
            && (thisObject.GetComponent<Status>().UnitType == eUnitType.Hero
            || thisObject.GetComponent<Status>().UnitType == eUnitType.Soldier))
        {
            moveSpeed += (LunaDataTable.Instance.playerData.AuroraLevel - 1 ) * 0.15f;
        }
        //if()

        thisObject.GetComponent<Transform>().Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (animation.state.Data.SkeletonData.FindAnimation("move") != null)
        {
            if (animation.AnimationName != "move")
            {
                animation.state.SetAnimation(0, "move", true);
            }
        }
        else if (animation.AnimationName != "idle")
        {
            animation.state.SetAnimation(0, "idle", true);
        }

        return eStatus.Success;
    }
}

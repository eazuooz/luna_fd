using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class DeathAction : BtAction
{
    private GameObject thisObject;
    private Status status;
    private SkeletonAnimation animation;

    public DeathAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        animation.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "death")
            {
                status.unitState.IsDeath = true;

                status.DeathRelease();

                if (status.UnitType == eUnitType.Hero)
                {
                    status.unitState.IsPossibleSummonState = false;
                }

                animation.state.SetAnimation(0, "idle", true);

                if (status.objectData.TypeLunaPlayer)
                {
                    PoolManager.Instance.releaseObject(thisObject);
                }
            }
        };

        
    }

    public override eStatus Update()
    {
        if (animation.AnimationName != "death")
        {
            animation.AnimationState.SetAnimation(0, "death", false);
        }

        return eStatus.Success;
    }
}

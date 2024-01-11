using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class DamagedAction : BtAction
{
    private GameObject thisObject;
    private Status thisStatus;
    private SkeletonAnimation animation;
    public DamagedAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        thisStatus = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        animation.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "damage")
            {
                thisStatus.unitState.IsDamaged = false;
            }
        };
    }

    public override eStatus Update()
    {
        if (animation.AnimationName != "damage")
        {
            animation.AnimationState.SetAnimation(0, "damage", false);
        }

        return eStatus.Success;
    }
}

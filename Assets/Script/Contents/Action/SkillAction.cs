using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class SkillAction : BtAction
{
    private GameObject thisObject;
    private Status status;
    private SkeletonAnimation animation;

    public SkillAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        animation.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "skill")
            {
                status.stat.Mp = 0.0f;
                status.unitState.IsSkillActivaion = true;
                status.unitState.IsActionDelay = true;
            }
        };
    }
    public override eStatus Update()
    {
        if (animation.AnimationName != "skill")
        {
            animation.AnimationState.SetAnimation(0, "skill", false);
        }

        return eStatus.Running;
    }
}

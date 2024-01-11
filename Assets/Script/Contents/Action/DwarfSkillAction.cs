using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class DwarfSkillAction : BtAction
{
    private GameObject thisObject;
    private Status thisStatus;
    private SkeletonAnimation animation;

    public DwarfSkillAction(GameObject _gameObject)
    {
        thisObject = _gameObject;
        thisStatus = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        animation.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "defense_ready")
            {
                thisStatus.unitState.IsSkillActivaion = true;
            }
        };
    }

    public override eStatus Update()
    {
        if (thisStatus.unitState.IsSkillActivaion == false && animation.AnimationName != "defense_ready" && animation.AnimationName != "defense")
        {
            animation.AnimationState.SetAnimation(0, "defense_ready", false);
        }

        return eStatus.Running;
    }
}

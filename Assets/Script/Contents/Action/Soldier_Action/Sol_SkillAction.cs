using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Sol_SkillAction : BtAction
{
    GameObject Obj;

    public Sol_SkillAction(GameObject gmObj)
    {
        Obj = gmObj;
    }

    public override void Initialize()
    {

    }

    public override void Terminate()
    {

    }

    public override eStatus Update()
    {
        if (Obj.GetComponent<SkeletonAnimation>().AnimationName != "skill")
        {
            Obj.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "skill", false);
            Obj.GetComponent<SkeletonAnimation>().AnimationState.AddAnimation(0, "idle", false, 1);
            Obj.GetComponent<Status>().stat.Stamina = 0.0f;
        }

        return eStatus.Running;
    }
}

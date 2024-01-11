using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Sol_DamagedAction : BtAction
{
    GameObject Obj;
    public Sol_DamagedAction(GameObject gmObj)
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
        if (Obj.GetComponent<SkeletonAnimation>().AnimationName != "damage")
        {
            Obj.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "damage", false);
            Obj.GetComponent<SkeletonAnimation>().AnimationState.AddAnimation(0, "idle", false, 0.5f);
            Obj.GetComponent<Status>().unitState.IsDamaged = false;
        }

        return eStatus.Success;
    }
}

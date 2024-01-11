using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Tow_DamagedAction : BtAction
{
    GameObject Obj;
    public Tow_DamagedAction(GameObject gmObj)
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
            Obj.GetComponent<SkeletonAnimation>().AnimationState.AddAnimation(0, "idle", true, 0.0f);
            Obj.GetComponent<Status>().unitState.IsDamaged = false;
        }

        return eStatus.Success;
    }
}

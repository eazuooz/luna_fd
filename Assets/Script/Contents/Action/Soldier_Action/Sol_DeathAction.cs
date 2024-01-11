using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Sol_DeathAction : BtAction
{
    GameObject Obj;
    bool die = false;

    public Sol_DeathAction(GameObject gmObj)
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
        if (die == false)
        {
            Obj.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "death", false);
            Obj.GetComponent<SkeletonAnimation>().AnimationState.AddAnimation(0, "idle", false, 1);
            die = true;
        }

        if(die == true && Obj.GetComponent<SkeletonAnimation>().AnimationName != "death")
        {
            Obj.GetComponent<Status>().unitState.IsDeath = true;
        }

        return eStatus.Success;
    }
}

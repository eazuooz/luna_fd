using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Sol_MoveAction : BtAction
{
    GameObject Obj;
    float speed;
    public Sol_MoveAction(GameObject gmObj, float spd)
    {
        Obj = gmObj;
        speed = spd;
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
            if (Obj.GetComponent<SkeletonAnimation>().AnimationName != "idle")
            {
                Obj.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "idle", true);
            }

            Obj.GetComponent<Transform>().Translate(Vector3.right * speed * Time.deltaTime);
        }

        return eStatus.Success;
    }
}

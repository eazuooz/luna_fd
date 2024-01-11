using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Sol_Idle : BtAction
{

    GameObject Obj;

    public Sol_Idle(GameObject gmObj)
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
        return eStatus.Failure;
    }
}

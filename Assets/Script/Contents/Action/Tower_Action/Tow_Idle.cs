using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Tow_Idle : BtAction
{
    GameObject Obj;

    public Tow_Idle(GameObject gmObj)
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


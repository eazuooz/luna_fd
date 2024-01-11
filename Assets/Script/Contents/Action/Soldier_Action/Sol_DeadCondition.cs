using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Sol_DeadCondition : BtCondition
{
    GameObject Obj;

    public Sol_DeadCondition(GameObject gmObj)
    {
        Obj = gmObj;
    }

    public override eStatus Update()
    {
        if (Obj.GetComponent<Status>().stat.Hp <= 0.0f)
            return eStatus.Success;
        else
            return eStatus.Failure;
    }
}


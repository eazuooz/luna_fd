using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Tow_DeadCondition : BtCondition
{
    GameObject Obj;

    public Tow_DeadCondition(GameObject gmObj)
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

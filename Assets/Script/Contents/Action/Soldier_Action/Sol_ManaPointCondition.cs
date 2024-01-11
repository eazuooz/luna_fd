using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Sol_ManaPointCondition : BtCondition
{
    GameObject Obj;
    float range;

    public Sol_ManaPointCondition(GameObject gmObj, float rng)
    {
        Obj = gmObj;
    }

    public override eStatus Update()
    {
        if(Obj.GetComponent<Status>().stat.Stamina > 100.0f)
            return eStatus.Success;
        else
            return eStatus.Failure;
    }
}

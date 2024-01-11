using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Tow_DamagedCondition : BtCondition
{
    GameObject Obj;

    public Tow_DamagedCondition(GameObject gmObj)
    {
        Obj = gmObj;
    }

    public override eStatus Update()
    {
        if (Obj.GetComponent<Status>().unitState.IsDamaged == true)
            return eStatus.Success;
        else
            return eStatus.Failure;
    }
}


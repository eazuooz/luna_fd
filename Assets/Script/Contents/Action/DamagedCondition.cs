using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class DamagedCondition : BtCondition
{
    private GameObject thisObject;
    private Status thisStatus;

    public DamagedCondition(GameObject _gameObject)
    {
        thisObject = _gameObject;
        thisStatus = thisObject.GetComponent<Status>();
    }

    public override eStatus Update()
    {
        if (thisStatus.unitState.IsSuperArmour == true)
            return eStatus.Failure;
        else if (thisStatus.unitState.IsDamaged == true)
            return eStatus.Success;
        else
            return eStatus.Failure;
    }
}

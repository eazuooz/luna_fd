using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;


public class AttackCondition : BtCondition
{
    private GameObject thisObject;
    private Status thisStatus;

    public AttackCondition(GameObject _gameObject)
    {
        thisObject = _gameObject;
        thisStatus = thisObject.GetComponent<Status>();
    }

    public override eStatus Update()
    {
        if (thisStatus.unitState.IsActionDelay == true)
        {
            return eStatus.Failure;
        }

        if (thisStatus.AttackTarget != null && thisStatus.AttackTarget.GetComponent<Status>().stat.Hp > 0.0f)
        {
            return eStatus.Success;
        }

        return eStatus.Failure;
    }
}

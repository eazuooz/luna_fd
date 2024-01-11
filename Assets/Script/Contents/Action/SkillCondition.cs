using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class SkillCondition : BtCondition
{
    private GameObject thisObject;
    private Status status;

    public SkillCondition(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
    }

    public override eStatus Update()
    {
        if (status.unitState.IsActionDelay == true)
        {
            return eStatus.Failure;
        }

        if (status.unitState.IsTargetingSkill == true && status.AttackTarget == null)
            return eStatus.Failure;
        else if (status.stat.Mp > 100.0f)
            return eStatus.Success;
        else
            return eStatus.Failure;
    }
}

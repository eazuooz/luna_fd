using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class DeathCondition : BtCondition
{
    private GameObject thisObject;
    private Status status;
    private SkeletonAnimation animation;

    public DeathCondition(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    public override eStatus Update()
    {
        if (status.stat.Hp <= 0.0f)
            return eStatus.Success;
        else
            return eStatus.Failure;
    }
}


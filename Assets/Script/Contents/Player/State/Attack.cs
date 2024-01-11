using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Attack : State
{
    private static Attack instance = new Attack();
    public static Attack Instance
    {
        get { return instance; }
    }

    public Attack()
    {
        mLevel = 0;
    }

    public override void OnEnter(GameObject gameObject)
    {
        //
        gameObject.GetComponent<LunaStateMachine>().curStateString = "attack";
    }

    public override void OnExcute(GameObject gameObject)
    {
        //
    }

    public override void OnExit(GameObject gameObject)
    {
        //
    }
}

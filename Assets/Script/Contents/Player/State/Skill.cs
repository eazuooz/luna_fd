using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Skill : State
{
    private static Skill instance = new Skill();
    public static Skill Instance
    {
        get { return instance; }
    }

    public Skill()
    {
        mLevel = 0;
    }

    public override void OnEnter(GameObject gameObject)
    {
        //
        gameObject.GetComponent<LunaStateMachine>().curStateString = "skill";
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

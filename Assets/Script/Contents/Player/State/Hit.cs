using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Hit : State
{
    private static Hit instance = new Hit();
    public static Hit Instance
    {
        get { return instance; }
    }

    public Hit()
    {
        mLevel = 0;
    }

    public override void OnEnter(GameObject gameObject)
    {
        gameObject.GetComponent<LunaStateMachine>().curStateString = "hit";
    }

    public override void OnExcute(GameObject gameObject)
    {
        ////// state ¿Ãµø
        if (gameObject.GetComponent<Status>().stat.Hp <= 0.0f)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Death.Instance);
        }
        else if (gameObject.GetComponent<Status>().unitState.IsDamaged == false)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Idle.Instance);
        }
    }

    public override void OnExit(GameObject gameObject)
    {
        //
    }
}

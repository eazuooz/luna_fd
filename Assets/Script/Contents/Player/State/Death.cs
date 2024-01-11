using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Death : State
{
    private static Death instance = new Death();
    public static Death Instance
    {
        get { return instance; }
    }

    public Death()
    {
        mLevel = 0;
    }

    public override void OnEnter(GameObject gameObject)
    {
        gameObject.GetComponent<LunaStateMachine>().curStateString = "death";
    }

    public override void OnExcute(GameObject gameObject)
    {
        ////// state 행동

        if (gameObject.GetComponent<Status>().stat.Hp > 0.0f)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Idle.Instance);
        }

        ////// state 이동
        //if (gameObject.GetComponent<Status>().stat.Hp <= 0.0f)
        //{
        //    gameObject.GetComponent<LunaStateMachine>().ChangeState(Death.Instance);
        //}
        //else if (gameObject.GetComponent<Status>().damaged == true)
        //{
        //    gameObject.GetComponent<LunaStateMachine>().ChangeState(Hit.Instance);
        //}
        //else if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        //{
        //    gameObject.GetComponent<LunaStateMachine>().ChangeState(Move.Instance);
        //}
        //else
        //{
        //    gameObject.GetComponent<LunaStateMachine>().ChangeState(Idle.Instance);
        //}
    }

    public override void OnExit(GameObject gameObject)
    {
        //
    }
}

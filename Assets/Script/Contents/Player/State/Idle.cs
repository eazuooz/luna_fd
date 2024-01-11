using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Idle : State
{
    private static Idle instance = new Idle();
    private float AutoAttackMoveDistance;
    private bool AutoAttackMoveTrigger;

    public static Idle Instance
    {
        get { return instance; }
    }

    public Idle()
    {
        mLevel = 0;
        AutoAttackMoveDistance = 1.5f;
        AutoAttackMoveTrigger = false;
    }

    public override void OnEnter(GameObject gameObject)
    {
        gameObject.GetComponent<Transform>().Find("Sprite").gameObject.GetComponent<SkeletonAnimation>().skeleton.ScaleX = 1.0f;

        gameObject.GetComponent<LunaStateMachine>().curStateString = "idle";
    }

    public override void OnExcute(GameObject gameObject)
    {

        if (gameObject.GetComponent<Status>().unitState.IsLobbyStartMove == true)
        {
            gameObject.GetComponent<Transform>().Translate(Vector3.right * 5.0f * Time.deltaTime);
            return;
        }
        else if (gameObject.GetComponent<Status>().unitState.IsLobbyBackMove == true)
        {
            gameObject.GetComponent<Transform>().Find("Sprite").gameObject.GetComponent<SkeletonAnimation>().skeleton.ScaleX = -1.0f;
            gameObject.GetComponent<Transform>().Translate(Vector3.left * 5.0f * Time.deltaTime);
            return;
        }
        else if(gameObject.GetComponent<Status>().unitState.IsLobbyMove == true)
        {
            return;
        }
        else if (gameObject.GetComponent<LunaStateMachine>().moving != 0)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Move.Instance);
        }

    }

    public override void OnExit(GameObject gameObject)
    {

    }

}

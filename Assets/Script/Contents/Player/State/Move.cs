using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Move : State
{
    private static Move instance = new Move();
    private SkeletonAnimation animation;

    public static Move Instance
    {
        get { return instance; }
    }

    public Move()
    {
        mLevel = 0;
    }


    public override void OnEnter(GameObject gameObject)
    {
        animation = gameObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        gameObject.GetComponent<LunaStateMachine>().curStateString = "move";
    }

    public override void OnExcute(GameObject gameObject)
    {
        if (gameObject.GetComponent<Status>().stat.Hp <= 0.0f)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Death.Instance);
        }
        else if (gameObject.GetComponent<LunaStateMachine>().moving == 0)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Idle.Instance);
        }

        if (gameObject.GetComponent<Status>().unitState.IsLobbyMove == true)
        {
            gameObject.GetComponent<LunaStateMachine>().ChangeState(Idle.Instance);
        }

        if (animation.AnimationName != "idle" && animation.AnimationName != "damage")
        {
            animation.ClearState();
            animation.skeletonDataAsset.Clear();
            animation.AnimationState.SetAnimation(0, "idle", true);
        }


        if (Game.Instance.lunaSceneManager.CurrentStage != null && Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>() != null)
        {
            Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>().FollowCamera = true;
        }


        if (gameObject.GetComponent<LunaStateMachine>().moving == 1)
        {
            gameObject.GetComponent<Transform>().Find("Sprite").gameObject.GetComponent<SkeletonAnimation>().skeleton.ScaleX = 1.0f;

            if (gameObject.GetComponent<Status>().AttackTarget == null ||
                gameObject.GetComponent<Status>().AttackTarget.transform.position.x > gameObject.transform.position.x + 1.0f)
            {
                gameObject.transform.Translate(Vector2.right * gameObject.GetComponent<Status>().stat.MoveSpeed * Time.deltaTime);
            }

            //gameObject.GetComponent<Status>().leftHand = false;
        }
        else if (gameObject.GetComponent<LunaStateMachine>().moving == -1)
        {
            gameObject.GetComponent<Transform>().Find("Sprite").gameObject.GetComponent<SkeletonAnimation>().skeleton.ScaleX = -1.0f;

            if (gameObject.transform.position.x > Defined.AllyTowerPosition.x)
                gameObject.transform.Translate(Vector2.left * gameObject.GetComponent<Status>().stat.MoveSpeed * Time.deltaTime);

            //gameObject.GetComponent<Status>().leftHand = true;
        }
    }
    public override void OnExit(GameObject gameObject)
    {
    }
}




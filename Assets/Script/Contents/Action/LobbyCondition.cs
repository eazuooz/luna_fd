using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class LobbyCondition : BtAction
{
    private GameObject thisObject;
    private Status status;
    private SkeletonAnimation animation;

    public LobbyCondition(GameObject _gameObject)
    {
        thisObject = _gameObject;
        status = thisObject.GetComponent<Status>();
        animation = thisObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    public override eStatus Update()
    {
        if (status.unitState.IsLobbyMove == true)
        {
            if (status.unitState.IsLobbyStartMove == true)
            {
                thisObject.GetComponent<Transform>().Translate(Vector3.right * 5.0f * Time.deltaTime);
                return eStatus.Success;
            }
            else if (status.unitState.IsLobbyBackMove == true)
            {
                animation.skeleton.ScaleX = -1.0f;
                thisObject.GetComponent<Transform>().Translate(Vector3.left * 5.0f * Time.deltaTime);
                return eStatus.Success;
            }
            else
            {

                return eStatus.Success;
            }
        }
        else
        {
            return eStatus.Failure;
        }

    }
}

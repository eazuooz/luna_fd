using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UISelectPopup : UILuna
{
    public UIReference refData;

    private GameObject mainTextObj;
    private string mainText;
    public override void OnInit() 
    {
        base.OnInit();
    }

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void PushPopupUI()
    {
        Game.Instance.uiManager.Push(type);
    }

    public void ReleaseThisUI()
    {
        Game.Instance.uiManager.Pop(type);
    }

    public void OnClickEscape()
    {
        Game.Instance.uiManager.Pop(type);
        Game.Instance.uiManager.Pop(UI_TYPE.SKILL);
        Game.Instance.uiManager.Pop(UI_TYPE.PLAYER);
        Game.Instance.uiManager.Pop(UI_TYPE.PROGRESSBAR);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
            unit.GetComponent<Status>().unitState.IsLobbyBackMove = true;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Lobby");
        });
    }
}

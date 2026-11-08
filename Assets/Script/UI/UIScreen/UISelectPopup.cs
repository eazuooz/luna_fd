using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UISelectPopup : UILuna
{

    private GameObject mainTextObj;
    private string mainText;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        
    }

    

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed); }

    

    protected override void OnClose(bool isShutdown, object userData)
    {
        try
        { 
        }
        finally { base.OnClose(isShutdown, userData); }
    }

    public void PushPopupUI()
    {
        LunaUI.Open(type);
    }

    public void ReleaseThisUI()
    {
        LunaUI.Close(type);
    }

    public void OnClickEscape()
    {
        LunaUI.Close(type);
        LunaUI.Close(UI_TYPE.SKILL);
        LunaUI.Close(UI_TYPE.PLAYER);
        LunaUI.Close(UI_TYPE.PROGRESSBAR);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<UnitLogic>().unitState.IsLobbyMove = true;
            unit.GetComponent<UnitLogic>().unitState.IsLobbyBackMove = true;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        LunaScenes.FadeIn(1.5f, () =>
        {
            LunaScenes.Load("Lobby");
        });
    }
}

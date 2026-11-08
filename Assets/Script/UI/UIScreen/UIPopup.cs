using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIPopup : UILuna
{

    private GameObject mainTextObj;
    public string mainText;
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

    public void SetMainText(string str)
    {
        mainTextObj = transform.Find("Image").Find("MainText").gameObject;
        mainTextObj.GetComponent<Text>().text = str;
    }
}

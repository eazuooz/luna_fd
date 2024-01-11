using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIPopup : UILuna
{
    public UIReference refData;

    private GameObject mainTextObj;
    public string mainText;
    public override void OnInit() 
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);
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

    public void SetMainText(string str)
    {
        mainTextObj = transform.Find("Image").Find("MainText").gameObject;
        mainTextObj.GetComponent<Text>().text = str;
    }
}

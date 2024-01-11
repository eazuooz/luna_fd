using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;

public class LButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject stage;

    void Awake()
    {
        stage = GameObject.FindGameObjectWithTag("Stage");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Game.Instance.dataManager.LunaPlayer != null)
        {
            Game.Instance.dataManager.LunaPlayer.GetComponent<LunaStateMachine>().LMove();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Game.Instance.dataManager.LunaPlayer != null)
        {
            Game.Instance.dataManager.LunaPlayer.GetComponent<LunaStateMachine>().NMove();
        }
    }
}
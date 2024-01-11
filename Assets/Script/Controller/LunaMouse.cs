using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EverydayDevup.Framework;

public class LunaMouse : LunaController
{
    
	public override void OnInit()
	{

	}
	public override void OnActive() 
	{ 
	
	}

	public override void OnInActive() 
	{ 
	
	}

	public override void OnUpdate() 
	{
        if( SceneManager.GetActiveScene().name == "Lobby")
        {
            LobbyMouseCheck(LobbyMousePicking);
        }
        else
        {

        }
    }

	public override void OnLoop() 
	{ 
		
	}

	public override void OnClear() 
	{ 
	
	}

    private void LobbyMouseCheck(System.Action callBack)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Game.Instance.uiManager.CompareTop(UI_TYPE.CHARACTERLIST)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.CHARACTERINFO)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.UPGRADE)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.SELECTSTAGE)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.INVENTORY)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.GAMESHOP)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.STAGEINFO)
                || Game.Instance.uiManager.CompareTop(UI_TYPE.DUNGEONINFO))
            {
                return;
            }

            if (null == GameObject.FindWithTag("UICharacterList"))
            {
                callBack();
            }
        }
    }

    private void LobbyMousePicking()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

        if (hit.collider != null)
        {
            
            if (hit.collider.GetComponent<Status>().objectData.TypeLunaPlayer == true)
            {
                Game.Instance.uiManager.Push(UI_TYPE.UPGRADE);
            }
            else 
            {
                Game.Instance.dataManager.PickingGameObject = hit.transform.gameObject;
                Game.Instance.uiManager.Push(UI_TYPE.CHARACTERLIST);
                //Game.Instance.uiManager.Push(UI_TYPE.CHARACTERINFO);
            }
        }
    }

    public Vector3 GetClickedWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public Vector3 GetClickedScreenPosition()
    {
        return Input.mousePosition;
    }
}

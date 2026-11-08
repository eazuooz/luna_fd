using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIStageInfo : UILuna
{

    private GameObject monsterContents;
    private GameObject itemContents;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        
        var stage = LunaSession.Current.SelectStagePrefab.GetComponent<LunaStageDefinition>();
        transform.Find("BackGround/Title/Text").GetComponent<Text>().text = LunaSession.Current.CurrentStageLevelString("악마의 탑 지하 ");
        transform.Find("BackGround/SelectButton/LevelText").GetComponent<Text>().text = LunaSession.Current.CurrentStageLevelString("입장 ");
        transform.Find("BackGround/Title/Skull").gameObject.SetActive(stage.isBossStage);
        LunaStagePreview.Fill(transform, stage);
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

    public void OnClickStartGame()
    {
        LunaUI.Close(UI_TYPE.STAGEINFO);
        LunaUI.Close(UI_TYPE.SELECTSTAGE);

        LunaScenes.CurrentLobbyGame?.OpenDoor();
        foreach (GameObject unit in LunaSession.Current.lobbyUnits)
        {
            if (unit != null)
                unit.GetComponent<UnitLogic>().unitState.IsLobbyStartMove = true;
        }


        //게임 시작 최초 선택된 스테이지가 없을 경우
        if (LunaSession.Current.CurrentStageName == "")
        {
            //LunaSession.Current.stageNum = 0;
            LunaSession.Current.SelectStagePrefab =
                Resources.Load<GameObject>("Prefabs/10Stage/Stage01-01");
        }
        else
        {
            string stageDir = "Prefabs/10Stage/"
                    + LunaSession.Current.CurrentStageName;

            LunaSession.Current.SelectStagePrefab
                = Resources.Load<GameObject>(stageDir);
        }

        LunaScenes.FadeIn(1.5f, () =>
        {
            LunaScenes.Load("Game");
        });
    }
    
    public void PushPopupUI()
    {
        LunaUI.Open(type);
    }

    public void ReleaseThisUI()
    {
        LunaUI.Close(type);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIDungeonInfo : UILuna
{

    private GameObject monsterContents;
    private GameObject itemContents;
    private int dungeonNumber;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        var prefab = LunaSession.Current.selectDungeon;
        if (prefab == null) { LunaUI.Close(type); return; }
        if (!int.TryParse(prefab.name.Replace("Dungeon", ""), out dungeonNumber)) throw new System.InvalidOperationException("Invalid dungeon selection.");
        var stage = prefab.GetComponent<LunaStageDefinition>();
        transform.Find("BackGround/Title/Text").GetComponent<Text>().text = stage.DungeonName;
        transform.Find("BackGround/SelectButton/LevelText").GetComponent<Text>().text = "던전 입장하기";
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

    public void ReleaseThisUI()
    {
        LunaUI.Close(type);
    }

    public int DungeonNumber { get; set; }

    public void OnClickSelectDungeon(int idx)
    {
        LunaUI.Close(type);
        LunaUI.Close(UI_TYPE.SELECTSTAGE);

        string dungeonString = "Dungeon0";
        dungeonString += dungeonNumber.ToString();

        LunaSession.Current.SelectDungeonString = dungeonString;

        string stageDir = "Prefabs/11Dungeons/"
                + LunaSession.Current.SelectDungeonString;

        LunaSession.Current.selectDungeon
            = Resources.Load<GameObject>(stageDir);

        MoveDungeonScene();

    }
    public void MoveDungeonScene()
    {
        //LunaUI.Close(type);
        foreach (GameObject unit in LunaSession.Current.lobbyUnits)
        {
            if (unit != null)
                unit.GetComponent<UnitLogic>().unitState.IsLobbyStartMove = true;
        }

        LunaScenes.FadeIn(1.5f, () =>
        {
            LunaScenes.Load("Dungeon");
        });
    }

}

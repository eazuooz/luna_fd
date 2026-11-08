using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class UISelectDungeon : UILuna
{

    private GameObject[] stagePrefabs;
    private Dictionary<string, Button> stageButtons
        = new Dictionary<string, Button>();

    private GameObject stageBttPrefab;

    private GameObject stageTab;
    private GameObject stageScrollView;
    private Transform stageContentTr;

    private GameObject dungeonTab;
    private GameObject dungeonScrollView;


    private GameObject dungeonBttPrefab;
    private List<GameObject> dungeonBtts
        = new List<GameObject>();


    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        stageButtons.Clear();
        dungeonBtts.Clear();
        

        

        InitializeGameObjects();
        InitStageList();
        InitStageButtons();
        SetStageButtonPositionAsSaveData();


        
    }

    

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        


    }

    

    protected override void OnClose(bool isShutdown, object userData)
    {
        try
        {
        StageButton.OnClear();
    
        }
        finally { base.OnClose(isShutdown, userData); }
    }

    private void InitializeGameObjects()
    {
        stageTab = GameObject.Find("Stage_Tab");
        dungeonTab = GameObject.Find("Dungeon_Tab");

        stageTab.transform.GetComponent<Button>().onClick.AddListener(() => tabClick(stageTab));
        dungeonTab.transform.GetComponent<Button>().onClick.AddListener(() => tabClick(dungeonTab));

        stageBttPrefab
            = Resources.Load<GameObject>("Prefabs/09UI/StageButton");

        stageScrollView = stageTab.transform.Find("Stage_ScrollView").gameObject;
          //= LunaUIViews.FindChildObject(gameObject, "Stage_ScrollView");
        stageContentTr = stageScrollView.transform.Find("Stage_Viewport").Find("Stage_Content");

        dungeonScrollView = dungeonTab.transform.Find("Dungeon_ScrollView").gameObject;
            //= LunaUIViews.FindChildObject(gameObject, "Dungeon_ScrollView");

        dungeonBttPrefab = Resources.Load<GameObject>("Prefabs/09UI/DungeonInfo");

        TurnOffAllOfScrollView();
        stageScrollView.gameObject.SetActive(true);
    }
    private void SetStage(string stageName)
    {
        LunaSession.Current.SelectStagePrefab
            = LunaSession.Current.AllStagePrefab.Find(obj => obj.name == stageName);

        LunaSession.Current.CurrentStageName = stageName;
        LunaDataTable.Instance.playerData.SaveStageName = stageName;
    }

    private void TurnOffAllOfScrollView()
    {
        if (stageScrollView.activeSelf == true)
        {
            stageScrollView.SetActive(false);
        }

        if (dungeonScrollView.activeSelf == true)
        {
            dungeonScrollView.SetActive(false);
        }
    }
    private void SwitchTab(string tabName)
    {
        TurnOffAllOfScrollView();
        switch (tabName)
        {
            case "Stage_Tab":
                {
                    TurnOnScrollview(stageTab);
                    
                }
                break;
            case "Dungeon_Tab":
                {
                    TurnOnScrollview(dungeonTab);
                }
                break;
            default:
                break;
        }
    }
    private void tabClick(GameObject tabObject)
    {
        if(LunaUI.IsTop(UI_TYPE.STAGEINFO))
        {
            LunaUI.Close(UI_TYPE.STAGEINFO);
        }

        SwitchTab(tabObject.name);
    }

    private void TurnOnScrollview(GameObject tabObject)
    {
        GameObject scrollViewObj = getScrollViewGameObject(tabObject);
        scrollViewObj.SetActive(true);
    }
    private GameObject getScrollViewGameObject(GameObject tabObject)
    {
        Transform scrollViewObj = null;
        string scrollViewStr = tabObject.name;
        scrollViewStr = scrollViewStr.Replace("Tab", "ScrollView");
        scrollViewObj = tabObject.transform.Find(scrollViewStr);
        if (scrollViewObj)
        {
            return scrollViewObj.gameObject;
        }
        else
        {
            return null;
        }
    }
    private void InitStageList()
    {
        stagePrefabs
            = Resources.LoadAll<GameObject>("Prefabs/10Stage");

        List<GameObject> stageList = new List<GameObject>();
        foreach (var item in stagePrefabs)
        {
            stageList.Add(item);
        }

        LunaSession.Current.AllStagePrefab.Clear();
        LunaSession.Current.AllStagePrefab.AddRange(stageList);
    }

    private void InitStageButtons()
    {
        foreach (var stage in stagePrefabs)
        {
            GameObject btt = CreateStageButton(stage.name);
            stageButtons.Add(stage.name, btt.GetComponent<Button>());
        }
    }
    private void SetStageButtonPositionAsSaveData()
    {
        string saveStage = LunaDataTable.Instance.playerData.SaveStageName;
        
        if (saveStage == "")
        {
            return;
        }
        SetStage(saveStage);

        Button btt;
        stageButtons.TryGetValue(saveStage, out btt);
        //89 + 5 = 94
        saveStage = saveStage.Replace("Stage", "");
        string[] strStage
                = saveStage.Split(new char[] { '-' });


        int front = Int32.Parse(strStage[0]) - 1;
        int back = Int32.Parse(strStage[1]) - 1;

        float moveSize = (front * 920) + (back * 92);

        StartCoroutine(MoveContentTr(moveSize));

        
        LunaUI.Open(UI_TYPE.STAGEINFO);
    }
    IEnumerator MoveContentTr(float movePos)
    {
        yield return null;
        stageContentTr.GetComponent<RectTransform>().anchoredPosition = new Vector3(-189, movePos, 0);


    }
    private GameObject CreateStageButton(string StageName)
    {
        GameObject newButton = Instantiate(stageBttPrefab);
        newButton.transform.SetParent(stageContentTr);
        newButton.transform.localScale = Vector3.one;

        //"stage01-01"
        string levelString
            = StageName.Replace("Stage", "");
        string[] strStage
            = levelString.Split(new char[] { '-' });

        int stageFront = int.Parse(strStage[0]);
        int stageBack = int.Parse(strStage[1]);
        int stageLevel = (((stageFront - 1) * 10) + stageBack);

        newButton.transform.Find("Text").GetComponent<Text>().text 
            = "지하 " + stageLevel.ToString() + "층";
        newButton.GetComponent<StageButton>().StageName = StageName;

        return newButton;
    }
    public void OnClickReleaseButton()
    {
        if (LunaUI.IsTop(UI_TYPE.STAGEINFO))
        {
            LunaUI.Close(UI_TYPE.STAGEINFO);
        }

        LunaUI.Close(type);
        
    }

    public int DungeonNumber { get; set; }
    public void OnClickSelectDungeon(int idx)
    {
        //LunaUI.Close(UI_TYPE.SELECTSTAGE);



        string dungeonString = "Dungeon0";
        dungeonString += idx.ToString();

        LunaSession.Current.SelectDungeonString = dungeonString;

        string stageDir = "Prefabs/11Dungeons/"
                + LunaSession.Current.SelectDungeonString;

        LunaSession.Current.selectDungeon
            = Resources.Load<GameObject>(stageDir);

        //MoveDungeonScene();

        LunaUI.Open(UI_TYPE.DUNGEONINFO);

    }
    public void MoveDungeonScene()
    {
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

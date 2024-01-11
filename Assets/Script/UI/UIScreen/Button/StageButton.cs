using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class StageButton : MonoBehaviour
{
    private static List<GameObject> allButtons
        = new List<GameObject>();

    private List<GameObject> dungeonInfos
        = new List<GameObject>();

    private GameObject checkImage;
    private GameObject bossImage;
    private GameObject dungeonInfo;
    private GameObject stagePrefab;

    public static void OnClear()
    {
        foreach (GameObject btt in allButtons)
        {
            Destroy(btt);
        }
        allButtons.Clear();
    }
    public string StageName { get; set; }

    private void TurnCheckImage(bool OnOff)
    {
        checkImage.SetActive(OnOff);
    }

    private void TurnBossImage(bool OnOff)
    {
        bossImage.SetActive(OnOff);
    }

    private void SetStage(string stageName)
    {
        Game.Instance.dataManager.SelectStagePrefab
            = Game.Instance.dataManager.AllStagePrefab.Find(obj => obj.name == stageName);

        Game.Instance.dataManager.CurrentStageName = stageName;
        LunaDataTable.Instance.playerData.SaveStageName = stageName;
    }

    public void InitializeDungeonInfo()
    {
        //dungeonInfo.SetActive(false);
    }
    private void InitializeButtons()
    {
        foreach (var item in allButtons)
        {
            item.GetComponent<StageButton>().TurnCheckImage(false);
            item.GetComponent<StageButton>().InitializeDungeonInfo();
        }
    }

    private void OnClickStageButton(string name)
    {
        if (Game.Instance.uiManager.CompareTop(UI_TYPE.STAGEINFO))
        {
            StartCoroutine(PushStageInfo());
        }

        InitializeButtons();
        TurnCheckImage(true);
        SetStage(name);

        Game.Instance.uiManager.Push(UI_TYPE.STAGEINFO);
    }

    IEnumerator PushStageInfo()
    {
        Game.Instance.uiManager.Pop(UI_TYPE.STAGEINFO);
        bool isPop = true;
        while (isPop)
        {
            //InfiniteLoopDetector.Run();
            if (!Game.Instance.uiManager.CompareTop(UI_TYPE.STAGEINFO))
            {
                isPop = false;
            }

            yield return null;
        }
        
        yield return null;
    }
    // Start is called before the first frame update
    void Start()
    {
        checkImage = gameObject.transform.Find("CheckImage").gameObject;
        bossImage = gameObject.transform.Find("BossImage").gameObject;

        TurnCheckImage(false);
        TurnBossImage(false);

        gameObject.GetComponent<Button>().onClick.AddListener(delegate { OnClickStageButton(StageName); });

        dungeonInfo
            = gameObject.transform.Find("DungeonInfos").gameObject;
        //dungeonInfo.SetActive(false);

        stagePrefab = Resources.Load("Prefabs/10Stage/" + StageName) as GameObject;

        if (stagePrefab.GetComponent<BasicDungeon>().isBossStage)
        {
            TurnBossImage(true);
        }

        dungeonInfos
            = LunaUtility.FindChildObjects
            (this.gameObject.transform.Find("DungeonInfos").gameObject,
            "DungeonInfo01", "DungeonInfo02", "DungeonInfo03", "DungeonInfo04");

        //



        DungeonTable dungeonTable = stagePrefab.GetComponent<BasicDungeon>().dungeonTable;


        for (int i = 0; i < 4; i++)
        {
            
            if (dungeonTable == null)
            {
                dungeonInfos[0].transform.parent.gameObject.SetActive(false);
                dungeonInfos[1].transform.parent.gameObject.SetActive(false);
                dungeonInfos[2].transform.parent.gameObject.SetActive(false);
                dungeonInfos[3].transform.parent.gameObject.SetActive(false);



                break;
            }


            if (dungeonTable.dungeonList[i])
            {
                string stageName = stagePrefab.name;
                string returnStage = stageName;

                returnStage
                    = returnStage.Replace("Stage", "");

                string[] strStage
                    = returnStage.Split(new char[] { '-' });

                int stageFront = int.Parse(strStage[0]);
                int stageBack = int.Parse(strStage[1]) - 1;

                int idx = (((stageFront - 1) * 10) + stageBack);

                StageData stageData;
                LunaDataTable.Instance.StageTable.TryGetValue(stagePrefab.name, out stageData);
                bool isClaer = stageData.isClear;
                if (!isClaer)
                {
                    dungeonInfos[0].transform.parent.gameObject.SetActive(false);
                    dungeonInfos[1].transform.parent.gameObject.SetActive(false);
                    dungeonInfos[2].transform.parent.gameObject.SetActive(false);
                    dungeonInfos[3].transform.parent.gameObject.SetActive(false);
                    break;
                }

                string number = dungeonTable.dungeonList[i].name;
                number = number.Replace("Dungeon", "");

                dungeonInfos[i].GetComponent<DungeonInfo>().DungeonNumber = Int32.Parse(number);
                dungeonInfos[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                dungeonInfos[i].transform.parent.gameObject.SetActive(false);
            }
        }



        allButtons.Add(this.gameObject);

        if (stagePrefab.name == LunaDataTable.Instance.playerData.SaveStageName)
        {
            TurnCheckImage(true);
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}

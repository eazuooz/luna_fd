using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIGameShop : UILuna
{
    public UIReference refData;

    private GameObject summonTab;
    private GameObject goldTab;
    private GameObject starStoneTab;
    private GameObject rubyTab;
    private GameObject rainbowStoneTab;


    class StarStone
    {
        public int count;

        public StarStone(int value)
        {
            count = value;
        }
    }

    private List<StarStone> starStoneChests = new List<StarStone>();
    public override void OnInit()
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);

        InitTabInfo();

        starStoneChests.Add(new StarStone(2000));
        starStoneChests.Add(new StarStone(2000));
        starStoneChests.Add(new StarStone(2000));


        
        //Game.Instance.tutorialManager.SetTutorial(gameObject, "SlimeTenTimesButton", 1);
    }

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate()
    {

    }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void OnClickSummon()
    {
        LunaDataTable.Instance.LotteryResult.Clear();
        if (Random.Range(0, 2) == 0)
        {
            int randIndex = Random.Range(0, 21);

            string name = LunaDataTable.Instance.heroTable[randIndex];

            LunaDataTable.Instance.LotteryResult.Add(name);
            LunaDataTable.Instance.IncreaseCardCount(name);
        }
        else 
        {
            int randIndex = Random.Range(0, 5);

            string name = LunaDataTable.Instance.soliderTable[randIndex];

            LunaDataTable.Instance.LotteryResult.Add(name);
            LunaDataTable.Instance.IncreaseCardCount(name);
        }

        LunaDataTable.Instance.playerData.IsBegining = false;

        Game.Instance.uiManager.Push(UI_TYPE.LOTTERY);
    }
    public void OnClickSummons()
    {
        LunaDataTable.Instance.LotteryResult.Clear();
        string resultText = "";

        int length = 0;
        if (LunaDataTable.Instance.playerData.IsBegining)
        {
            length = 5;
            //LunaDataTable.Instance.playerData.IsBegining = false;

            //Game.Instance.tutorialManager.SetTutorial(gameObject, "ReleaseButton", 2);
        }
        else
        {
            length = 10;
        }

        for (int i = 0; i < length; i++)
        {
            if (LunaDataTable.Instance.playerData.IsBegining)
            {
                int randIndex = Random.Range(0, 21);

                string name = LunaDataTable.Instance.heroTable[randIndex];

                LunaDataTable.Instance.LotteryResult.Add(name);
                LunaDataTable.Instance.IncreaseCardCount(name);
                resultText += name + "´çÃ·\n";
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    int randIndex = Random.Range(0, 21);


                    string name = LunaDataTable.Instance.heroTable[randIndex];

                    LunaDataTable.Instance.LotteryResult.Add(name);
                    LunaDataTable.Instance.IncreaseCardCount(name);

                    resultText += name + "´çÃ·\n";
                }
                else
                {
                    int randIndex = Random.Range(0, 5);

                    string name = LunaDataTable.Instance.soliderTable[randIndex];

                    LunaDataTable.Instance.LotteryResult.Add(name);
                    LunaDataTable.Instance.IncreaseCardCount(name);
                    resultText += name + "´çÃ·\n";
                }
            }
        }

        LunaDataTable.Instance.playerData.IsBegining = false;

        Game.Instance.uiManager.Push(UI_TYPE.LOTTERY);
    }
    public void OnClickBuyStarStone()
    {
        LunaDataTable.Instance.playerData.StarStone += starStoneChests[0].count;

    }

    public void EquipLunaSlimeSkin()
    {
        Game.Instance.dataManager.EquipDesignNumber = 3;
        Game.Instance.uiManager.Push(UI_TYPE.EQUIPDESIGN);
    }
    private void InitTabInfo()
    {
        summonTab = LunaUtility.FindChildObject(gameObject, "Summon_Tab");
        goldTab = LunaUtility.FindChildObject(gameObject, "Gold_Tab");
        starStoneTab = LunaUtility.FindChildObject(gameObject, "StarStone_Tab");
        rubyTab = LunaUtility.FindChildObject(gameObject, "Ruby_Tab");
        rainbowStoneTab = LunaUtility.FindChildObject(gameObject, "RainbowStone_Tab");


        summonTab.transform.Find("TabButton").gameObject.GetComponent<Button>().onClick.AddListener(() => tabClick(summonTab));
        goldTab.transform.Find("TabButton").gameObject.GetComponent<Button>().onClick.AddListener(() => tabClick(goldTab));
        starStoneTab.transform.Find("TabButton").gameObject.GetComponent<Button>().onClick.AddListener(() => tabClick(starStoneTab));
        rubyTab.transform.Find("TabButton").gameObject.GetComponent<Button>().onClick.AddListener(() => tabClick(rubyTab));
        rainbowStoneTab.transform.Find("TabButton").gameObject.GetComponent<Button>().onClick.AddListener(() => tabClick(rainbowStoneTab));
        
        TurnOffAllOfScrollView();
        TurnOnScrollview(summonTab);
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

    private void TurnOnScrollview(GameObject tabObject)
    {
        GameObject scrollViewObj = getScrollViewGameObject(tabObject);
        scrollViewObj.SetActive(true);
    }
    private void TurnOffAllOfScrollView()
    {
        GameObject scrollViewObj = getScrollViewGameObject(summonTab);
        if (scrollViewObj.activeSelf == true)
        {
            scrollViewObj.SetActive(false);
        }
        
        scrollViewObj = getScrollViewGameObject(goldTab);
        if (scrollViewObj.activeSelf == true)
        {
            scrollViewObj.SetActive(false);
        }

        scrollViewObj = getScrollViewGameObject(starStoneTab);
        if (scrollViewObj.activeSelf == true)
        {
            scrollViewObj.SetActive(false);
        }

        scrollViewObj = getScrollViewGameObject(rubyTab);
        if (scrollViewObj.activeSelf == true)
        {
            scrollViewObj.SetActive(false);
        }

        scrollViewObj = getScrollViewGameObject(rainbowStoneTab);
        if (scrollViewObj.activeSelf == true)
        {
            scrollViewObj.SetActive(false);
        }
    }
    private void SwitchTab(string tabName)
    {
        TurnOffAllOfScrollView();
        switch (tabName)
        {
            case "Summon_Tab":
                {
                    TurnOnScrollview(summonTab);
                }
                break;
            case "Gold_Tab":
                {
                    TurnOnScrollview(goldTab);
                }
                break;
            case "StarStone_Tab":
                {
                    TurnOnScrollview(starStoneTab);
                }
                break;
            case "Ruby_Tab":
                {
                    TurnOnScrollview(rubyTab);
                }
                break;
            case "RainbowStone_Tab":
                {
                    TurnOnScrollview(rainbowStoneTab);
                }
                break;

            default:
                break;
        }
    }
    private void tabClick(GameObject tabObject)
    {
        SwitchTab(tabObject.name);
    }
    public void OnReleaseButton()
    {
        Game.Instance.uiManager.Pop(type);
        MyUnits.SetColliderEnabled(true);
    }


    //ÀÓ½Ã ¹æÆí¿ë »Ì±â
    public void OnReleaseResult()
    {
        GameObject resultObj = LunaUtility.FindChildObject(gameObject, "Result");
        resultObj.SetActive(false);
    }
    
    public void SetStringResult(string str)
    {
        GameObject Text = LunaUtility.FindChildObject(gameObject.transform.Find("Result").gameObject, "Text");
        Text.GetComponent<Text>().text = str;
    }

}

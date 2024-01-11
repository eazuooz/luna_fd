using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.UI;

public class UICharacterInfo : UILuna, IObserver
{
    public enum eState
    {
        Basic,
        JoinToParty,
    }

    public UIReference refData;

    private eState state;
    private string clickedType;
    private GameObject levelUpTab;
    private GameObject wakeUpTab;
    private GameObject levelup;
    private GameObject wakeup;
    
    private GameObject prefabObj;
    private ObjectData objData;
    private Status objStatus;

    private UICharacterList uiCharacterList;
    public UICharacterList UICharacterList 
    { 
        get { return uiCharacterList; }
        set { uiCharacterList = value; }
    }
    public int Priority { get; set; }
    public int ID { get; set; }
    public void OnResponse(object obj)
    {
        UpdateLevelUpInfo();
        UpdateRankUpButton();
        //CharacterStarRankInfoUpdate();
        //StarInfoInit();
    }
    //private Text infoText;



    public override void OnInit()
    {
        base.OnInit();

        uiCharacterList = GameObject.FindWithTag("UICharacterList").GetComponent<UICharacterList>();

        if(uiCharacterList.PickedUnitName == null)
        {
            Game.Instance.uiManager.Pop(type);
            return;
        }

        string[] info = uiCharacterList.PickedUnitName.Split('_');
        clickedType = info[1];


        prefabObj = GetClickedUIPrefab(info[0]);
        uiCharacterList.PickedUnitName = "";

        if (prefabObj == null)
        {
            return;
        }
        objStatus = prefabObj.GetComponent<Status>();
        objData = objStatus.objectData;

        InitTab();
        InitializeUnitData(levelup);
        UpdateLevelUpInfo();


        


        LunaDataTable.Instance.UnitTable[objData.engName].subject.AddObserver(this);
    }

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate()
    {



    }

    public override void OnLoop() { }

    public override void OnClear()
    {
        LunaDataTable.Instance.UnitTable[objData.engName].subject.RemoveObserver(this);
    }

    public void ReleaseUI()
    {
        //uICharacterList = GameObject.FindWithTag("UICharacterList");
        //if(uICharacterList)
        //    uICharacterList.GetComponent<UICharacterList>().SetActivCardDeck(true);

        Game.Instance.uiManager.Pop(type);
    }

    private void InitTab()
    {
        wakeUpTab = LunaUtility.FindChildObject(gameObject, "wakeUpTab");
        levelUpTab = LunaUtility.FindChildObject(gameObject, "levelupTab");

        wakeUpTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(wakeUpTab.name));
        levelUpTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(levelUpTab.name));

        if (levelup == null)
        {
            levelup = LunaUtility.FindChildObject(gameObject, "bgLevelUp");
        }

        if (wakeup == null)
        {
            wakeup = LunaUtility.FindChildObject(gameObject, "bgWakeUp");
        }

        InitializePartyButtons();

        levelup.SetActive(true);
        wakeup.SetActive(false);
    }

    private void OnTabClick(string tabName)
    {
        switch (tabName)
        {
            case "levelupTab":
                {
                    levelup.SetActive(true);
                    wakeup.SetActive(false);
                    InitializeUnitData(levelup);
                    UpdateLevelUpInfo();
                }
                break;

            case "wakeUpTab":
                {
                    levelup.SetActive(false);
                    wakeup.SetActive(true);
                    InitializeUnitData(wakeup);
                    UpdateWakeUpInfo();
                }
                break;


            default:
                {
                    Debug.Log("Debug info Tab!!");
                }
                break;
        }
    }

    private GameObject GetClickedUIPrefab(string name)
    {
        ObjectData objData = LunaDataTable.Instance.UnitTable[name];

        GameObject prefab = null;
        if (objData.UnitType == eUnitType.Hero)
        {
            prefab = Resources.Load<GameObject>("Prefabs/01Heroes/" + name);
        }
        else
        {
            prefab = Resources.Load<GameObject>("Prefabs/02Soldiers/" + name);
        }

        return prefab;
    }
    
    private void InitializePartyButtons()
    {
        List<GameObject> joinButtons = LunaUtility.FindChildObjects(transform.gameObject, "JoinPartyButton");
        foreach (GameObject go in joinButtons)
        {
            go.GetComponent<Button>().onClick.AddListener(OnClickJoinMyDeck);
        }

        List<GameObject> exitButtons = LunaUtility.FindChildObjects(transform.gameObject, "ExitPartyButton");
        foreach (GameObject go in exitButtons)
        {
            go.GetComponent<Button>().onClick.AddListener(OnClickExitMyDeck);
        }

        switch (clickedType)
        {
            case "Card":
                {
                    foreach (GameObject go in exitButtons)
                    {
                        go.SetActive(false);
                    }
                }
                break ;

            case "Deck":
                {
                    foreach (GameObject go in joinButtons)
                    {
                        go.SetActive(false);
                    }
                }
                break;
        }
    }

    private void InitializeUnitData(GameObject infoObj)
    {
        CharacterInfo_SetIllustration(infoObj.name, objData.illustPicture);
        CharacterInfo_SetName(infoObj.name, objData.illustName);
        CharacterInfo_SetInfoText(infoObj.name);
    }
    
    private void CharacterInfo_SetIllustration(string tabName, Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Transform bgTr = gameObject.transform.Find(tabName);
        Transform illustTr = bgTr.Find("Illustration");
        Transform imageTr = illustTr.Find("Image");
        imageTr.GetComponent<Image>().sprite = sprite;
        imageTr.GetComponent<Image>().SetNativeSize();
    }

    private void CharacterInfo_SetName(string tabName, string name)
    {
        if (name == "")
        {
            return;
        }

        Transform bgTr = gameObject.transform.Find(tabName);
        Transform illustTr = bgTr.Find("Illustration");
        Transform nameTextTr = illustTr.Find("NameText");

        nameTextTr.GetComponent<Text>().text = name;
    }

    private void UpdateCharacterInfoText(Text text)
    {
        int star = ((int)objData.ultState);
        int ultState = star;
        if (star > 5)
        {
            star = 5;
            ultState -= 5;
        }

        text.text = "-" + objData.Level.ToString() + "<color=lime>(+1)</color>" + " 레벨 " + "\n"
                        + "- " + star.ToString() + "성" + "\n"
                        + "- " + ultState.ToString() + "각성\n"
                        + "- 강화 카드 " + objData.cardCount.ToString() + "장";
    }

    private string CalculateTempString(string frontString)
    {
        string binStr = "";
        for (int i = frontString.Length - 1; i < 5; i++)
        {
            binStr += "  ";
        }

        return binStr;
    }

    private void UpdateLevelUpInfo()
    {
        GameObject statObj
            = LunaUtility.FindChildObject(levelup, "LevelUpStat");

        GameObject subObj = statObj.transform.Find("Text").gameObject;
        Text statText = subObj.GetComponent<Text>();

        //공격력 HP
        //방어력 스킬공격력
        statText.text = "-공격력 " + objData.GetAttackDamage_Value().ToString()
                        + "<color=lime>(+" + objData.LevelUpAttPoint.ToString() + ")</color>"
                        + CalculateTempString(objData.LevelUpAttPoint.ToString())
                        + "-HP " + objData.GetCurrentLevelHP_Value().ToString()
                        + "<color=lime>(+" + objData.LevelUpHpPoint.ToString() + ")</color>\n"
                        + "-방어력 " + objData.GetArmour_ValueInt().ToString()
                        + "<color=lime>(+" + objData.LevelUpArmourPoint.ToString() + ")</color>\n"
                        + CalculateTempString(objData.LevelUpArmourPoint.ToString())
                        + "-스킬 공격력 " + objData.GetResultSkillAtt_Value().ToString()
                        + "<color=lime>(+" + objData.LevelUpSkillDamage.ToString() + ")</color>\n";

        GameObject specialObj
             = LunaUtility.FindChildObject(levelup, "SpecialStat");

        specialObj.GetComponent<Text>().text
            = objData.skillName.ToString();

        GameObject subSpecialObj = specialObj.transform.Find("Text").gameObject;
        Text specialText = subSpecialObj.GetComponent<Text>();
        specialText.text = objData.skillStr;

        subSpecialObj = specialObj.transform.Find("Image").gameObject;
        Image image = subSpecialObj.GetComponent<Image>();
        image.sprite = objData.skillIcon;
        //image.SetNativeSize();

    }

    private void UpdateWakeUpInfo()
    {
        GameObject statObj
            = LunaUtility.FindChildObject(wakeup, "WakeUpStat");

        GameObject subObj = statObj.transform.Find("Text").gameObject;
        Text statText = subObj.GetComponent<Text>();

        statText.text = "3 성 : " + objData.start3.ToString() + "\n"
                        + "5 성 : " + objData.start5.ToString() + "\n\n"
                        + "1각성 : " + objData.ultra1.ToString() + "\n"
                        + "2각성 : " + objData.ultra2.ToString() + "\n";
    }

    private void UpdateRankUpButton()
    {
        if ((int)objData.ultState > 4)
        {
            GameObject statObj
                = LunaUtility.FindChildObject(wakeup, "WakeUpButton");

            GameObject imgObj
                = LunaUtility.FindChildObject(statObj, "SourceImage");

            //Text costText
            //    = LunaUtility.FindComponentByChildObject<Text>(statObj, "CostText");

            Sprite starStoneSprite
                = Resources.Load<Sprite>("UI/popup_ soldier/goods/starstone");

            imgObj.GetComponent<Image>().sprite = starStoneSprite;

            //costText.text = "x 2000";
        }
    }

    private void CharacterInfo_SetInfoText(string tabName)
    {
        Transform bgTr = gameObject.transform.Find(tabName);
        Transform illustTr = bgTr.Find("Illustration");
        Transform infoTextTr = illustTr.Find("InfoText");

        Text tempText = infoTextTr.GetComponent<Text>();
        UpdateCharacterInfoText(tempText);
    }

    public void OnClickCharacterLevelUp(int levelUpPrice)
    {
        int level = objData.Level;
        int cost = 1000 + (level * 1000);
        
        if (LunaDataTable.Instance.playerData.Gold < cost)
            return;

        objData.LevelUp();
        LunaDataTable.Instance.playerData.Gold -= cost;

        Text infoText =
            LunaUtility.FindComponentByChildObject<Text>(levelup, "InfoText");

        UpdateCharacterInfoText(infoText);
        UpdateLevelUpInfo();

        GameObject levelUpButton
            = LunaUtility.FindChildObject(gameObject, "LevelupButton");
        Transform costText = levelUpButton.transform.Find("CostText");
        costText.GetComponent<Text>().text = "x" + cost.ToString();
    }

    public void OnClickRankUp()
    {
        int level = (int)objData.ultState;
        int cost = 1000 + (level * 1000);

        if ((int)objData.ultState >= 7)
        {
            Debug.Log("최대강화입니다");

            return;
        }

        if (objData.cardCount < 10)
        {
            return;
        }

        //레벨 5이상이면 카드 소모
        //5이하이면 스타스톤 1000개 소모
        if ((int)objData.ultState >= 5)
        {
            LunaDataTable.Instance.playerData.StarStone -= 2000;
        }
        else
        {
            objData.cardCount -= 10;
        }

        objData.RankUp();

        Text infoText =
             LunaUtility.FindComponentByChildObject<Text>(wakeup, "InfoText");
        UpdateCharacterInfoText(infoText);

        //GameObject levelUpButton
        //    = LunaUtility.FindChildObject(gameObject, "WakeUpButton");
        //Transform costText = levelUpButton.transform.Find("StarStone").Find("CostText");
        //costText.GetComponent<Text>().text = "x" + cost.ToString();
    }

    public void OnClickJoinMyDeck()
    {
        string unitName = objData.engName;
        uiCharacterList.GetComponent<UICharacterList>().ReadyToJoinMyDeck(unitName);

        ReleaseUI();
    }

    public void OnClickExitMyDeck()
    {
        string unitName = objData.engName;
        uiCharacterList.GetComponent<UICharacterList>().ExitMyDeck(unitName);

        

        ReleaseUI();
    }
}

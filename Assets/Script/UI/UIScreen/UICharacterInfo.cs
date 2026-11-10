using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.UI;

public class UICharacterInfo : UILuna
{
    public enum eState
    {
        Basic,
        JoinToParty,
    }

    private eState state;
    private string clickedType;
    private GameObject levelUpTab;
    private GameObject wakeUpTab;
    private GameObject levelup;
    private GameObject wakeup;
    
    private GameObject prefabObj;
    private ObjectData objData;


    private UICharacterList uiCharacterList;
    public UICharacterList UICharacterList 
    { 
        get { return uiCharacterList; }
        set { uiCharacterList = value; }
    }


    public void OnResponse(object obj)
    {
        UpdateLevelUpInfo();
        UpdateRankUpButton();
        //CharacterStarRankInfoUpdate();
        //StarInfoInit();
    }
    //private Text infoText;



    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        

        uiCharacterList = GameObject.FindWithTag("UICharacterList").GetComponent<UICharacterList>();

        if(uiCharacterList.PickedUnitName == null)
        {
            LunaUI.Close(type);
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
        objData = prefabObj.GetComponent<UnitView>().Data;

        InitTab();
        InitializeUnitData(levelup);
        UpdateLevelUpInfo();
        UpdateRankUpButton();


        


        LunaDataTable.Instance.UnitTable[objData.engName].Changed += OnResponse;
    }

    

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);



    }

    

    protected override void OnClose(bool isShutdown, object userData)
    {
        try
        {
        if (objData != null) objData.Changed -= OnResponse;
    
        }
        finally { base.OnClose(isShutdown, userData); }
    }

    public void ReleaseUI()
    {
        //uICharacterList = GameObject.FindWithTag("UICharacterList");
        //if(uICharacterList)
        //    uICharacterList.GetComponent<UICharacterList>().SetActivCardDeck(true);

        LunaUI.Close(type);
    }

    private void InitTab()
    {
        wakeUpTab = LunaUIViews.FindChildObject(gameObject, "wakeUpTab");
        levelUpTab = LunaUIViews.FindChildObject(gameObject, "levelupTab");

        wakeUpTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(wakeUpTab.name));
        levelUpTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(levelUpTab.name));

        if (levelup == null)
        {
            levelup = LunaUIViews.FindChildObject(gameObject, "bgLevelUp");
        }

        if (wakeup == null)
        {
            wakeup = LunaUIViews.FindChildObject(gameObject, "bgWakeUp");
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
                    UpdateRankUpButton();
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
        List<GameObject> joinButtons = LunaUIViews.FindChildObjects(transform.gameObject, "JoinPartyButton");
        foreach (GameObject go in joinButtons)
        {
            go.GetComponent<Button>().onClick.AddListener(OnClickJoinMyDeck);
        }

        List<GameObject> exitButtons = LunaUIViews.FindChildObjects(transform.gameObject, "ExitPartyButton");
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
        int star = Mathf.Min(5, (int)objData.ultState);
        int ultState = Mathf.Max(0, (int)objData.ultState - 5);

        text.text = "-" + objData.Level.ToString() + "<color=lime>(+1)</color>" + " 레벨 " + "\n"
                        + "- " + star.ToString() + "성" + "\n"
                        + "- " + ultState.ToString() + "각성\n"
                        + "- 강화 카드 " + objData.cardCount + "장\n"
                        + (objData.ultState >= eUltState.ULT2 ? "최고 등급" : "승급: 카드 " + LunaEconomy.RankCardCost(objData.ultState) +
                            "장 · 별석 " + LunaEconomy.RankStoneCost(objData.ultState));
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
        var cost = LunaUIViews.FindChildObject(levelup, "LevelupButton").transform.Find("CostText");
        cost.GetComponent<Text>().text = objData.Level >= LunaEconomy.UnitMaxLevel ? "최대" : "×" + LunaEconomy.UnitLevelCost(objData.Level);
        GameObject statObj
            = LunaUIViews.FindChildObject(levelup, "LevelUpStat");

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
             = LunaUIViews.FindChildObject(levelup, "SpecialStat");

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
            = LunaUIViews.FindChildObject(wakeup, "WakeUpStat");

        GameObject subObj = statObj.transform.Find("Text").gameObject;
        Text statText = subObj.GetComponent<Text>();

        statText.text = "3 성 : " + objData.start3.ToString() + "\n"
                        + "5 성 : " + objData.start5.ToString() + "\n\n"
                        + "1각성 : " + objData.ultra1.ToString() + "\n"
                        + "2각성 : " + objData.ultra2.ToString() + "\n"
                        + "승급마다 전투 배치 체력과 공격력 +5%";
    }

    private void UpdateRankUpButton()
    {
        var button = LunaUIViews.FindChildObject(wakeup, "WakeUpButton");
        var cost = button.transform.Find("CostText")?.GetComponent<Text>();
        if (cost != null)
            cost.text = objData.ultState >= eUltState.ULT2 ? "최대" : "×" + LunaEconomy.RankCardCost(objData.ultState);
        var stones = button.transform.Find("StarStone");
        if (stones != null)
        {
            int amount = LunaEconomy.RankStoneCost(objData.ultState);
            stones.gameObject.SetActive(objData.ultState < eUltState.ULT2 && amount > 0);
            stones.Find("CostText").GetComponent<Text>().text = "×" + amount;
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
        if (!LunaEconomy.TryLevelUp(LunaDataTable.Instance.playerData, objData)) return;
        LunaRuntimeData.SaveNow();

        Text infoText =
            LunaUIViews.FindComponentByChildObject<Text>(levelup, "InfoText");

        UpdateCharacterInfoText(infoText);
        UpdateLevelUpInfo();

    }

    public void OnClickRankUp()
    {
        if (!LunaEconomy.TryRankUp(LunaDataTable.Instance.playerData, objData)) return;
        LunaRuntimeData.SaveNow();

        Text infoText =
             LunaUIViews.FindComponentByChildObject<Text>(wakeup, "InfoText");
        UpdateCharacterInfoText(infoText);
        UpdateRankUpButton();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Spine.Unity;
public class UIUpgrade : UILuna
{
    public delegate void ContentsCallBack();
    public delegate void SkillContentCallBack(string name);
    public class Contents
    {
        //basic
        public GameObject parent;
        public GameObject insertPrefab;
        public GameObject profile;
        public Text nameText;
        public Text cardText;

        public int currentLevel;
        public int maxLevel;

        //upgrade
        public Button button;

        //Equip
        public Button equipButton;
        public Button designButton;
    }

    public UIReference refData;

    private UILobby uiLobby;
    #region tab and view
    private GameObject lunaTab;
    private GameObject equipTab;
    private GameObject skillTab;

    private GameObject lunaUI;
    private GameObject lunaUIInstance;
    private GameObject lunaDiscription;
    private GameObject equipDiscription;
    private GameObject skillDiscription;

    private GameObject lunaScrollView;
    private GameObject equipScrollView;
    private GameObject skillScrollView;
    #endregion

    public GameObject player;

    private Dictionary<string, Contents> contentsMap
        = new Dictionary<string, Contents>();

    private List<GameObject> equipSkillButtons
        = new List<GameObject>();
    public override void OnInit()
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);
        uiLobby = GameObject.FindWithTag("UILobby").GetComponent<UILobby>();

        InitTabAndScrollView();
        InitProfile();
        InitContents();
        InitUpgradeCardText();
        InitEquipContentsText();

        InitTabs();
    }
    public override void OnActive() { }
    public override void OnInActive() { }
    public override void OnUpdate()
    {
    }
    public override void OnLoop() { }
    public override void OnClear() { }
    private void OnClickContent(Contents contents, ContentsCallBack callBack)
    {
        callBack();
    }
    private void OnClickSkillContents(string name, SkillContentCallBack callBack)
    {
        callBack(name);
    }
    private void OnClickViewEquipDesign(int itemNumber)
    {
        Game.Instance.dataManager.EquipDesignNumber = itemNumber;
        Game.Instance.uiManager.Push(UI_TYPE.EQUIPDESIGN);
    }
    private void AddLunaContentInContentsDictionary(string contentName, GameObject prefab, ContentsCallBack callBack)
    {
        Contents content = new Contents();
        content.parent = LunaUtility.FindChildObject(gameObject, contentName);
        content.profile = LunaUtility.FindChildObject(content.parent, "Profile");
        content.button = LunaUtility.FindComponentByChildObject<Button>(content.parent, "Button");


        GameObject NameObj = LunaUtility.FindChildObject(content.parent, "Name");

        if (NameObj != null)
        {
            content.nameText = NameObj.GetComponent<Text>();
            content.cardText = LunaUtility.FindComponentByChildObject<Text>(content.parent, "Manual");
        }
        content.insertPrefab = prefab;

        if (content.button != null)
            content.button.onClick.AddListener(() => OnClickContent(content, callBack));

        contentsMap.Add(contentName, content);
    }
    private void AddSKillContentInContentsDictionary(string contentName, GameObject prefab, SkillContentCallBack callBack)
    {
        Contents content = new Contents();
        content.parent = LunaUtility.FindChildObject(gameObject, contentName);
        content.button = LunaUtility.FindComponentByChildObject<Button>(content.parent, "Button");
        content.insertPrefab = prefab;

        if (content.button != null)
            content.button.onClick.AddListener(() => OnClickSkillContents(contentName, callBack));

        contentsMap.Add(contentName, content);
    }
    private void AddSkinContentInContentsDictionary(string contentName, GameObject prefab, ContentsCallBack callBack, int itemNumber = -1)
    {
        Contents content = new Contents();
        content.parent = LunaUtility.FindChildObject(gameObject, contentName);
        content.profile = LunaUtility.FindChildObject(content.parent, "Profile");
        content.equipButton = LunaUtility.FindComponentByChildObject<Button>(content.parent, "EquipButton");
        content.designButton = LunaUtility.FindComponentByChildObject<Button>(content.parent, "DesignButton");

        GameObject NameObj = LunaUtility.FindChildObject(content.parent, "Name");
        content.nameText = NameObj.GetComponent<Text>();
        content.cardText = LunaUtility.FindComponentByChildObject<Text>(content.parent, "Manual");
        content.insertPrefab = prefab;

        if (content.equipButton != null)
            content.equipButton.onClick.AddListener(() => OnClickContent(content, callBack));

        if (content.designButton != null)
            content.designButton.onClick.AddListener(() => OnClickViewEquipDesign(itemNumber));

        contentsMap.Add(contentName, content);
    }
    private void InitStatText()
    {
        Transform dis01 = lunaDiscription.transform.Find("Discription01");
        Transform dis02 = lunaDiscription.transform.Find("Discription02");

        string type = LunaDataTable.Instance.playerData.GetStringSkinType();

        ObjectData objData = null;
        LunaDataTable.Instance.lunaTable.TryGetValue(type, out objData);

        Text nameText = transform.Find("Board").Find("Title").Find("Name").GetComponent<Text>();
        nameText.text = "<루나 LV" + objData.Level.ToString() + ">";

        dis01.GetComponent<Text>().text
            = "-공격력 : " + objData.GetAttackDamage_Value().ToString() + "\n"
            + "-방어력 : " + objData.GetArmour_ValueInt().ToString() + "\n"
            + "-체력 : " + objData.GetCurrentLevelHP_Value().ToString() + "\n";


        dis02.GetComponent<Text>().text
            = "-공격속도 : " + objData.AttackOnceBySecondValue.ToString() + "초 \n"
            + "-이동속도 : " + objData.GetResultMoveSpeed_Value().ToString() + "\n";
    }
    private void UpdateStatText()
    {
        Transform dis01 = lunaDiscription.transform.Find("Discription01");
        Transform dis02 = lunaDiscription.transform.Find("Discription02");

        string type = LunaDataTable.Instance.playerData.GetStringSkinType();

        ObjectData objData = null;
        LunaDataTable.Instance.lunaTable.TryGetValue(type, out objData);

        Text nameText = transform.Find("Board").Find("Title").Find("Name").GetComponent<Text>();
        nameText.text = "<루나 LV" + objData.Level.ToString() + ">";

        dis01.GetComponent<Text>().text
            = "-공격력 : " + "<color=#00ff00>" + objData.GetAttackDamage_Value().ToString() + "</color>" + "\n"
            + "-방어력 : " + "<color=#00ff00>" + objData.GetArmour_ValueInt().ToString() + "</color>" + "\n"
            + "-체력 : " + "<color=#00ff00>" + objData.GetCurrentLevelHP_Value().ToString() + "</color>" + "\n";


        dis02.GetComponent<Text>().text
            = "-공격속도 : " + "<color=#00ff00>" + objData.AttackOnceBySecondValue.ToString() + "</color>" + "초 \n"
            + "-이동속도 : " + "<color=#00ff00>" + objData.GetResultMoveSpeed_Value().ToString() + "</color>" + "\n";
    }
    private void InitializeSkillSlot()
    {
        List<string> equipSkills = LunaDataTable.Instance.playerData.equippedSkill;

        int idx = 1;
        foreach (var skillName in equipSkills)
        {
            string childName = "Skill0" + idx.ToString();

            GameObject bttObject = skillDiscription.transform.Find(childName).gameObject;

            equipSkillButtons.Add(bttObject.transform.Find("EquipSlotEffect").gameObject);

            Image image = bttObject.GetComponent<Image>();
            SkillData data = LunaDataTable.Instance.skiiTable[skillName];
            image.sprite = data.skillImage;

            int skillIndex = idx - 1;
            bttObject.GetComponent<Button>().onClick.AddListener
                (() => OnClickSkillEquipButton(bttObject, skillIndex));

            idx += 1;
        }

        foreach (var item in equipSkillButtons)
        {
            item.SetActive(false);
        }
    }
    public void UpdateEquipDesignData()
    {
        {
            int itemNumber = (int)LunaDataTable.Instance.playerData.SkinType;
            ItemData data = LunaDataTable.Instance.itemTable[itemNumber];

            equipDiscription.transform.Find("Name").GetComponent<Text>().text
                = data.itemName.Replace(">", "LV") + data.level.ToString() + ">";
        }
        {
            int itemNumber = (int)LunaDataTable.Instance.playerData.SkinType;
            ItemData data = LunaDataTable.Instance.itemTable[itemNumber];
            EquipDesignItemData equipData = data as EquipDesignItemData;
            Image imageUI = equipDiscription.transform.Find("Sword").GetComponent<Image>();
            imageUI.sprite = LunaUtility.CreateSprite(equipData.weapon);
        }

        {
            int itemNumber = (int)LunaDataTable.Instance.playerData.SkinType;
            ItemData data = LunaDataTable.Instance.itemTable[itemNumber];
            EquipDesignItemData equipData = data as EquipDesignItemData;
            Image imageUI = equipDiscription.transform.Find("Shiled").GetComponent<Image>();
            imageUI.sprite = LunaUtility.CreateSprite(equipData.shield);
        }

        {
            int itemNumber = (int)LunaDataTable.Instance.playerData.SkinType;
            ItemData data = LunaDataTable.Instance.itemTable[itemNumber];
            EquipDesignItemData equipData = data as EquipDesignItemData;
            Image imageUI = equipDiscription.transform.Find("Ring").GetComponent<Image>();
            imageUI.sprite = LunaUtility.CreateSprite(equipData.ring);
        }
    }
    private void InitProfile()
    {
        PlayerData.eLunaSkin skinType
            = LunaDataTable.Instance.playerData.SkinType;
        InitializeLunaUI(lunaUI, skinType);

        InitStatText();
        //UpdateStatText();
        InitializeSkillSlot();
        UpdateEquipDesignData();


    }
    public void TurnOn_Off_SkillEffect(bool value)
    {

        foreach (var item in equipSkillButtons)
        {
            item.SetActive(value);
        }
    }
    private void InitTabAndScrollView()
    {
        lunaTab = GameObject.Find("Luna_Tab");
        equipTab = GameObject.Find("Equip_Tab");
        skillTab = GameObject.Find("Skill_Tab");

        Transform title = transform.Find("Board").Find("Title");
        lunaUI = title.Find("LunaUI").gameObject;



        lunaDiscription = title.Find("Upgrade").gameObject;
        equipDiscription = title.Find("Equip").gameObject;
        skillDiscription = title.Find("Skill").gameObject;



        lunaScrollView = transform.Find("Luna_ScrollView").gameObject;
        equipScrollView = transform.Find("Equip_ScrollView").gameObject;
        skillScrollView = transform.Find("Skill_ScrollView").gameObject;

        lunaTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(lunaScrollView.name));
        equipTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(equipScrollView.name));
        skillTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(skillScrollView.name));
    }
    private void InitContents()
    {
        InitUpgradeContents();
        InitEquipContents();
        InitSkillContents();
    }
    private void InitUpgradeContents()
    {
        AddLunaContentInContentsDictionary("Luna_LevelUp", player, Luna_LevelUp);
        AddLunaContentInContentsDictionary("IncreaseLunaMoveSpeed", player, LunaMoveSpeedLevelUp);
        AddLunaContentInContentsDictionary("IncreaseTowerHp", player, TowerHpLevelUp);
        AddLunaContentInContentsDictionary("IncreaseGoldSpeed", player, GoldSpeed_LevelUp);
        AddLunaContentInContentsDictionary("IncreaseAuroraRange", player, AuroraRange_LevelUp);
        AddLunaContentInContentsDictionary("IncreaseUnitMoveSpeed", player, UnitMoveSpeed_LevelUp);
        AddLunaContentInContentsDictionary("IncreaseLunaRevivalTime", player, LunaRevivalTime_LevelUp);
        //AddLunaContentInContentsDictionary("IncreaseTowerLevel", player, Tower_LevelUp);
    }
    private void InitEquipContents()
    {
        AddSkinContentInContentsDictionary("luna_basic", player, EquipLunaBasic, 0);
        AddSkinContentInContentsDictionary("luna_wood", player, EquipLunaWoodSkin, 1);
        AddSkinContentInContentsDictionary("luna_skull_ax", player, EquipLunaSkullAxSkin, 2);
        AddSkinContentInContentsDictionary("luna_slime", player, EquipLunaSlimeSkin, 3);
        AddSkinContentInContentsDictionary("luna_rich", player, EquipLunaRichSkin, 4);
        AddSkinContentInContentsDictionary("luna_dragon", player, EquipLunaDragonSkin, 5);
        AddSkinContentInContentsDictionary("luna_lene", player, EquipLunaLeneSkin, 6);
    }
    private void InitSkillContents()
    {
        AddSKillContentInContentsDictionary("FireBall", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("Blizzard", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("Healing", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("AngerArmy", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("BeleifWave", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("DemiGod", player, OnClickSkillButton);

        AddSKillContentInContentsDictionary("Dwarf", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("GoldyBlessing", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("KingSlime", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("LoneBleesing", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("SerenBlessing", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("SkeletonArcher", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("Thunder", player, OnClickSkillButton);
        AddSKillContentInContentsDictionary("WaterDropBarrier", player, OnClickSkillButton);

    }
    private void InitTabs()
    {
        lunaScrollView.SetActive(true);
        equipScrollView.SetActive(false);
        skillScrollView.SetActive(false);

        lunaDiscription.SetActive(true);
        equipDiscription.SetActive(false);
        skillDiscription.SetActive(false);
    }
    public void OnTabClick(string tabName)
    {
        switch (tabName)
        {
            case "Luna_ScrollView":
                {
                    lunaScrollView.SetActive(true);
                    equipScrollView.SetActive(false);
                    skillScrollView.SetActive(false);

                    lunaDiscription.SetActive(true);
                    equipDiscription.SetActive(false);
                    skillDiscription.SetActive(false);

                }
                break;

            case "Equip_ScrollView":
                {
                    lunaScrollView.SetActive(false);
                    equipScrollView.SetActive(true);
                    skillScrollView.SetActive(false);

                    lunaDiscription.SetActive(false);
                    equipDiscription.SetActive(true);
                    skillDiscription.SetActive(false);
                }
                break;

            case "Skill_ScrollView":
                {
                    lunaScrollView.SetActive(false);
                    equipScrollView.SetActive(false);
                    skillScrollView.SetActive(true);

                    lunaDiscription.SetActive(false);
                    equipDiscription.SetActive(false);
                    skillDiscription.SetActive(true);
                }
                break;

            default:
                {
                    Debug.Log("Debug Upgrade Tab!!");
                }
                break;
        }
        MyUnits.SetColliderEnabled(false);
    }
    private GameObject Luna_LoadPrefab(PlayerData.eLunaSkin skinType)
    {
        GameObject prefab = null;
        switch (skinType)
        {
            case PlayerData.eLunaSkin.Basic:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaBasicUI");
                }
                break;
            case PlayerData.eLunaSkin.Wood:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaWoodUI");
                }
                break;
            case PlayerData.eLunaSkin.SkullAx:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaSkullUI");
                }
                break;
            case PlayerData.eLunaSkin.Slime:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaSlimeUI");
                }
                break;
            case PlayerData.eLunaSkin.Rich:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaRichUI");
                }
                break;
            case PlayerData.eLunaSkin.Dragon:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaDragonUI");
                }
                break;
            case PlayerData.eLunaSkin.Lene:
                {
                    prefab = Resources.Load<GameObject>("Prefabs/09UI/UILuna/LunaLeneUI");
                }
                break;
            default:
                break;
        }

        return prefab;
    }
    private void InitializeLunaUI(GameObject parent, PlayerData.eLunaSkin skinType)
    {
        GameObject prefab = Luna_LoadPrefab(skinType);

        if (prefab == null)
            return;

        lunaUIInstance = Instantiate(prefab);
        if (lunaUIInstance == null)
            return;

        lunaUIInstance.transform.SetParent(parent.transform);
        lunaUIInstance.transform.localScale = prefab.transform.localScale;
        lunaUIInstance.transform.position = parent.transform.position;
        lunaUIInstance.transform.localPosition = Vector3.zero;
    }
    public void OnClickSkillEquipButton(GameObject bttObject, int idx)
    {
        string skillName = Game.Instance.dataManager.OnClickSkillName;

        if(skillName == "")
        {
            string temp = LunaDataTable.Instance.playerData.equippedSkill[idx];
            OnClickSkillButton(temp);
            TurnOn_Off_SkillEffect(false);

            return;
        }

        bool isExist = LunaDataTable.Instance.playerData.equippedSkill.Contains(skillName);
        if (isExist == false)
        {
            Image image = bttObject.GetComponent<Image>();
            SkillData data = LunaDataTable.Instance.skiiTable[skillName];
            image.sprite = data.skillImage;

            LunaDataTable.Instance.playerData.equippedSkill[idx] = skillName;

            uiLobby.UpdateLunaProfile();

            Game.Instance.dataManager.OnClickSkillName = "";
        }

        TurnOn_Off_SkillEffect(false);
    }
    public void OnClickReleaseButton()
    {
        if (SceneManager.GetActiveScene().name == "NewLobby")
            MyUnits.InstantiateLobby();

        Game.Instance.uiManager.Pop(type);
        MyUnits.SetColliderEnabled(true);
    }
    private void initUpgradeContentsInfo(Contents content, System.Func<string> nameFunc, System.Func<string> textFunc)
    {
        string nameStr = nameFunc();
        UpdateNameText(content.nameText, nameStr);

        string str = textFunc();
        UpdateManualText(content.cardText, str);
    }
    private void initEquipContentsInfo(Contents content, System.Func<string> nameFunc, System.Func<string> textFunc)
    {
        //callBack();
        string nameStr = nameFunc();
        UpdateNameText(content.nameText, nameStr);

        string str = textFunc();
        UpdateManualText(content.cardText, str);
    }
    private void InitUpgradeCardText()
    {
        Contents outContent;

        //UPGRADE
        contentsMap.TryGetValue("Luna_LevelUp", out outContent);
        initUpgradeContentsInfo(outContent, GetLunaLevelString, GetLunaLevelUpInfoString);

        contentsMap.TryGetValue("IncreaseLunaMoveSpeed", out outContent);
        initUpgradeContentsInfo(outContent, GetLunaMoveSpeedNameString, GetLunaMoveSpeedLevelUpInfoString);

        contentsMap.TryGetValue("IncreaseTowerHp", out outContent);
        initUpgradeContentsInfo(outContent, GetTowerHPNameString, GetTowerHPInfoString);

        contentsMap.TryGetValue("IncreaseGoldSpeed", out outContent);
        initUpgradeContentsInfo(outContent, GetGoldSpeedNameString, GetGoldSpeedInfoString);

        contentsMap.TryGetValue("IncreaseAuroraRange", out outContent);
        initUpgradeContentsInfo(outContent, AuroraRangeNameString, AuroraRangeInfoString);

        contentsMap.TryGetValue("IncreaseUnitMoveSpeed", out outContent);
        initUpgradeContentsInfo(outContent, UnitMoveSpeedNameString, UnitMoveSpeedInfoString);

        contentsMap.TryGetValue("IncreaseLunaRevivalTime", out outContent);
        initUpgradeContentsInfo(outContent, LunaRevivalTimeNameString, LunaRevivalTimeInfoString);

        //contentsMap.TryGetValue("IncreaseTowerLevel", out outContent);
        //initUpgradeContentsInfo(outContent, TowerLevelNameString, TowerLevelInfoString);
    }
    private void InitEquipContentsText()
    {
        Contents outContent;

        contentsMap.TryGetValue("luna_basic", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignBasicString, GetEquipDesignInfoBasicString);

        contentsMap.TryGetValue("luna_wood", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignWoodString, GetEquipDesignInfoWoodString);

        contentsMap.TryGetValue("luna_skull_ax", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignSkullAxString, GetEquipDesignInfoSkullAxString);

        contentsMap.TryGetValue("luna_slime", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignSlimeString, GetEquipDesignInfoSlimeString);

        contentsMap.TryGetValue("luna_rich", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignRichString, GetEquipDesignInfoRichString);

        contentsMap.TryGetValue("luna_dragon", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignDragonString, GetEquipDesignInfoDragonString);

        contentsMap.TryGetValue("luna_lene", out outContent);
        initEquipContentsInfo(outContent, GetEquipDesignLeneString, GetEquipDesignInfoLeneString);

    }
    private void UpdateManualText(Text cardText, string str)
    {
        cardText.text = str;
        cardText.color = Color.white;
    }
    private void UpdateNameText(Text nameText, string str)
    {
        nameText.text = str;
    }
    #region UPGRADE
    private bool IncreaseLevel(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.LunaLevelUp(cost) == false)
            return false;

        LunaDataTable.Instance.SetLunaDatasLevel(playerData.LunaLevel);

        UpdateStatText();
        return true;
    }
    private bool IncreaseLunaMoveSpeed(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.LunaMoveSpeedLevelUp(cost) == false)
            return false;

        ObjectData objectData
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData;

        objectData.moveSpeedLevel += 1;
        return true;
    }
    private bool IncreaseGoldSpeed(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.GoldSpeedLevelUp(cost) == false)
            return false;

        return true;
    }
    private bool IncreaseTowerHP(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.TowerHpLevelUp(cost) == false)
            return false;

        return true;
    }
    private bool IncreaseAuroraRangeLevel(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.AuroraLevelUp(cost) == false)
            return false;

        return true;
    }
    private bool IncreaseUnitMoveSpeedLevel(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.UnitMoveSpeedLevelUp(cost) == false)
            return false;

        return true;
    }
    private bool IncreaseLunaRevivalTimeLevel(int cost)
    {
        PlayerData playerData = LunaDataTable.Instance.playerData;
        if (playerData.LunaReviveTimeLevelUp(cost) == false)
            return false;

        return true;
    }
    //private void IncreaseTowerLevel(int cost)
    //{
    //    //LunaDataTable.Instance.playerData.Gold -= cost;
    //    //LunaDataTable.Instance.playerData.TowerLevel += 1;
    //}
    private void Luna_LevelUp()
    {
        if (IncreaseLevel(1000) == false)
            return;

        Contents outContent;
        contentsMap.TryGetValue("Luna_LevelUp", out outContent);

        string nameStr = GetLunaLevelString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = GetLunaLevelUpInfoString();
        UpdateManualText(outContent.cardText, str);
    }
    private string GetLunaLevelString()
    {
        ObjectData playerData
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData;

        string nameStr = "루나의 레벨이 "
           + playerData.Level.ToString() + "로 <color=whilte>+1</color> 증가합니다. (Max 5)";

        return nameStr;
    }
    private string GetLunaLevelUpInfoString()
    {
        ObjectData objectData
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData;

        string str = "";
        str += "- 공격력 " + objectData.GetAttackDamage_Value() + "( +" + objectData.LevelUpAttPoint.ToString() + ")";
        str += "    - 체력 " + objectData.GetCurrentLevelHP_Value() + "( +" + objectData.LevelUpHpPoint.ToString() + ")\n";
        str += "- 아머 " + objectData.GetArmour_ValueInt() + "( +" + objectData.LevelUpArmourPoint.ToString() + ")";
        //str += "    - 스킬공격력 " + objectData.GetResultSkillAtt_Value() + "( +" + objectData.levelUpSkillDamagePoint.ToString() + ")\n";

        return str;
    }
    private void LunaMoveSpeedLevelUp()
    {

        if (IncreaseLunaMoveSpeed(1000))
            return;

        Contents outContent;
        contentsMap.TryGetValue("IncreaseLunaMoveSpeed", out outContent);

        string nameStr = GetLunaMoveSpeedNameString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = GetLunaMoveSpeedLevelUpInfoString();
        UpdateManualText(outContent.cardText, str);
    }
    private string GetLunaMoveSpeedNameString()
    {
        ObjectData objectData
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData;

        string nameStr = "루나의 이동속도가 <color=whilte>"
            + objectData.GetAddedMoveSpeed().ToString()
            + "%</color> 증가합니다.";

        return nameStr;
    }
    private string GetLunaMoveSpeedLevelUpInfoString()
    {
        ObjectData objectData
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData;

        string str = "";
        str += "LV " + objectData.moveSpeedLevel.ToString() + "\n";
        str += "현재 " + objectData.GetAddedMoveSpeed().ToString() +"%";
        str += " -> " + objectData.GetAddedLevelUpMoveSpeed().ToString() + "%";

        return str;
    }
    private void TowerHpLevelUp()
    {
        if (IncreaseTowerHP(1000) == false)
            return;

        Contents outContent;
        contentsMap.TryGetValue("IncreaseTowerHp", out outContent);

        string nameStr = GetTowerHPNameString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = GetTowerHPInfoString();
        UpdateManualText(outContent.cardText, str);

    }
    private string GetTowerHPNameString()
    {
        string nameStr = "아군타워의 방어력이 <color=whilte>"
                        + LunaDataTable.Instance.playerData.TowerHPLevel.ToString()
                        + "%</color> 증가합니다.";

        return nameStr;
    }
    private string GetTowerHPInfoString()
    {
        string str = "";
        str += "LV " + LunaDataTable.Instance.playerData.TowerHPLevel.ToString() + "\n";
        str += "현재 " + LunaDataTable.Instance.playerData.TowerHPLevel.ToString() + "%";
        str += " -> " + (LunaDataTable.Instance.playerData.TowerHPLevel + 1).ToString() + "%";

        return str;
    }
    private void GoldSpeed_LevelUp()
    {
        if (IncreaseGoldSpeed(1000) == false)
            return;

        Contents outContent;
        contentsMap.TryGetValue("IncreaseGoldSpeed", out outContent);

        string nameStr = GetGoldSpeedNameString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = GetGoldSpeedInfoString();
        UpdateManualText(outContent.cardText, str);
    }
    private string GetGoldSpeedNameString()
    {
        string nameStr = "소환력이 <color=whilte>"
                + LunaDataTable.Instance.playerData.GoldSpeedLevel.ToString()
                + "%</color> 증가합니다.";

        return nameStr;
    }
    private string GetGoldSpeedInfoString()
    {
        string str = "";
        str += "LV " + LunaDataTable.Instance.playerData.GoldSpeedLevel.ToString() + "\n";
        str += "현재 " + LunaDataTable.Instance.playerData.GoldSpeedLevel.ToString() + "%";
        str += " -> " + (LunaDataTable.Instance.playerData.GoldSpeedLevel + 1).ToString() + "%";

        return str;
    }
    private void AuroraRange_LevelUp()
    {
        if (IncreaseAuroraRangeLevel(1000) == false)
            return;

        Contents outContent;
        contentsMap.TryGetValue("IncreaseAuroraRange", out outContent);

        string nameStr = AuroraRangeNameString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = AuroraRangeInfoString();
        UpdateManualText(outContent.cardText, str);
    }
    private string AuroraRangeNameString()
    {
        string nameStr = "오로라 범위가 <color=whilte>"
        + LunaDataTable.Instance.playerData.AuroraLevel.ToString()
        + "%</color> 증가합니다.";

        return nameStr;
    }
    private string AuroraRangeInfoString()
    {
        string str = "";
        str += "LV " + LunaDataTable.Instance.playerData.AuroraLevel.ToString() + "\n";
        str += "현재 " + LunaDataTable.Instance.playerData.AuroraLevel.ToString() + "%";
        str += " -> " + (LunaDataTable.Instance.playerData.AuroraLevel + 1).ToString() + "%";

        return str;
    }
    private void UnitMoveSpeed_LevelUp()
    {
        if (IncreaseUnitMoveSpeedLevel(1000))
            return;

        Contents outContent;
        contentsMap.TryGetValue("IncreaseUnitMoveSpeed", out outContent);

        string nameStr = UnitMoveSpeedNameString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = UnitMoveSpeedInfoString();
        UpdateManualText(outContent.cardText, str);
    }
    private string UnitMoveSpeedNameString()
    {
        string nameStr = "유닛 이동속도가 <color=whilte>"
                       + LunaDataTable.Instance.playerData.UnitMoveSpeedLevel.ToString()
                       + "%</color> 증가합니다.";

        return nameStr;
    }
    private string UnitMoveSpeedInfoString()
    {
        string str = "";
        str += "LV " + LunaDataTable.Instance.playerData.UnitMoveSpeedLevel.ToString() + "\n";
        str += "현재 " + LunaDataTable.Instance.playerData.UnitMoveSpeedLevel.ToString() + "%";
        str += " -> " + (LunaDataTable.Instance.playerData.UnitMoveSpeedLevel + 1).ToString() + "%";

        return str;
    }
    private void LunaRevivalTime_LevelUp()
    {
        if (IncreaseLunaRevivalTimeLevel(1000) == false)
            return;

        Contents outContent;
        contentsMap.TryGetValue("IncreaseLunaRevivalTime", out outContent);

        string nameStr = LunaRevivalTimeNameString();
        UpdateNameText(outContent.nameText, nameStr);

        string str = LunaRevivalTimeInfoString();
        UpdateManualText(outContent.cardText, str);
    }
    private string LunaRevivalTimeNameString()
    {
        string nameStr = "루나 리스폰 시간이 <color=whilte>"
               + LunaDataTable.Instance.playerData.LunaReviveTimeLevel.ToString()
               + "%</color> 감소합니다.";

        return nameStr;
    }
    private string LunaRevivalTimeInfoString()
    {
        string str = "";
        str += "LV " + LunaDataTable.Instance.playerData.LunaReviveTimeLevel.ToString() + "\n";
        str += "현재 " + LunaDataTable.Instance.playerData.LunaReviveTimeLevel.ToString() + "%";
        str += " -> " + (LunaDataTable.Instance.playerData.LunaReviveTimeLevel + 1).ToString() + "%";

        return str;
    }
    //private void Tower_LevelUp()
    //{
    //    IncreaseTowerLevel(1000);

    //    Contents outContent;
    //    contentsMap.TryGetValue("IncreaseTowerLevel", out outContent);

    //    string nameStr = TowerLevelNameString();
    //    UpdateNameText(outContent.nameText, nameStr);

    //    string str = TowerLevelInfoString();
    //    UpdateManualText(outContent.cardText, str);
    //}
    //private string TowerLevelNameString()
    //{
    //    //int level = LunaDataTable.Instance.playerData.TowerLevel;

    //    //string nameStr = "<타워 LV"
    //    //   + level.ToString() + ">";

    //   // string nameStr = "타워 레벨이 <color=whilte>+"
    //   //+ LunaDataTable.Instance.playerData.TowerLevel.ToString()
    //   //+ "</color> 증가합니다.";

    //   // return nameStr;
    //}
    //private string TowerLevelInfoString()
    //{
    //    //string str = "";
    //    //str += "- 타워 업그레이드";

    //    string str = "";
    //    str += "LV " + LunaDataTable.Instance.playerData.TowerLevel.ToString() + "\n";
    //    str += "LV 5, LV 10, LV 15 특수 공격추가\n";

    //    return str;
    //}
    #endregion
    #region EQUIP BUTTON
    private void OnClickDesignPopup()
    {
        Game.Instance.uiManager.Push(UI_TYPE.EQUIPDESIGN);
    }
    IEnumerator SetProfileAnimation(GameObject profile)
    {
        profile.GetComponent<AudioSource>().Play();
        profile.GetComponent<SkeletonGraphic>()
            .AnimationState.SetAnimation(0, "attack", false);
        profile.GetComponent<SkeletonGraphic>()
            .AnimationState.Complete += delegate (Spine.TrackEntry trackEntry)
            {
                if (trackEntry.Animation.Name == "attack")
                {
                    profile.GetComponent<SkeletonGraphic>()
                    .AnimationState.SetAnimation(0, "idle", true);
                }
            };
        yield return null;
    }
    private void EquipButtonAnimation(GameObject inst)
    {
        //Contents outContent;
        //contentsMap.TryGetValue(contentName, out outContent);
        StartCoroutine(SetProfileAnimation(inst));
    }
    public void EquipLunaSkin(PlayerData.eLunaSkin skinType)
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, skinType);
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaBasic()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.Basic);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.Basic;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();

        Game.Instance.uiManager.Push(UI_TYPE.POPUP);
        //UIPopup popup = Game.Instance.uiManager.Top() as UIPopup;
        //popup.SetMainText("장착하였습니다.");

        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaWoodSkin()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.Wood);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.Wood;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaSkullAxSkin()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.SkullAx);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.SkullAx;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaSlimeSkin()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.Slime);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.Slime;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaRichSkin()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.Rich);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.Rich;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaDragonSkin()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.Dragon);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.Dragon;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private void EquipLunaLeneSkin()
    {
        Destroy(lunaUIInstance);
        lunaUIInstance = null;

        InitializeLunaUI(lunaUI, PlayerData.eLunaSkin.Lene);

        EquipButtonAnimation(lunaUIInstance);
        LunaDataTable.Instance.playerData.SkinType
            = PlayerData.eLunaSkin.Lene;
        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
        UpdateEquipDesignData();
        uiLobby.UpdateLunaProfile();
    }
    private string GetEquipDesignBasicString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Basic;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoBasicString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Basic;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    private string GetEquipDesignWoodString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Wood;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoWoodString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Wood;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    private string GetEquipDesignSkullAxString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.SkullAx;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoSkullAxString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.SkullAx;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    private string GetEquipDesignSlimeString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Slime;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoSlimeString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Slime;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    private string GetEquipDesignRichString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Rich;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoRichString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Rich;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    private string GetEquipDesignDragonString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Dragon;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoDragonString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Dragon;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    private string GetEquipDesignLeneString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Lene;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string designName = data.itemName;
        designName = designName.Replace("<", "");
        designName = designName.Replace(">", "");

        string nameStr = "<" + designName + "LV"
           + data.level.ToString() + ">";

        return nameStr;
    }
    private string GetEquipDesignInfoLeneString()
    {
        int itemNumber = (int)PlayerData.eLunaSkin.Lene;

        EquipDesignItemData data
            = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        string str = "";
        str += "- 파츠 " + data.level + "/4 활성화" + "\n";
        str += " -" + data.dropStageLevel.ToString() + "스테이지에서 도안, 재료 드랍\n";

        return str;
    }
    #endregion
    #region SKILL
    private void OnClickSkillButton(string skillName)
    {
        Game.Instance.dataManager.OnClickSkillName = skillName;
        Game.Instance.uiManager.Push(UI_TYPE.SKILLINFO);
    }
    #endregion
}

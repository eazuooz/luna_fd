using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UILobby : UILuna
{
    public bool ProfileReady { get; private set; }



    public void OnResponse(object obj)
    {
        SetStageInfo();
    }


    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        ProfileReady = false;
        

        LunaSession.Current.CurrentStageName
            = LunaDataTable.Instance.playerData.SaveStageName;


        SetStageInfo();

        UpdateLunaProfile();

        LunaSession.Current.CurrentStageChanged += OnResponse;
    }

    

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);

    }

    

    protected override void OnClose(bool isShutdown, object userData)
    {
        try
        {
        ProfileReady = false;
        if (LunaSession.Current != null) LunaSession.Current.CurrentStageChanged -= OnResponse;
    
        }
        finally { base.OnClose(isShutdown, userData); }
    }

    public void OnClickCharacterList()
    {
        LunaUI.Open(UI_TYPE.CHARACTERLIST);
    }

    public void OnClickUpgrade()
    {
        LunaUI.Open(UI_TYPE.GOODS);
        LunaUI.Open(UI_TYPE.UPGRADE);
    }

    public void OnClickAdventure()
    {
        LunaUI.Open(UI_TYPE.ADVENTURE);
    }
    public void OnClickShop()
    {
        LunaUI.Open(UI_TYPE.GAMESHOP);
    }
    public void OnClickSelectDungeon()
    {
        LunaUI.Open(UI_TYPE.SELECTSTAGE);
    }
    public void OnClickInventory()
    {
        LunaUI.Open(UI_TYPE.INVENTORY);
    }

    private void MoveLobbyUnits()
    {
        foreach (GameObject unit in LunaSession.Current.lobbyUnits)
        {
            if (unit != null)
                unit.GetComponent<UnitLogic>().unitState.IsLobbyStartMove = true;
        }
    }

    private void LoadStage()
    {
        var profile = LunaDataTable.Instance.playerData;
        var stage = LunaStageTables.Get(profile.SaveStageName);
        string key = LunaStageTables.IsUnlocked(stage, profile) ? stage.Key : "Stage01-01";
        LunaSession.Current.CurrentStageName = key;
        LunaSession.Current.SetStagePrefab(key);
    }

    private void MoveGameScene()
    {
        LunaScenes.FadeIn(1.5f, () =>
        {
            LunaScenes.Load("Game");
        });
    }

    public void OnClickStart()
    {
        LoadStage();

        LunaScenes.CurrentLobbyGame?.OpenDoor();

        MoveLobbyUnits();
        MoveGameScene();
    }

    public void PushPopupUI()
    {
        LunaUI.Open(UI_TYPE.POPUP);
    }

    private void SetStageInfo()
    {
        Text stageName
            = LunaUIViews.FindComponentByChildObject<Text>(this.gameObject, "DungeonText");

        stageName.text = LunaSession.Current.CurrentStageLevelString("지하 ");

    }

    public void OnClickLunaProfile()
    {
        LunaUI.Open(UI_TYPE.UPGRADE);
    }

    public void UpdateLunaProfile()
    {
        Transform lunaProfile = transform.Find("Luna");
        Text levelText
            = lunaProfile.Find("NameBackImage")
            .Find("LevelText").GetComponent<Text>();

        // Lobby actors load asynchronously; profile data already exists in the session.
        levelText.text = "LV ​<color=#D1A606>" + LunaDataTable.Instance.playerData.LunaLevel.ToString() + "</color>";

        List<GameObject> skillObjects = new List<GameObject>();
        skillObjects.Add(lunaProfile.Find("LunaImage").Find("Skill_01").gameObject);
        skillObjects.Add(lunaProfile.Find("LunaImage").Find("Skill_02").gameObject);
        skillObjects.Add(lunaProfile.Find("LunaImage").Find("Skill_03").gameObject);

        int i = 0;
        foreach (GameObject obj in skillObjects)
        {
            string skillName = LunaDataTable.Instance.playerData.equippedSkill[i];
            SkillData skillData = LunaDataTable.Instance.skiiTable[skillName];

            obj.GetComponent<Image>().sprite = skillData.skillImage;
            i++;
        }

        int itemNumber = (int)LunaDataTable.Instance.playerData.SkinType;
        EquipDesignItemData data = LunaDataTable.Instance.itemTable[itemNumber] as EquipDesignItemData;

        GameObject design = lunaProfile.Find("LunaImage").Find("EquipDesign_00").gameObject;
        GameObject weapon = lunaProfile.Find("LunaImage").Find("EquipDesign_01").gameObject;
        GameObject shiled = lunaProfile.Find("LunaImage").Find("EquipDesign_02").gameObject;
        GameObject ring = lunaProfile.Find("LunaImage").Find("EquipDesign_03").gameObject;


        design.GetComponent<Image>().sprite
            = LunaUIViews.CreateSprite(data.equipIcon);

        weapon.GetComponent<Image>().sprite
            = LunaUIViews.CreateSprite(data.weapon);

        shiled.GetComponent<Image>().sprite
            = LunaUIViews.CreateSprite(data.shield);

        ring.GetComponent<Image>().sprite
            = LunaUIViews.CreateSprite(data.ring);
        ProfileReady = true;
    }
}

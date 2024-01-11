using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UILobby : UILuna, IObserver
{
    public UIReference refData;

    public GameObject backGround;
    public int Priority { get; set; }
    public int ID { get; set; }
    public void OnResponse(object obj)
    {
        SetStageInfo();
    }


    public override void OnInit()
    {
        base.OnInit();

        Game.Instance.dataManager.CurrentStageName
            = LunaDataTable.Instance.playerData.SaveStageName;

        backGround = GameObject.FindWithTag("BackGround");

        MyUnits.InstantiateLobby();
        SetStageInfo();

        UpdateLunaProfile();

        Game.Instance.dataManager.currentStageString_Subject.AddObserver(this);
    }

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate()
    {

    }

    public override void OnLoop() { }

    public override void OnClear()
    {
        Game.Instance.dataManager.currentStageString_Subject.RemoveObserver(this);
    }

    public void OnClickCharacterList()
    {
        Game.Instance.uiManager.Push(UI_TYPE.CHARACTERLIST);
    }

    public void OnClickUpgrade()
    {
        Game.Instance.uiManager.Push(UI_TYPE.GOODS);
        Game.Instance.uiManager.Push(UI_TYPE.UPGRADE);
    }

    public void OnClickAdventure()
    {
        Game.Instance.uiManager.Push(UI_TYPE.ADVENTURE);
    }
    public void OnClickShop()
    {
        Game.Instance.uiManager.Push(UI_TYPE.GAMESHOP);
    }
    public void OnClickSelectDungeon()
    {
        Game.Instance.uiManager.Push(UI_TYPE.SELECTSTAGE);
    }
    public void OnClickInventory()
    {
        Game.Instance.uiManager.Push(UI_TYPE.INVENTORY);
    }

    private void MoveLobbyUnits()
    {
        foreach (GameObject unit in Game.Instance.dataManager.lobbyUnits)
        {
            if (unit != null)
                unit.GetComponent<Status>().unitState.IsLobbyStartMove = true;
        }
    }

    private void LoadStage()
    {
        //게임 시작 최초 선택된 스테이지가 없을 경우
        if (Game.Instance.dataManager.CurrentStageName == "")
        {
            Game.Instance.dataManager.SelectStagePrefab =
                Resources.Load<GameObject>("Prefabs/10Stage/Stage01-01");
        }
        else
        {
            string stageDir = "Prefabs/10Stage/"
                    + Game.Instance.dataManager.CurrentStageName;

            Game.Instance.dataManager.SelectStagePrefab
                = Resources.Load<GameObject>(stageDir);
        }
    }

    private void MoveGameScene()
    {
        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Game");
        });
    }

    public void OnClickStart()
    {
        LoadStage();

        backGround.transform
            .Find("Entrance")
            .Find("Door").GetComponent<LobbyDoor>()
            .DoorOpenAction();

        MoveLobbyUnits();
        MoveGameScene();
    }

    public void PushPopupUI()
    {
        Game.Instance.uiManager.Push(UI_TYPE.POPUP);
    }

    private void SetStageInfo()
    {
        Text stageName
            = LunaUtility.FindComponentByChildObject<Text>(this.gameObject, "DungeonText");

        stageName.text = Game.Instance.dataManager.CurrentStageLevelString("지하 ");

    }

    public void OnClickLunaProfile()
    {
        Game.Instance.uiManager.Push(UI_TYPE.UPGRADE);
    }

    public void UpdateLunaProfile()
    {
        Transform lunaProfile = transform.Find("Luna");
        Text levelText
            = lunaProfile.Find("NameBackImage")
            .Find("LevelText").GetComponent<Text>();

        ObjectData playerData
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData;

        levelText.text = "LV ​<color=#D1A606>" + playerData.Level.ToString() + "</color>";

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
            = LunaUtility.CreateSprite(data.equipIcon);

        weapon.GetComponent<Image>().sprite
            = LunaUtility.CreateSprite(data.weapon);

        shiled.GetComponent<Image>().sprite
            = LunaUtility.CreateSprite(data.shield);

        ring.GetComponent<Image>().sprite
            = LunaUtility.CreateSprite(data.ring);



    }
}

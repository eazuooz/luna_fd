using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIStageInfo : UILuna
{
    public UIReference refData;

    private GameObject monsterContents;
    private GameObject itemContents;

    public override void OnInit()
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);
        gameObject.transform.Find("BackGround")
            .Find("Title")
            .Find("Text").GetComponent<Text>()
            .text = Game.Instance.dataManager.CurrentStageLevelString("악몽의 성 지하 ");

        gameObject.transform.Find("BackGround")
            .Find("SelectButton")
            .Find("LevelText").GetComponent<Text>()
            .text = Game.Instance.dataManager.CurrentStageLevelString("지하 ");

        monsterContents = gameObject.transform.Find("Monster_ScrollView")
            .Find("Monster_Scroll View_Viewport")
            .Find("Monster_Content").gameObject;

        itemContents = gameObject.transform.Find("Item_ScrollView")
            .Find("Item_Scroll View_Viewport")
            .Find("Item_Content").gameObject;

        //ProfileInformation

        GameObject stagePrefab
            = Game.Instance.dataManager.SelectStagePrefab;

        if (stagePrefab.GetComponent<BasicDungeon>().isBossStage == false)
        {
            gameObject.transform.Find("BackGround")
                .Find("Title")
                .Find("Skull").gameObject.SetActive(false);
        }

        //List<BasicDungeon.MonsterData> monsterList 
        //    = stagePrefab.GetComponent<BasicDungeon>().nonBossMonsters;

        string dungeonDataPath = "ScriptableObjects/DungeonData/BasicDungeon/" + stagePrefab.name.Replace("(Clone)", "");
        BasicDungeonData thisDungeonData = Resources.Load<BasicDungeonData>(dungeonDataPath);


        GameObject prefab
            = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");
        foreach (var item in thisDungeonData.nonBossMonsters)
        {
            Status status = item.monster.GetComponent<Status>();
            if (status == null)
                continue;

            Sprite monsterSprite
                = item.monster.GetComponent<Status>().objectData.profilePicture;

            if (monsterSprite == null)
                continue;

            GameObject instant 
                = Game.Instantiate(prefab);

            instant.transform.SetParent(monsterContents.transform);


            instant.GetComponent<RectTransform>().localScale = Vector3.one;
            instant.GetComponent<RectTransform>().position = gameObject.transform.position;
            instant.GetComponent<RectTransform>().localPosition = Vector3.zero;
            instant.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            instant.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            
            instant.GetComponent<ProfileInformation>().DisableCountText();

            if(monsterSprite != null)
                instant.GetComponent<ProfileInformation>().SetProfilePicture(monsterSprite);
        }

        List<BasicDungeonData.ItemInformation> itemList
            = new List<BasicDungeonData.ItemInformation>();

        foreach (var item in thisDungeonData.MonterDropTable)
        {
            itemList.Add(item);
        }
        foreach (var item in thisDungeonData.ItemDropTable)
        {
            itemList.Add(item);
        }


        foreach (var item in itemList)
        {
            GameObject instant
                = Game.Instantiate(prefab);

            instant.transform.SetParent(itemContents.transform);

            instant.GetComponent<RectTransform>().localScale = Vector3.one;
            instant.GetComponent<RectTransform>().position = gameObject.transform.position;
            instant.GetComponent<RectTransform>().localPosition = Vector3.zero;
            instant.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            instant.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            instant.GetComponent<ProfileInformation>().DisableCountText();

            Texture texture = item.itemData.IconImage;
            if (texture)
            {
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite itemSprite
                        = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));
                
                instant.GetComponent<ProfileInformation>().SetProfilePicture(itemSprite);
            }
        }
    }
    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void OnClickStartGame()
    {
        Game.Instance.uiManager.Pop(UI_TYPE.STAGEINFO);
        Game.Instance.uiManager.Pop(UI_TYPE.SELECTSTAGE);

        GameObject.FindWithTag("BackGround")
            .transform.Find("Entrance").Find("Door").GetComponent<LobbyDoor>().DoorOpenAction();
        foreach (GameObject unit in Game.Instance.dataManager.lobbyUnits)
        {
            if (unit != null)
                unit.GetComponent<Status>().unitState.IsLobbyStartMove = true;
        }


        //게임 시작 최초 선택된 스테이지가 없을 경우
        if (Game.Instance.dataManager.CurrentStageName == "")
        {
            //Game.Instance.dataManager.stageNum = 0;
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

        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Game");
        });
    }
    
    public void PushPopupUI()
    {
        Game.Instance.uiManager.Push(type);
    }

    public void ReleaseThisUI()
    {
        Game.Instance.uiManager.Pop(type);
    }
}

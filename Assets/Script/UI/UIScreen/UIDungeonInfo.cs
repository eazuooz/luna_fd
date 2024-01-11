using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIDungeonInfo : UILuna
{
    public UIReference refData;

    private GameObject monsterContents;
    private GameObject itemContents;
    private int dungeonNumber;
    public override void OnInit()
    {
        base.OnInit();


        GameObject stagePrefab
            = Game.Instance.dataManager.selectDungeon;





        //ProfileInformation

        if (stagePrefab.name == "Dungeon01")
        {
            dungeonNumber = 1;
            gameObject.transform.Find("BackGround")
    .Find("Title")
    .Find("Text").GetComponent<Text>()
    .text = stagePrefab.GetComponent<BossDungeon>().DungeonName;

            gameObject.transform.Find("BackGround")
                .Find("SelectButton")
                .Find("LevelText").GetComponent<Text>()
                .text = "던전 선택 하기";

            monsterContents = gameObject.transform.Find("Monster_ScrollView")
                .Find("Monster_Scroll View_Viewport")
                .Find("Monster_Content").gameObject;

            itemContents = gameObject.transform.Find("Item_ScrollView")
                .Find("Item_Scroll View_Viewport")
                .Find("Item_Content").gameObject;

            List<GameObject> monsterList
                = new List<GameObject>();

            monsterList.Add(stagePrefab.GetComponent<BossDungeon>().bossMonster);

            GameObject prefab
                = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");
            foreach (var item in monsterList)
            {
                GameObject instant
                    = Game.Instantiate(prefab);

                instant.transform.SetParent(monsterContents.transform);


                instant.GetComponent<RectTransform>().localScale = Vector3.one;
                instant.GetComponent<RectTransform>().position = gameObject.transform.position;
                instant.GetComponent<RectTransform>().localPosition = Vector3.zero;
                instant.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                instant.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                instant.GetComponent<ProfileInformation>().DisableCountText();
                Sprite monsterSprite
                    = item.GetComponent<Status>().objectData.profilePicture;

                instant.GetComponent<ProfileInformation>().SetProfilePicture(monsterSprite);
            }

            List<BossDungeon.ItemInformation> itemList
                = new List<BossDungeon.ItemInformation>();
            foreach (var item in stagePrefab.GetComponent<BossDungeon>().MonterDropTable)
            {
                itemList.Add(item);
            }
            foreach (var item in stagePrefab.GetComponent<BossDungeon>().ItemDropTable)
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
        else if (stagePrefab.name == "Dungeon04")
        {
            dungeonNumber = 4;
            gameObject.transform.Find("BackGround")
                    .Find("Title")
                    .Find("Text").GetComponent<Text>()
                    .text = stagePrefab.GetComponent<EndlessDungeon>().stageName;

            gameObject.transform.Find("BackGround")
                .Find("SelectButton")
                .Find("LevelText").GetComponent<Text>()
                .text = "던전 선택 하기";

            monsterContents = gameObject.transform.Find("Monster_ScrollView")
                .Find("Monster_Scroll View_Viewport")
                .Find("Monster_Content").gameObject;

            itemContents = gameObject.transform.Find("Item_ScrollView")
                .Find("Item_Scroll View_Viewport")
                .Find("Item_Content").gameObject;

            List<GameObject> monsterList
                = stagePrefab.GetComponent<EndlessDungeon>().Monsters;

            GameObject prefab
                = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");
            foreach (var item in monsterList)
            {
                GameObject instant
                    = Game.Instantiate(prefab);

                instant.transform.SetParent(monsterContents.transform);


                instant.GetComponent<RectTransform>().localScale = Vector3.one;
                instant.GetComponent<RectTransform>().position = gameObject.transform.position;
                instant.GetComponent<RectTransform>().localPosition = Vector3.zero;
                instant.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                instant.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                instant.GetComponent<ProfileInformation>().DisableCountText();
                Sprite monsterSprite
                    = item.GetComponent<Status>().objectData.profilePicture;

                instant.GetComponent<ProfileInformation>().SetProfilePicture(monsterSprite);
            }

            List<ItemData> itemList
                = new List<ItemData>();
            ItemData data;
            LunaDataTable.Instance.itemTable.TryGetValue(1000, out data);
            itemList.Add(data);
            LunaDataTable.Instance.itemTable.TryGetValue(1001, out data);
            itemList.Add(data);

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

                Texture texture = item.IconImage;
                if (texture)
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    Sprite itemSprite
                            = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));

                    instant.GetComponent<ProfileInformation>().SetProfilePicture(itemSprite);
                }
            }
        }
    }

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void ReleaseThisUI()
    {
        Game.Instance.uiManager.Pop(type);
    }

    public int DungeonNumber { get; set; }

    public void OnClickSelectDungeon(int idx)
    {
        Game.Instance.uiManager.Pop(type);
        Game.Instance.uiManager.Pop(UI_TYPE.SELECTSTAGE);

        string dungeonString = "Dungeon0";
        dungeonString += dungeonNumber.ToString();

        Game.Instance.dataManager.SelectDungeonString = dungeonString;

        string stageDir = "Prefabs/11Dungeons/"
                + Game.Instance.dataManager.SelectDungeonString;

        Game.Instance.dataManager.selectDungeon
            = Resources.Load<GameObject>(stageDir);

        MoveDungeonScene();

    }
    public void MoveDungeonScene()
    {
        //Game.Instance.uiManager.Pop(type);
        foreach (GameObject unit in Game.Instance.dataManager.lobbyUnits)
        {
            if (unit != null)
                unit.GetComponent<Status>().unitState.IsLobbyStartMove = true;
        }

        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Dungeon");
        });
    }

}

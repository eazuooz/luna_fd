using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIResult : UILuna
{
    public UIReference refData;

    private List<GameObject> resultItems
        = new List<GameObject>();
    public override void OnInit() 
    {
        base.OnInit();
        
        GameObject stage = GameObject.FindGameObjectWithTag("Stage");
        if (stage.GetComponent<BasicDungeon>() != null)
        {
            if (stage.GetComponent<BasicDungeon>().isStageWin == true)
            {
                transform.Find("ImagePlate").Find("ImageTitle").Find("ResultText").GetComponent<Text>().text = "승리!";

                transform.Find("ResultBoard").Find("ResultText").GetComponent<Text>().text = "'승리'";

                LunaDataTable.Instance.playerData.SaveStageName = Game.Instance.dataManager.NextStageString();
            }
            else
            {
                transform.Find("ImagePlate").Find("ImageTitle").Find("ResultText").GetComponent<Text>().text = "패배!";
                
                transform.Find("ResultBoard").Find("ResultText").GetComponent<Text>().text = "​<color=red>'패배'</color>";

                transform.Find("ImagePlate").Find("NextStage").gameObject.SetActive(false);
                transform.Find("ImagePlate").Find("CurrentStage").localPosition = new Vector2(0.0f, -225.0f);
            }

            transform.Find("Coin").Find("Text").GetComponent<Text>().text
                = "골드 " + ( LunaDataTable.Instance.playerData.RewardGold * 100 ).ToString() + "골드 획득\n";

            transform.Find("ImagePlate").Find("NextStage").Find("NextStage_Text").GetComponent<Text>().text
                =  Game.Instance.dataManager.NextStageLevelString();

            transform.Find("ImagePlate").Find("CurrentStage").Find("CurrentStage_Text").GetComponent<Text>().text
                = Game.Instance.dataManager.CurrentStageLevelString();
        }

        Transform itemContents 
            = gameObject.transform.Find("Item_ScrollView")
                                  .Find("Item_Scroll View_Viewport")
                                  .Find("Item_Content");

        GameObject prefab
            = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");


        int itemCount = 0;
        foreach (var item in Game.Instance.dataManager.pickUpData)
        {
            Texture texture = LunaDataTable.Instance.itemTable[item.Key].IconImage;
            if (texture)
            {
                GameObject instant
                    = Game.Instantiate(prefab);

                instant.transform.SetParent(itemContents.transform);
                instant.GetComponent<RectTransform>().localScale = Vector3.one;
                instant.GetComponent<RectTransform>().position = gameObject.transform.position;
                instant.GetComponent<RectTransform>().localPosition = Vector3.zero;
                instant.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                instant.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite monsterSprite
                    = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));

                instant.GetComponent<ProfileInformation>().SetProfilePicture(monsterSprite);
                instant.GetComponent<ProfileInformation>().SetCountText("x" + item.Value.ToString());
            }
            itemCount++;
        }

        int spacingValue = 0;
        switch (itemCount)
        {
            case 2:
                {
                    spacingValue = -400;
                }
                break;

            case 3:
                {
                    spacingValue = -300;
                }
                break;

            case 4:
                {
                    spacingValue = -150;
                }
                break;

            default:
                break;
        }
        itemContents.GetComponent<HorizontalLayoutGroup>().spacing = spacingValue;

    }

    public override void OnActive()
    {

    }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void ReStage()
    {
        Game.Instance.uiManager.Pop(UI_TYPE.RESULT);

        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Game");
        });
    }
    public void GoNextStage()
    {
        GameObject stage = GameObject.FindGameObjectWithTag("Stage");
        stage.GetComponent<BasicDungeon>().ToNextStage();
    }

    public void ReleaseBt()
    {
        Game.Instance.uiManager.Pop(UI_TYPE.RESULT);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyBackMove = true;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Lobby");
        });
    }

    public void PartyBt()
    {
        Game.Instance.uiManager.Push(UI_TYPE.CHARACTERLIST);
    }
}

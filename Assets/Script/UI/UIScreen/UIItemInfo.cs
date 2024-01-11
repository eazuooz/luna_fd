using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIItemInfo : UILuna
{
    public UIReference refData;


    public override void OnInit()
    {
        base.OnInit();

        GameObject profile = gameObject.transform
                                        .Find("BackGround")
                                        .Find("Profile").gameObject;

        int itemNumber = Game.Instance.dataManager.pickItemNumber;
        ItemData itemData;
        LunaDataTable.Instance.itemTable.TryGetValue(itemNumber, out itemData);
        Texture texture = itemData.IconImage;
        if (texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Sprite itemSprite
                    = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));

            profile.GetComponent<Image>().sprite = itemSprite;
        }


        profile.transform.Find("NameText").GetComponent<Text>().text = itemData.itemName;
        profile.transform.Find("MainText").GetComponent<Text>().text = itemData.itemDiscription;
    }
    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void OnClickStartGame()
    {
        
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

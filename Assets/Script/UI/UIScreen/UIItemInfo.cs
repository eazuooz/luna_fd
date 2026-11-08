using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIItemInfo : UILuna
{

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        

        GameObject profile = gameObject.transform
                                        .Find("BackGround")
                                        .Find("Profile").gameObject;

        int itemNumber = LunaSession.Current.pickItemNumber;
        ItemData itemData;
        LunaDataTable.Instance.itemTable.TryGetValue(itemNumber, out itemData);
        Texture texture = itemData.IconImage;
        if (texture)
        {
            Sprite itemSprite
                    = LunaUIViews.CreateSprite(texture);

            profile.GetComponent<Image>().sprite = itemSprite;
        }


        profile.transform.Find("NameText").GetComponent<Text>().text = itemData.itemName;
        profile.transform.Find("MainText").GetComponent<Text>().text = itemData.itemDiscription;
    }
    

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed); }

    

    protected override void OnClose(bool isShutdown, object userData)
    {
        try
        { 
        }
        finally { base.OnClose(isShutdown, userData); }
    }

    public void OnClickStartGame()
    {
        
    }
    
    public void PushPopupUI()
    {
        LunaUI.Open(type);
    }

    public void ReleaseThisUI()
    {
        LunaUI.Close(type);
    }
}

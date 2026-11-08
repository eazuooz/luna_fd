using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIInventory : UILuna
{

    private GameObject materialTab;
    private GameObject relicsTab;
    private GameObject designTab;
    private GameObject chestTab;
    private GameObject keyTab;

    private GameObject materialScorollView;
    private GameObject relicsScorollView;
    private GameObject designScorollView;
    private GameObject chestScorollView;
    private GameObject keyScorollView;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        
        

        materialTab = transform.Find("BackGround").Find("MaterialTab").gameObject;
        relicsTab = transform.Find("BackGround").Find("RelicsTab").gameObject;
        designTab = transform.Find("BackGround").Find("DesignTab").gameObject;
        chestTab = transform.Find("BackGround").Find("ChestTab").gameObject;
        keyTab = transform.Find("BackGround").Find("KeyTab").gameObject;

        materialScorollView = materialTab.transform.Find("MaterialScroll View").gameObject;
        relicsScorollView = relicsTab.transform.Find("RelicsScroll View").gameObject;
        designScorollView = designTab.transform.Find("DesignScroll View").gameObject;
        chestScorollView = chestTab.transform.Find("ChestScroll View").gameObject;
        keyScorollView = keyTab.transform.Find("KeyScroll View").gameObject;

        materialTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(materialTab.name));
        relicsTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(relicsTab.name));
        designTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(designTab.name));
        chestTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(chestTab.name));
        keyTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(keyTab.name));

        GameObject materialContents = this.materialTab.transform
            .Find("MaterialScroll View")
            .Find("MaterialViewport")
            .Find("MaterialContent").gameObject;
        
        Transform[] allchild = materialContents.GetComponentsInChildren<Transform>();
        int itemIndex = 1;
        foreach (var item in LunaDataTable.Instance.itemTable)
        {
            if (item.Value.IconImage && item.Value.type == ItemData.ItemType.MATERIAL)
            {
                Texture texture = item.Value.IconImage;
                if (texture)
                {
                    Sprite itemSprite
                        = LunaUIViews.CreateSprite(texture);

                    allchild[itemIndex].GetComponent<Image>().sprite
                        = itemSprite;

                    allchild[itemIndex].GetComponent<Image>().color = Color.white;

                    allchild[itemIndex].GetComponent<Button>().onClick.AddListener(() => OnClickItem(item.Value.itemNumber));
                }
                itemIndex++;
            }
        }

        GameObject designContents = this.designTab.transform
            .Find("DesignScroll View")
            .Find("DesignViewport")
            .Find("DesignContent").gameObject;

        allchild = designContents.GetComponentsInChildren<Transform>();
        itemIndex = 1;
        foreach (var item in LunaDataTable.Instance.itemTable)
        {
            if (item.Value.IconImage && item.Value.type == ItemData.ItemType.DESIGN)
            {
                Texture texture = item.Value.IconImage;
                if (texture)
                {
                    Sprite itemSprite
                        = LunaUIViews.CreateSprite(texture);

                    allchild[itemIndex].GetComponent<Image>().sprite
                        = itemSprite;

                    allchild[itemIndex].GetComponent<Image>().color = Color.white;
                    allchild[itemIndex].GetComponent<Button>().onClick.AddListener(() => OnClickItem(item.Value.itemNumber));
                }
                itemIndex++;
            }
        }

        //
        //
        relicsScorollView.SetActive(false);
        designScorollView.SetActive(false);
        chestScorollView.SetActive(false);
        keyScorollView.SetActive(false);
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

    private void AllTabOff()
    {
        materialScorollView.SetActive(false);
        relicsScorollView.SetActive(false);
        designScorollView.SetActive(false);
        chestScorollView.SetActive(false);
        keyScorollView.SetActive(false);
    }

    public void OnTabClick(string tabName)
    {
        AllTabOff();
        switch (tabName)
        {
            case "MaterialTab":
                {
                    materialScorollView.SetActive(true);
                }
                break;

            case "RelicsTab":
                {
                    relicsScorollView.SetActive(true);
                }
                break;

            case "DesignTab":
                {
                    designScorollView.SetActive(true);
                }
                break;

            case "ChestTab":
                {
                    chestScorollView.SetActive(true);
                }
                break;

            case "KeyTab":
                {
                    keyScorollView.SetActive(true);
                }
                break;

            default:
                {
                    Debug.Log("Debug Upgrade Tab!!");
                }
                break;
        }
    }

    private void OnClickItem(int itemNumber)
    {
        if (LunaUI.IsTop(UI_TYPE.ITEMINFO))
        {
            LunaUI.Close(UI_TYPE.ITEMINFO);
        }

        LunaSession.Current.pickItemNumber = itemNumber;
        LunaUI.Open(UI_TYPE.ITEMINFO);
    }

    public void PushPopupUI()
    {
        LunaUI.Open(type);
    }

    public void ReleaseThisUI()
    {
        if (LunaUI.IsTop(UI_TYPE.ITEMINFO))
        {
            LunaUI.Close(UI_TYPE.ITEMINFO);
        }
        LunaUI.Close(type);

        
    }
}

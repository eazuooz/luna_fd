using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIInventory : UILuna
{
    public UIReference refData;


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

    public override void OnInit() 
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);

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
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    Sprite itemSprite
                        = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));

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
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    Sprite itemSprite
                        = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));

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

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

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
        if (Game.Instance.uiManager.CompareTop(UI_TYPE.ITEMINFO))
        {
            Game.Instance.uiManager.Pop(UI_TYPE.ITEMINFO);
        }

        Game.Instance.dataManager.pickItemNumber = itemNumber;
        Game.Instance.uiManager.Push(UI_TYPE.ITEMINFO);
    }

    public void PushPopupUI()
    {
        Game.Instance.uiManager.Push(type);
    }

    public void ReleaseThisUI()
    {
        if (Game.Instance.uiManager.CompareTop(UI_TYPE.ITEMINFO))
        {
            Game.Instance.uiManager.Pop(UI_TYPE.ITEMINFO);
        }
        Game.Instance.uiManager.Pop(type);

        MyUnits.SetColliderEnabled(true);
    }
}

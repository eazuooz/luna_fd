
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Spine.Unity;

public class UIEquipDesign : UILuna
{
    public UIReference refData;

    private GameObject uiUpgrade;
    private EquipDesignItemData equipData;
    private GameObject instCharacterUI;
    public override void OnInit()
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);

        uiUpgrade = GameObject.FindWithTag("UIUpgrade");

        GetItemDataByItemTable();
        SetEquipDataProfile();

        SetEquipPartsData("Weapon", equipData.weapon, equipData.weaponeStr, equipData.weaponeDis);
        SetEquipPartsData("Shield", equipData.shield, equipData.shieldStr, equipData.shieldDis);
        SetEquipPartsData("Ring", equipData.ring, equipData.ringStr, equipData.ringDis);

        SetEquipButton();
    }

    public override void OnActive()
    {

    }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    private void GetItemDataByItemTable()
    {
        int itemNumber = Game.Instance.dataManager.EquipDesignNumber;

        ItemData data;
        LunaDataTable.Instance.itemTable.TryGetValue(itemNumber, out data);
        equipData = data as EquipDesignItemData;
    }

    private void SetEquipDataProfile()
    {
        GameObject parentObj = gameObject.transform.Find("Object").gameObject;
        instCharacterUI = Instantiate(equipData.uiPrefab);
        instCharacterUI.transform.SetParent(parentObj.transform);
        instCharacterUI.transform.localScale = Vector3.one;
        instCharacterUI.transform.localPosition = new Vector3(0.0f, -34.0f, 0.0f);
        
        gameObject.transform.Find("Name").GetComponent<Text>().text = equipData.itemName;
    }

    private void SetEquipPartsData(string partsName, Texture texture,  string name, string dis)
    {
        GameObject parts = gameObject.transform.Find(partsName).gameObject;

        Rect rect = Rect.zero;

        if (texture != null)
        {
            rect = new Rect(0, 0, texture.width, texture.height);
            parts.GetComponent<Image>().sprite
                = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));

            parts.transform.Find("Text").GetComponent<Text>().text
                = name + "\n"
                + dis;
        }
    }

    private void SetEquipButton()
    {
        GameObject equipBtt = gameObject.transform.Find("EquipButton").gameObject;
        equipBtt.GetComponent<Button>().onClick.AddListener(OnClickEquipButton);
    }
    public void OnClickEquipButton()
    {
        LunaDataTable.Instance.playerData.SkinType
            = (PlayerData.eLunaSkin)equipData.itemNumber;//PlayerData.eLunaSkin.SkullAx;

        uiUpgrade.GetComponent<UIUpgrade>().EquipLunaSkin(equipData.skinType);
        StartCoroutine(SetProfileAnimation(instCharacterUI));


        LunaDataTable.Instance.playerData.SetCurPlayer();

        MyUnits.InstantiateLobby();
    }

    public void OnClickReleaseButton()
    {
        Game.Instance.uiManager.Pop(type);
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
}
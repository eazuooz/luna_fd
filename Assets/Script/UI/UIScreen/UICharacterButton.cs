using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using Spine;
using Spine.Unity;
using UnityEngine.UI;



public class UICharacterButton : MonoBehaviour
{
    private ObjectData observed;
    private void OnDestroy() { if (observed != null) observed.Changed -= OnResponse; }
    public GameObject characterObj;
    private GameObject characterUI;
    private GameObject characterBg;
    private GameObject instCharBg;
    private GameObject instCharacterUI;

    private GameObject[] starts;
    private string prefabDir;
    public GameObject CharacterUI
    {
        get { return characterUI; }
    }
    public GameObject CharacterBg
    {
        get { return characterBg; }
    }


    public void OnResponse(object obj)
    {
        UpdateStarImgae();
        UpdateCardCount();
        UpdateLevelText();
    }

    public void SetCharacterPrefab(eUnitType type, string name)
    {
        switch (type)
        {
            case eUnitType.Hero:
                {
                    characterObj
                        = Resources.Load<GameObject>("Prefabs/01Heroes/" + name);
                }
                break;
            case eUnitType.Soldier:
                {
                    characterObj
                        = Resources.Load<GameObject>("Prefabs/02Soldiers/" + name);
                }
                break;
            default:
                break;
        }

    }
    public void SetUICharacter(string charName)
    {
        if (characterObj == null)
            return ;

        prefabDir = "Prefabs/09UI/UICharcter/";
        prefabDir += charName + "UI";
        characterUI =
            Resources.Load(prefabDir) as GameObject;

        if (characterUI == null)
        {
            Debug.Log(charName + "prefab is not found\n");
        }
    }

    public void InstantiateUIObject()
    {
        if (characterUI == null)
        {
            Debug.LogWarning("Character UI prefab is not assigned: " + prefabDir);

            return;
        }

        instCharacterUI = Instantiate(characterUI, transform.position, Quaternion.identity, transform);

        if (instCharacterUI == null)
        {
            Debug.Log(characterUI.name + "is not found\n");
        }

        instCharacterUI.transform.SetParent(gameObject.transform);
        instCharacterUI.transform.localScale = characterUI.transform.localScale;
        instCharacterUI.transform.position = gameObject.transform.position;
        instCharacterUI.transform.localPosition = new Vector2(0.0f, -30.0f);
        
    }

    public void SetBGImageInfo(int fontSize = 20)
    {
        ObjectData objData = instCharacterUI.GetComponent<UnitView>().Data;
        if (observed != null) observed.Changed -= OnResponse;
        observed = LunaDataTable.Instance.UnitTable[objData.engName];
        observed.Changed += OnResponse;

        prefabDir = "Prefabs/09UI/";
        prefabDir += "CharacterUIBackground";

        characterBg =
            Resources.Load(prefabDir) as GameObject;

        instCharBg = Instantiate(characterBg);

        instCharBg.transform.SetParent(gameObject.transform);
        instCharBg.transform.localScale = Vector3.one;
        instCharBg.transform.position = gameObject.transform.position;

        starts = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject starObj
                = LunaUIViews.FindChildObject(instCharBg, "StarImage" + i.ToString());

            starts[i] = starObj;
            starts[i].SetActive(false);
        }

        if (!LunaUIPresentation.Owned(objData))
        {
            instCharacterUI.transform.Find("SkeletonGraphic").GetComponent<SkeletonGraphic>().color
                = Color.black;
        }


        Transform charTr = instCharBg.transform.Find("NameText");
        Text charText = charTr.GetComponent<Text>();
        charText.fontSize = fontSize;
        charText.text = instCharacterUI.GetComponent<UnitView>().Data.UnitName;
        int characterLevel = characterUI.GetComponent<UnitView>().Data.Level;
        charText.text += " LV";
        charText.text += characterLevel.ToString();
        charText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); charText.fontSize = 16;
        charText.rectTransform.anchoredPosition = new Vector2(0f, -58f); charText.rectTransform.sizeDelta = new Vector2(150f, 36f);
        charText.resizeTextForBestFit = true; charText.resizeTextMinSize = 12; charText.resizeTextMaxSize = 16;
        charText.alignment = TextAnchor.MiddleCenter;

        Text cardCountText
            = LunaUIViews.FindComponentByChildObject<Text>(instCharBg, "CardText");

        LunaUIPresentation.CardQuantity(cardCountText, objData.cardCount);
        LunaUIPresentation.Label(transform, "UnitRole", LunaUIPresentation.Role(objData), new Vector2(0f, -80f), new Vector2(150f, 20f), 12, TextAnchor.MiddleCenter);

        int starLength = (int)instCharacterUI.GetComponent<UnitView>().Data.ultState;
        for (int i = 0; i < starLength; i++)
        {
            if (i > 4)
                break;

            starts[i].SetActive(true);
        }
    }
    public void SetEquipped(bool equipped)
    {
        var label = transform.Find("UnitRole")?.GetComponent<Text>();
        if (label == null) return;
        var data = characterUI.GetComponent<UnitView>().Data;
        label.text = (equipped ? "편성 · " : LunaUIPresentation.Owned(data) ? "보유 · " : "미보유 · ") + (data.AttackRange >= 3f ? "원거리" : "근접");
    }
    void UpdateLevelText()
    {
        Transform charTr = instCharBg.transform.Find("NameText");

        Text charText = charTr.GetComponent<Text>();

        charText.text = instCharacterUI.GetComponent<UnitView>().Data.UnitName;
        //charText.text = charText.text.Remove(charText.text.Length-2, 2);
        charText.text += " LV";
        charText.text += characterUI.GetComponent<UnitView>().Data.Level;
    }

    void UpdateStarImgae()
    {
        int starLength = (int)instCharacterUI.GetComponent<UnitView>().Data.ultState;
        for (int i = 0; i < starLength; i++)
        {
            if (i > 4)
                break;

            starts[i].SetActive(true);
        }
    }

    void UpdateCardCount()
    {
        Text cardText 
            = LunaUIViews.FindComponentByChildObject<Text>(gameObject, "CardText");

        LunaUIPresentation.CardQuantity(cardText, characterUI.GetComponent<UnitView>().Data.cardCount);
    }
}

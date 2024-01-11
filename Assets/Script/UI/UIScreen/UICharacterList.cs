using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using Spine;

//Collections.Generic.Dictionary<UnitName, UICharacterList.Unit>
//using UnitCard = System.Collections.Generic.Dictionary<string, UICharacterList.Unit>;
public class UICharacterList : UILuna, IObserver
{
    public enum eState
    {
        Basic,
        Join,
    }
    private eState state;
    // Observer Interface
    public int Priority { get; set; }
    public int ID { get; set; }
    public void OnResponse(object obj)
    {
        UpdateStatusData();
    }
    public UIReference refData;

    public class Tab
    {
        public GameObject tab;
        public GameObject contents;
        public List<GameObject> unitButtons;
    }

    private GameObject heroesTab;
    private GameObject soldierTab;
    private GameObject allTab;

    private GameObject heroScrollView;
    private GameObject soldierScrollView;
    private GameObject allScrollView;

    private GameObject allContents;
    private GameObject heroContents;
    private GameObject soldierContents;

    private List<GameObject> soldierCardButtons;
    private List<GameObject> heroCardButtons;
    private List<GameObject> allCardButtons;

    #region CardDeckInfo
    private Dictionary<int, UnitButton> myDecks
        = new Dictionary<int, UnitButton>();

    //private Dictionary<int, >
    //private Dictionary<string, Unit> deckButtons;
    public class Unit
    {
        public Unit()
        {
            button = null;
            unitObject = null;
            unitStatus = null;
        }
        /// <summary>
        /// Click Button Objet
        /// </summary>
        public Button button;
        /// <summary>
        /// UICharacter Object
        /// </summary>
        public GameObject unitObject;
        /// <summary>
        /// Character Status Object
        /// </summary>
        public GameObject unitStatus;

    }

    public class UnitButton
    {
        public string name;
        public Unit unit;
    }

    private string pickedUnitName;
    public string PickedUnitName
    {
        get { return pickedUnitName; }
        set { pickedUnitName = value; }
    }
    private List<GameObject> arrows = new List<GameObject>();
    #endregion

    public override void OnInit()
    {
        base.OnInit();
        MyUnits.SetColliderEnabled(false);

        state = eState.Basic;

        arrows = LunaUtility.FindChildObjects(gameObject, "Arrow");
        SetActiveArrows(false);

        ScrollViewInit();
        TabButtonInitialize();
        CreateCardButton();
        CardDeckInit();
        CharacterCardInitialize();
        SetDeckData();
        ObserverInitialize();
        ScrollViewInitialize();
    }

    public override void OnActive()
    {

    }

    public override void OnInActive()
    {

    }

    public override void OnUpdate()
    {
        
    }

    public override void OnLoop()
    {

    }

    public override void OnClear()
    {

    }

    private void SetActiveArrow(int idx, bool value)
    {
        arrows[idx].SetActive(value);
    }
    private void SetActiveArrows(bool value)
    {
        foreach(GameObject a in arrows)
        {
            a.SetActive(value);
        }
    }
    private void SetActiveArrows(eUnitType type, bool value)
    {
        if(type == eUnitType.Hero)
        {
            for(int i = 0; i < 4; i++)
            {
                arrows[i].SetActive(value);
            }
        }
        else
        {
            for (int i = 4; i < 6; i++)
            {
                arrows[i].SetActive(value);
            }
        }
    }
    private void ScrollViewInitialize()
    {
        heroScrollView.SetActive(true);
        soldierScrollView.SetActive(false);
        allScrollView.SetActive(false);
    }
    private void ObserverInitialize()
    {
        foreach (var card in LunaDataTable.Instance.UnitTable)
        {
            card.Value.subject.AddObserver(this);
        }
    }
    private UnitButton SetUnitButton(int index, string unitName)
    {
        UnitButton unitButton = myDecks[index];
        unitButton.name = unitName;

        string cardName = "Card_0" + index.ToString();
        Transform card = transform.Find("DeckBackGround").Find(cardName);

        unitButton.unit.unitObject = CreateUnitUIObject(eCardUiType.UnitUI, unitName);
        if(unitButton.unit.unitObject != null)
            SetParent(card.transform, unitButton.unit.unitObject.transform, new Vector2(0.0f, -30.0f));
        unitButton.unit.unitStatus = CreateUnitUIObject(eCardUiType.Status, unitName);
        if (unitButton.unit.unitObject != null)
            SetParent(card.transform, unitButton.unit.unitStatus.transform, new Vector2(0.95f, 1.0f));

        return unitButton;
    }
    private void AddUnitButtonInMyDeck(int key, UnitButton value)
    {
        myDecks[key] = value;
        myDecks[key].unit = value.unit;
        myDecks[key].unit.button = value.unit.button;
        myDecks[key].unit.unitObject = value.unit.unitObject;
        myDecks[key].unit.unitStatus = value.unit.unitStatus;

    //myDecks.Add(key, value);
    }
    private void CreateCardButton()
    {
        for (int index = 0; index < Defined.MaxUnitCnt; index++)
        {
            UnitButton unitButton = new UnitButton();
            unitButton.name = "";

            string cardName = "Card_0" + index.ToString();
            Transform card = transform.Find("DeckBackGround").Find(cardName);

            unitButton.unit = new Unit();
            unitButton.unit.button = card.GetComponent<Button>();

            int cardIndex = index;
            unitButton.unit.button.onClick.RemoveAllListeners();
            unitButton.unit.button.onClick.AddListener(() => OnClickDeckUnitButton(cardIndex));

            AddUnitButtonInMyDeck(index, unitButton);
        }



    }
    private void CardDeckInit()
    {
        List<string> saveUnitNames = LunaDataTable.Instance.playerData.UnitNamesInDeck;
        for (int i = 0; i < Defined.MaxUnitCnt; i++)
        {
            string unitName = saveUnitNames[i];
            if (unitName == "" || unitName == null)
                continue;

            SetUnitButton(i, unitName);

            //myDecks.Add(i, unitButton);
            //AddUnitButtonInMyDeck(i, unitButton);
        }
    }
    private void TabButtonInitialize()
    {
        heroesTab = GameObject.Find("HeroTab");
        soldierTab = GameObject.Find("SoldierTab");
        allTab = GameObject.Find("AllTab");

        heroesTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(heroScrollView.name));
        soldierTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(soldierScrollView.name));
        allTab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(allScrollView.name));

        allContents = LunaUtility.FindChildObject(gameObject, "AllContent");
        heroContents = LunaUtility.FindChildObject(gameObject, "HeroContent"); ;
        soldierContents = LunaUtility.FindChildObject(gameObject, "SoldierContent"); ;
    }
    private void CreateCharacterCard(ref GameObject contents, ref List<GameObject> addList, ObjectData data)
    {
        GameObject bgPrefab = Resources.Load<GameObject>("Prefabs/09UI/BackCardButton");
        GameObject charButton = Instantiate(bgPrefab, contents.transform);

        if (data.UnitType == eUnitType.Hero)
        {
            charButton.GetComponent<UICharacterButton>().SetCharacterPrefab(eUnitType.Hero, data.name);
            charButton.GetComponent<Button>().onClick.AddListener(() => OnClickCard(data.name));
            charButton.GetComponent<UICharacterButton>().SetUICharacter(data.name);
            charButton.GetComponent<UICharacterButton>().InstantiateUIObject();

            charButton.GetComponent<UICharacterButton>().SetBGImageInfo();
        }
        else
        {
            charButton.GetComponent<UICharacterButton>().SetCharacterPrefab(eUnitType.Soldier, data.name);
            charButton.GetComponent<Button>().onClick.AddListener(() => OnClickCard(data.name));
            charButton.GetComponent<UICharacterButton>().SetUICharacter(data.name);
            charButton.GetComponent<UICharacterButton>().InstantiateUIObject();

            charButton.GetComponent<UICharacterButton>().SetBGImageInfo(17);
        }

        charButton.SetActive(true);
        addList.Add(charButton);
    }
    private void CharacterCardInitialize()
    {
        allCardButtons = new List<GameObject>();
        heroCardButtons = new List<GameObject>();
        soldierCardButtons = new List<GameObject>();

        foreach (var data in LunaDataTable.Instance.UnitTable)
        {
            if (data.Key == null)
            {
                break;
            }

            //HeroTab
            if (data.Value.UnitType == eUnitType.Hero)
                CreateCharacterCard(ref heroContents, ref heroCardButtons, data.Value);
            //SoldierTab
            else
                CreateCharacterCard(ref soldierContents, ref soldierCardButtons, data.Value);


            //AllTab
            CreateCharacterCard(ref allContents, ref allCardButtons, data.Value);
        }
    }
    private void ScrollViewInit()
    {
        heroScrollView = transform.Find("HeroScroll View").gameObject;
        soldierScrollView = transform.Find("SoldierScroll View").gameObject;
        allScrollView = transform.Find("AllScroll View").gameObject;

        heroScrollView.SetActive(true);
        soldierScrollView.SetActive(true);
        allScrollView.SetActive(true);
    }
    public void OnTabClick(string tabName)
    {
        if (tabName == "HeroScroll View")
        {
            heroScrollView.SetActive(true);
            soldierScrollView.SetActive(false);
            allScrollView.SetActive(false);
        }
        else if (tabName == "SoldierScroll View")
        {
            heroScrollView.SetActive(false);
            soldierScrollView.SetActive(true);
            allScrollView.SetActive(false);
        }
        else if (tabName == "AllScroll View")
        {
            heroScrollView.SetActive(false);
            soldierScrollView.SetActive(false);
            allScrollView.SetActive(true);
        }
    }
    public void OnClickCard(string name)
    {
        switch (state)
        {
            case eState.Basic:
                {
                    PickedUnitName = name + "_Card";
                    ShowCharacterInfo();
                }
                break;

            case eState.Join:
                {

                }
                break;
        }
    }
    private void SetDeckData()
    {
        
    }
    public void ReadyToJoinMyDeck(string name)
    {
        state = eState.Join;
        pickedUnitName = name;
        ObjectData data = LunaDataTable.Instance.UnitTable[name];
        if ( data.UnitType == eUnitType.Hero)
        {
            SetActiveArrows(eUnitType.Hero, true);
        }
        else
        {
            SetActiveArrows(eUnitType.Soldier, true);
        }
    }
    public void ExitMyDeck(string name)
    {
        RemoveCardAsMyDeck(name);

        SetActiveArrows(false);
    }
    private void RemoveCard(UnitButton unitButton)
    {
        unitButton.name = null;
        //unitButton.unit.button.onClick.RemoveAllListeners();
        //unitButton.unit.button = null;
        Destroy(unitButton.unit.unitObject);
        unitButton.unit.unitObject = null;
        Destroy(unitButton.unit.unitStatus);
        unitButton.unit.unitStatus = null;
    }
    private void RemoveCardAsMyCard(int key, UnitButton value)
    {
        if(myDecks[key].name != "")
        {
            RemoveCard(value);
            //myDecks.Remove(key);
        }
    }
    private void RemoveCardAsMyDeck(string name)
    {
        foreach (var go in myDecks)
        {
            if (go.Value.name == name)
            {
                RemoveCardAsMyCard(go.Key, go.Value);
                break;
            }
        }
    }
    private bool InsertUnitMyDeck(int idx)
    {
        UnitButton unitButton = SetUnitButton(idx, pickedUnitName);
        AddUnitButtonInMyDeck(idx, unitButton);
        pickedUnitName = "";

        return true;
    }
    private bool DuplicatedCardCheck(int idx, string name)
    {
        foreach(var go in myDecks)
        {
            if (go.Value.name == name)
            {
                return false;
            }
        }
        
        ObjectData data
        = LunaDataTable.Instance.UnitTable[name];

        if (data.UnitType == eUnitType.Hero)
        {
            if (idx > Defined.MaxHeroCnt)
                return false;
        }
        else
        {
            if (idx < Defined.MaxHeroCnt)
                return false;
        }

        if (myDecks[idx].name == name)
            return false;

        foreach (var go in myDecks)
        {
            if (go.Value.name == name)
                return false;
        }

        return true;
    }
    public void OnClickDeckUnitButton(int idx)
    {
        SetActiveArrows(false);
        switch (state)
        {
            case eState.Basic:
                {
                    if (myDecks[idx].name == "")
                        return;

                    PickedUnitName = myDecks[idx].name + "_Deck";
                    ShowCharacterInfo();
                }
                break;

            case eState.Join:
                {
                    state = eState.Basic;
                    bool duplicated = DuplicatedCardCheck(idx, PickedUnitName);

                    if (!duplicated)
                        break;

                    if (myDecks.ContainsKey(idx))
                        RemoveCardAsMyCard(idx, myDecks[idx]);

                    InsertUnitMyDeck(idx);
                }
                break;
        }
    }
    public void OnClickReleaseButton()
    {
        SaveUnitsData();
        MyUnits.SetMyDeckUnitPrafab();

        foreach (var card in LunaDataTable.Instance.UnitTable)
        {
            card.Value.subject.OnClear();
        }

        if (Game.Instance.uiManager.CompareTop(UI_TYPE.CHARACTERINFO))
        {
            Game.Instance.uiManager.Pop(UI_TYPE.CHARACTERINFO);
        }

        Game.Instance.uiManager.Pop(type);

        if (SceneManager.GetActiveScene().name == "Lobby")
            MyUnits.InstantiateLobby();

        MyUnits.SetColliderEnabled(true);
    }
    public enum eCardUiType
    {
        UnitUI,
        Status,
    }
    public void SetParent(Transform parent, Transform child, Vector3 locaPosition)
    {
        //locaPosition = Vector3.zero;

        child.SetParent(parent);
        child.localScale = Vector3.one;

        child.position = parent.position;
        child.localPosition = locaPosition;//new Vector2(0.0f, -30.0f);
    }
    public GameObject CreateUnitUIObject(eCardUiType type, string name)
    {
        if (name == "")
            return null;

        GameObject unitUIObject = null;

        string prefabDir = "Prefabs/09UI/";
        switch (type)
        {
            case eCardUiType.UnitUI:
                {
                    prefabDir += "UICharcter/";
                    prefabDir += name + "UI";
                }
                break;
            case eCardUiType.Status:
                {
                    prefabDir += "CharacterUIBackground";
                }
                break;
        }

        GameObject unitPrefab =
            Resources.Load(prefabDir) as GameObject;

        if (unitPrefab == null)
            return null;

        unitUIObject = Instantiate(unitPrefab);

        if (type == eCardUiType.Status)
        {
            SetStatusData(name, unitUIObject);
        }

        return unitUIObject;
    }
    private void SetStatusData(string name, GameObject statusObject)
    {
        ObjectData objectData 
            = LunaDataTable.Instance.UnitTable[name];
        Transform charTr = statusObject.transform.Find("NameText");

        Text charText = charTr.GetComponent<Text>();

        charText.text = objectData.UnitName;
        charText.text += " LV";
        charText.text += objectData.Level;

        Text cardCountText
            = LunaUtility.FindComponentByChildObject<Text>(statusObject, "CardText");

        cardCountText.text = "+" + objectData.cardCount.ToString();

        GameObject[] starts = new GameObject[5];

        for (int i = 0; i < 5; i++)
        {
            GameObject starObj
                = statusObject.transform.Find("StarImage" + i.ToString()).gameObject;
                //= LunaUtility.FindChildObject(statusObject, "StarImage" + i.ToString());

            starts[i] = starObj;
            if(starts[i] != null)
                starts[i].SetActive(false);
        }

        int starLength = (int)objectData.ultState;
        for (int i = 0; i < starLength; i++)
        {
            if (i > 4)
                break;
            if (starts[i] != null)
                starts[i].SetActive(true);
        }
    }
    private void SaveUnitsData()
    {
        LunaDataTable.Instance.playerData.ReleaseUnitNameDeck();
        foreach (var go in myDecks)
        {
            LunaDataTable.Instance.playerData.UnitNamesInDeck[go.Key] = go.Value.name;
        }
    }
    private void UpdateStatusData()
    {
        foreach (var item in myDecks)
        {
            SetStatusData(item.Value.name, item.Value.unit.unitStatus);

        }
    }
    private bool ShowCharacterInfo()
    {
        if (Game.Instance.uiManager.CompareTop(UI_TYPE.CHARACTERINFO))
        {
            GameObject uiInfo = GameObject.FindWithTag("UICharacterInfo");
            uiInfo.GetComponent<UICharacterInfo>().OnInit();
        }
        else
        {
            Game.Instance.uiManager.Push(UI_TYPE.CHARACTERINFO);
        }

        return true;
    }
    public void OnClickPopup()
    {
        Game.Instance.uiManager.Push(UI_TYPE.POPUP);
    }
}

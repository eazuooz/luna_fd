using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UIPlayer : UILuna
{
    public UIReference refData;

    private GameObject currentStage;
    private GameObject uiProgressBar;

    private Text TimeText;

    private const int MAX_CARD_LENGTH = 6;
    private List<CardData> summonCards; 
        
    public List<CardData> SummonCards
    {
        get { return summonCards; }
    }
    //Money
    private Image goldGauge;
    private Text goldText;
    private int gold;
    private int maxGold;
    
    private Image manaGauge;
    private Text manaText;
    private int mana;
    private int maxMana;
    public int Money
    {
        get { return gold; }
        set { gold = value; }
    }
    public int Mana
    {
        get { return mana; }
        set { mana = value; }
    }

    //public PlayerData playerVariable;
    //private GameObject[] heroGameObjects = new GameObject[6];

    public class CardData
    {
        public Button button;
        public GameObject unitPrefab;
        public Image cardImage;
        public Status status;
        public GameObject heroInstance;
        public GameObject slotEffect;
        public Text levelText;


        public bool isCooltime;
        public float coolTime;
        public float leftTime;

        public bool isUp;
        public bool isDown;

        
        public CardData()
        {
            isDown = false;
            isUp = true;

            isCooltime = false;
            coolTime = 0.0f;
            leftTime = 0.0f;

            button = null;
            unitPrefab = null;
            heroInstance = null;
        }
    }


    Dictionary<KeyCode, System.Action> keypadClick;
    void KeypadInit()
    {
        keypadClick = new Dictionary<KeyCode, System.Action>
        {
            { KeyCode.A, () => TmpLMove() },
            { KeyCode.D, () => TmpRMove() },
            { KeyCode.Alpha1, () => OnClickSummon(0)},
            { KeyCode.Alpha2, () => OnClickSummon(1)},
            { KeyCode.Alpha3, () => OnClickSummon(2)},
            { KeyCode.Alpha4, () => OnClickSummon(3)},
            { KeyCode.Alpha5, () => OnClickSummon(4)},
            { KeyCode.Alpha6, () => OnClickSummon(5)},
            { KeyCode.Escape, () => OnClickEscape() },
        };
    }

    void TmpLMove()
    {
        if (Game.Instance.dataManager.LunaPlayer == null)
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                Game.Instance.dataManager.LunaPlayer = unit;
            }, IdentifyFunc.IsPlayer);

            if(Game.Instance.dataManager.LunaPlayer == null)
            {
                return;
            }
        }

        Game.Instance.dataManager.LunaPlayer.GetComponent<LunaStateMachine>().LMove();
    }

    void TmpRMove()
    {
        if (Game.Instance.dataManager.LunaPlayer == null)
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                Game.Instance.dataManager.LunaPlayer = unit;
            }, IdentifyFunc.IsPlayer);

            if (Game.Instance.dataManager.LunaPlayer == null)
            {
                return;
            }
        }

        Game.Instance.dataManager.LunaPlayer.GetComponent<LunaStateMachine>().RMove();
    }


    void OnClickEscape()
    {
        Game.Instance.uiManager.Pop(UI_TYPE.SKILL);
        Game.Instance.uiManager.Pop(UI_TYPE.PLAYER);
        Game.Instance.uiManager.Pop(UI_TYPE.PROGRESSBAR);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
            unit.GetComponent<Status>().unitState.IsLobbyBackMove = true;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        Game.Instance.lunaSceneManager.FadeIn(1.5f, () =>
        {
            SceneManager.LoadScene("Lobby");
        });
    }
    IEnumerator CreateAllUnit()
    {
        yield return 0;

        if (Game.Instance.dataManager.doNotSummonMode)
        {
            for (int i = 0; i < 6; i++)
            {
                if (GameObject.FindGameObjectWithTag("Stage").GetComponent<EndlessDungeon>() != null &
                    GameObject.FindGameObjectWithTag("Stage").GetComponent<EndlessDungeon>().randomYDistance == true)
                {
                    float tmpY = UnityEngine.Random.Range(-0.3f, 1.0f);

                    Vector3 tmpYPos = new Vector3(0, tmpY, tmpY);
                    CreateUnit(i, Defined.AllyTowerPosition + tmpYPos, Quaternion.identity);
                }
                else
                {
                    Vector3 createPos = Defined.AllyTowerPosition;
                    createPos.z = UnityEngine.Random.Range(0.10001f, 0.00001f);
                    CreateUnit(i, createPos, Quaternion.identity);
                }
            }
        }
    }
    public override void OnInit()
    {
        base.OnInit();
        Game.Instance.dataManager.pickUpData.Clear();
        //Game.Instance.dataManager.InstHeroes.Clear();
        uiProgressBar = GameObject.FindGameObjectWithTag("UIProgressBar");
        KeypadInit();

        StageInfoInit();
        initCardsAndWarmPool();
        InitCardButtons();
        InitGoldInfo();
        StartCoroutine(UpdateGoldAndMana());
        StartCoroutine(CreateAllUnit());

        TimeText = gameObject.transform.Find("TimeText").gameObject.GetComponent<Text>();
    }

    public override void OnActive() { }

    public override void OnInActive() 
    {
        
    }

    public override void OnUpdate()
    {
        PlayerRespawnCheck();
        UpdateHeroCoolTimeCheck();
        ButtonDisableChecker();
        CardButtonsCoolTimeCheck();
        UpdateMoneySlider();
        UpdateManaSlider();
        UpdateCardMpSliders();
        KeyBoardClickUpChecker();

        if (GameObject.FindGameObjectWithTag("Stage").GetComponent<BossDungeon>() != null)
        {
            TimeText.text = GameObject.FindGameObjectWithTag("Stage").GetComponent<BossDungeon>().dungeonCount.ToString("F2");
        }

        //uiProgressBar.GetComponent<UIProgressBar>().DeleteProfile();
        //foreach (var item in summonCards)
        //{
        //    uiProgressBar.GetComponent<UIProgressBar>().CreateProfile(item.heroInstance);
        //}

    }

    public override void OnLoop() { }

    public override void OnClear() 
    {
        Game.Instance.uiManager.Pop(UI_TYPE.SKILL);
        
    }

    private void StageInfoInit()
    {
        Time.timeScale = 1.5f;

        if (GameObject.FindGameObjectWithTag("Stage") != null)
            currentStage = GameObject.FindGameObjectWithTag("Stage");
    }

    private void initCardsAndWarmPool()
    {
        summonCards = new List<CardData>();

        for (int i = 0; i < MAX_CARD_LENGTH; i++)
        {
            summonCards.Add(new CardData());
        }

        LoadUnitInfo(Game.Instance.dataManager.HeroePrefabs, 0, 1);
        LoadUnitInfo(Game.Instance.dataManager.SoldierPrefabs, 4, 50);
    }
    private void LoadUnitInfo(List<GameObject> unitPrefabs, int startIndex, int warmCount)
    {
        for (int i = 0; i < unitPrefabs.Count; i++)
        {
            if (unitPrefabs[i] == null)
                continue;

            summonCards[i+ startIndex].unitPrefab 
                = unitPrefabs[i];
            summonCards[i + startIndex].status
                = unitPrefabs[i].GetComponent<Status>();

            PoolManager.WarmPool(unitPrefabs[i], warmCount);
        }
    }
    private void InitCardButtons()
    {
        Button[] allChildren = GetComponentsInChildren<Button>();
        Dictionary<string, Button> buttons
            = new Dictionary<string, Button>();
        for (int i = 0; i < allChildren.Length; i++)
        {
            buttons.Add(allChildren[i].name, allChildren[i]);
        }

        summonCards[0].button = buttons["Button_0"];
        summonCards[1].button = buttons["Button_1"];
        summonCards[2].button = buttons["Button_2"];
        summonCards[3].button = buttons["Button_3"];
        summonCards[4].button = buttons["Button_4"];
        summonCards[5].button = buttons["Button_5"];

        foreach (var card in summonCards)
        {
            int index = summonCards.IndexOf(card);
            AddListenerInSummonButton(index);
            initSummonCardInfo(index);
        }
    }
    private void AddListenerInSummonButton(int idx)
    {
        summonCards[idx].button.onClick.AddListener(delegate { OnClickSummon(idx); });
    }
    private void initSummonCardInfo(int idx)
    {
        if (summonCards[idx].unitPrefab != null)
        {
            InitTime(idx);
            InitImage(idx);
            InitButtonPrice(idx);
            InitProfilePicture(idx);
            InitSkillSlider(idx);
        }
        else
        {
            offProfile(idx);
        }
    }
    private void offProfile(int idx)
    {
        summonCards[idx].button.transform.Find("Image").gameObject.SetActive(false);
        summonCards[idx].button.transform.Find("Stars").gameObject.SetActive(false);
    }
    private void InitGoldInfo()
    {
        gold = 0;
        mana = 0;
        maxGold = 100;
        maxMana = 100;

        goldText
            = gameObject.transform.Find("GoldText").GetComponent<Text>();
        manaText
            = gameObject.transform.Find("ManaText").GetComponent<Text>();

        gameObject.transform.Find("GoldMaxText").GetComponent<Text>().text
            = "/" + maxGold.ToString();
        gameObject.transform.Find("ManaMaxText").GetComponent<Text>().text
            = "/" + maxMana.ToString();

        goldGauge
            = gameObject.transform.Find("GoldGaugeImage").GetComponent<Image>();
        manaGauge
            = gameObject.transform.Find("ManaGaugeImage").GetComponent<Image>();

        gameObject.transform.Find("NameBackImage").Find("LevelText").GetComponent<Text>().text
            = "LV. " + "​<color=#D1A606>" + Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().objectData.Level.ToString() + "</color>";
        //LV ​<color=#D1A606>20</color>
    }

    void InitSkillSlider(int idx)
    {
        ObjectData objData
            = summonCards[idx].status.objectData;

        GameObject slider
            = summonCards[idx].button.transform.Find("Slider").gameObject;

        if (objData.UnitType == eUnitType.Hero)
        {
            slider.SetActive(true);
        }
    }

    private void InitProfilePicture(int idx)
    {
        ObjectData objData = summonCards[idx].status.objectData;

        summonCards[idx].button.GetComponent<Image>().sprite
            = objData.cardPicture;

        Transform imgTr = summonCards[idx].button.transform.Find("Image");
        if (objData.profilePicture != null)
        {
            imgTr.gameObject.SetActive(true);
            imgTr.GetComponent<Image>().sprite
                = objData.profilePicture;
        }
        else
        {
            imgTr.gameObject.SetActive(false);
        }

        
        Transform slotEffectTr = summonCards[idx].button.transform.Find("SlotEffect");
        if (slotEffectTr)
        {
            summonCards[idx].slotEffect = slotEffectTr.gameObject;
            summonCards[idx].slotEffect.SetActive(false);
        }

        Transform levelText = summonCards[idx].button.transform.Find("LevelText");
        if (levelText)
        {
            summonCards[idx].levelText = levelText.GetComponent<Text>();
            string level = summonCards[idx].unitPrefab.GetComponent<Status>().objectData.Level.ToString();
            summonCards[idx].levelText.text = level;
        }
    }
    private void InitButtonPrice(int idx)
    {
        ObjectData objData
            = summonCards[idx].unitPrefab.GetComponent<Status>().objectData;
        Text coinText
            = summonCards[idx].button.transform.Find("Summons").Find("Text").GetComponent<Text>();
        int unitPrice = objData.Price;
        coinText.text = unitPrice.ToString();
    }
    private void InitImage(int idx)
    {
        summonCards[idx].cardImage
            = summonCards[idx].button.GetComponent<Image>();
    }
    private void InitTime(int idx)
    {
        summonCards[idx].coolTime = summonCards[idx].status.objectData.SpawnDelayTime;
        summonCards[idx].leftTime = summonCards[idx].coolTime;
    }
    public void ButtonDisableChecker()
    {
        for (int i = 0; i < MAX_CARD_LENGTH; i++)
        {
            //if (summonCards[i].unitPrefab != null)
                buttonOnOffCheck(i);
        }
    }

    private void buttonOnOffCheck(int idx)
    {
        if (summonCards[idx].unitPrefab == null)
        {
            //ImageOff(summonCards[idx].cardImage);
            //if (summonCards[idx].isUp == true)
            //{
            //summonCards[idx].button.gameObject.GetComponent<RectTransform>().localPosition
                   // = new Vector3(0, 46, 0);

            //    summonCards[idx].isDown = true;
            //    summonCards[idx].isUp = false;
            //}

            return;
        }

        //    int unitCnt = 0;
        //foreach (var card in summonCards)
        //{
        //    if (card.unitPrefab != null)
        //        unitCnt++;
        //}

        //if (idx >= unitCnt)
        //{
        //    return;
        //}

        if (IsHeroSummoned(idx))
        {
            return;
        }

        if (IsMoneyImPossible(idx))
        {
            ImageOff(summonCards[idx].cardImage);
            if (summonCards[idx].isUp == true)
            {
                summonCards[idx].button.gameObject.GetComponent<RectTransform>().localPosition
                    -= new Vector3(0, 43, 0);

                summonCards[idx].isDown = true;
                summonCards[idx].isUp = false;
            }
        }
        else
        {
            ImageOn(summonCards[idx].cardImage);

            if (summonCards[idx].isDown == true)
            {
                summonCards[idx].button.gameObject.GetComponent<RectTransform>().localPosition
                    += new Vector3(0, 43, 0);

                summonCards[idx].isUp = true;
                summonCards[idx].isDown = false; ;
            }
        }
    }

    private bool IsMoneyImPossible(int idx)
    {
        if (idx >= summonCards.Count)
        {
            return false;
        }

        bool returnBoolen
            = gold < summonCards[idx].status.objectData.Price;

        return returnBoolen;
    }
    private bool IsHeroSummoned(int idx)
    {
        if (summonCards[idx].heroInstance == null)
            return false;

        bool returnBoolen
            = summonCards[idx].status.unitState.IsSummonState == true;
        returnBoolen
            = returnBoolen && summonCards[idx].status.UnitType == eUnitType.Hero;

        return returnBoolen;
    }
    private void ImageOff(Image image)
    {
        image.color
            = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);


    }

    private void ImageOn(Image image)
    {
        image.color
            = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    private void UpdateHeroCoolTimeCheck()
    {
        for (int idx = 0; idx < Defined.MaxHeroCnt; idx++)
        {
            if (summonCards[idx].heroInstance == null)
                continue;

            if (summonCards[idx].status.unitState.IsPossibleSummonState == false)
            {

                summonCards[idx].status.unitState.IsPossibleSummonState = true;
                summonCards[idx].slotEffect.SetActive(false);
                //mButtons[i].transform.Find("Slider").gameObject.activeSelf = false;
                Slider slider
                        = summonCards[idx].button.transform.Find("Slider").gameObject.GetComponent<Slider>();

                SetSliderValue(slider, 1.0f);

                //StartCoolTime(idx);
            }
        }

    }

    private void CardButtonsCoolTimeCheck()
    {
        for (int i = 0; i < summonCards.Count; i++)
        {
            if (Check_ClickedAndLeftTime(i))
            {
                summonCards[i].leftTime -= Time.deltaTime * 5.0f;

                if (summonCards[i].leftTime < 0.0f)
                {
                    PossibleSummonState(i);
                    continue;
                }

                if (isButtonCoolTime(summonCards[i]))
                {
                    ImPossibleSummonState(i);
                }
            }
        }
    }

    private bool isButtonCoolTime(CardData buttonInfo)
    {
        return buttonInfo.cardImage &&
                    buttonInfo.isCooltime == false;
    }
    private bool Check_ClickedAndLeftTime(int idx)
    {
        return summonCards[idx].leftTime > 0;
    }
    private void PossibleSummonState(int idx)
    {
        summonCards[idx].leftTime = 0;

        if (summonCards[idx].button)
            summonCards[idx].button.enabled = true;

        summonCards[idx].isCooltime = false;
    }

    private void ImPossibleSummonState(int idx)
    {
        summonCards[idx].cardImage.color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        summonCards[idx].isCooltime = true;
    }
    private void UpdateMoneySlider()
    {
        if (gold != 0)
        {
            float ratio = ((float)gold / (float)maxGold);
            goldGauge.fillAmount = ratio;
        }
    }
    private void UpdateManaSlider()
    {
        if (mana != 0)
        {
            float ratio = ((float)mana / (float)maxGold);
            manaGauge.fillAmount = ratio;
        }
    }

    private void UpdateCardMpSliders()
    {
        for (int i = 0; i < summonCards.Count; i++)
        {
            int buttonIndex = new int();
            buttonIndex = i;

            if (summonCards[buttonIndex].button.transform.Find("Slider").gameObject.activeSelf == true)
            {
                if (summonCards[buttonIndex].heroInstance != null)
                {
                    UpdateCardMpSlider(buttonIndex);
                }
            }
        }
    }

    private void UpdateCardMpSlider(int idx)
    {
        Slider slider
            = summonCards[idx].button.transform.Find("Slider").gameObject.GetComponent<Slider>();
        float mp = summonCards[idx].status.stat.Mp;
        SetSliderValue(slider, mp);
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.value = value / 100.0f;
        }
    }

    private void KeyBoardClickUpChecker()
    {
        if (Input.anyKeyDown)
        {
            foreach (var keypad in keypadClick)
            {
                if (Input.GetKeyDown(keypad.Key))
                {
                    keypad.Value();
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            Game.Instance.dataManager.LunaPlayer.GetComponent<LunaStateMachine>().NMove();
        }
    }

    public void OnClickSummon(int buttonIndex)
    {
        if (summonCards[buttonIndex].unitPrefab == null)
            return;

        if (Game.Instance.dataManager.doNotSummonMode == true)
            return;

        if (summonCards[buttonIndex].status.objectData.UnitType == eUnitType.Hero)
        {
            GameObject[] tmpUnit = GameObject.FindGameObjectsWithTag("Unit");
            foreach (GameObject unit in tmpUnit)
            {
                if (unit.GetComponent<Status>().UnitName
                    == summonCards[buttonIndex].unitPrefab.GetComponent<Status>().objectData.UnitName)
                {
                    return;
                }
            }
            
        }

        bool isCreate
            = PurchaseUnit(Defined.AllyTowerPosition, Quaternion.identity, buttonIndex);
        


        bool isSummon
            = summonCards[buttonIndex].status.unitState.IsPossibleSummonState;
        if (isCreate && isSummon)
        {
            StartCoolTime(buttonIndex); 
        }
    }

    private int unitCount = 0;
    bool PurchaseUnit(Vector3 position, Quaternion rotation, int unitIndex)
    {
        if (summonCards[unitIndex].unitPrefab != null &&
            summonCards.Count > unitIndex)
        {
            int unitPrice = summonCards[unitIndex].status.objectData.Price;
            if (gold < unitPrice)
            {
                Debug.Log("NO MONEY!!");
                return false;
            }
            gold -= unitPrice;
            unitCount++;
            Vector3 createPos = Defined.AllyTowerPosition;
            createPos.z = 0.00001f * unitCount;
            CreateUnit(unitIndex, createPos, rotation);
        }

        return true;
    }

    void CreateUnit(int unitIndex, Vector3 pos, Quaternion qRot)
    {
        GameObject tmpUnit
                = PoolManager.Instance.spawnObject(summonCards[unitIndex].unitPrefab, pos, qRot);
        tmpUnit.GetComponent<Status>().ResetRespwanData();
        tmpUnit.GetComponent<UnitAI>().ResetAnimation();

        if (tmpUnit.GetComponent<Status>().objectData.UnitType == eUnitType.Hero)
        {
            summonCards[unitIndex].heroInstance = tmpUnit;
            summonCards[unitIndex].status = tmpUnit.GetComponent<Status>();

            GameObject uiProgressBar
                = LunaUtility.FindWithTagGameObject("UIProgressBar");

            UIProgressBar comp = uiProgressBar.GetComponent<UIProgressBar>();
            comp.PushProfile(tmpUnit);
        }
    }

    public void StartCoolTime(int index)
    {
        summonCards[index].leftTime = summonCards[index].coolTime;

        if (summonCards[index].slotEffect)
        {
            summonCards[index].slotEffect.SetActive(true);
        }

        if (summonCards[index].button)
            summonCards[index].button.enabled = false; // 버튼 기능을 해지함.
    }


    IEnumerator UpdateGoldAndMana()
    {
        float goldTime = 0.0f;
        float manaTime = 0.0f;

        while (true)
        {
            //InfiniteLoopDetector.Run();
            //GOLD
            float increaseGoldRatio
                = (Defined.AddGoldSpeedValue * LunaDataTable.Instance.playerData.GoldSpeedLevel)
                + (Defined.AddGoldSpeedValue * Defined.SkillTrigger_Goldy);

            increaseGoldRatio += 1.0f;

            goldTime += ( Time.deltaTime * 2.0f  * increaseGoldRatio);
            manaTime += (Time.deltaTime * 2.0f);
            if (goldTime > 1.0f)
            {
                gold++;
                goldText.text = gold.ToString();
                goldTime = 0.0f;
            }

            //MANA
            
            if (manaTime > 1.0f)
            {
                mana++;
                manaText.text = mana.ToString();
                manaTime = 0.0f;
            }

            if (gold >= 100)
            {
                gold = 100;
                goldText.text = gold.ToString();
            }

            if (mana >= 100)
            {
                mana = 100;
                manaText.text = mana.ToString();
            }

            yield return null;
        }
    }

    private void PlayerRespawnCheck()
    {
        if (Game.Instance.dataManager.LunaPlayer == null)
        {
            return;
        }

        bool IsDeath
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().unitState.IsDeath;

        if (IsDeath == true)
        {
            StartCoroutine(SpawnPlayer());
            Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().unitState.IsDeath = false;
        }
    }

    IEnumerator SpawnPlayer()
    {
        float basicSpawnTime
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().stat.SpawnDelayTime;
        float minusRatio = Defined.lunaReviveTimeValue * LunaDataTable.Instance.playerData.LunaReviveTimeLevel;        float ratio 
            = 1.0f - minusRatio;
        float spawnTime = basicSpawnTime * ratio;

        string playerGravePath = "Prefabs/06Effects/Grave_tmp";
        GameObject playerGrave_tmp = Resources.Load<GameObject>(playerGravePath);
        GameObject playerGrave = Instantiate(playerGrave_tmp, Game.Instance.dataManager.LunaPlayer.transform.position, Quaternion.identity);

        //부활 이펙트 생성Prefabs/01Heroes/
        string effectPath = "Prefabs/06Effects/Skill/ResurrectPlayer";
        GameObject effectPrefab = Resources.Load<GameObject>(effectPath);

        //OptionalDefine.startPos;
        GameObject effect
            = Defined.EffectCreate(effectPrefab, new Vector3(-8.0f, 0.6f));
        //effect.GetComponent<Transform>().position += new Vector3(0.0f, 0.5f, 0.0f);


        effectPath = "Prefabs/06Effects/RebirthStoneEffect";

        GameObject rebirthPrefab = Resources.Load<GameObject>(effectPath);

        GameObject rebirthEffect
            = Defined.EffectCreate(rebirthPrefab, new Vector3(-8.0f, 0.6f));
        //rebirthEffect.GetComponent<Transform>().position += new Vector3(0.0f, 0.5f, 0.0f);

        yield return new WaitForSeconds(spawnTime);

        DestroyImmediate(playerGrave);

        rebirthEffect.GetComponent<NormalSkeletonObjControl>().SetAnimationState("rebirth", false);
        rebirthEffect.GetComponent<NormalSkeletonObjControl>().DestroyThisObject(2.0f);
        GameObject playerPrefab
            = LunaDataTable.Instance.playerData.CurPlayer();

        PoolManager.SpawnObject(playerPrefab
                                , Defined.AllyTowerPosition
                                , Quaternion.identity);

        Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().Ready();

        Game.Destroy(effect);
    }



}


using System;
using System.Collections.Generic;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Battle HUD. The Procedure-owned round retains currency, cooldowns and native summon requests.</summary>
public class UIPlayer : UILuna
{
    public sealed class CardData
    {
        public Button button;
        public Image cardImage;
        public GameObject slotEffect;
        public Text levelText;
        public Vector3 AuthoredPosition;
        public LunaBattleRoster.Slot Slot;
        public GameObject unitPrefab => Slot.Prefab;
        public ObjectData Definition => Slot.Definition;
        public int EntityId => Slot.EntityId;
        public bool Pending => Slot.Pending;
        public GameObject heroInstance => Slot.LiveUnit != null ? Slot.LiveUnit.gameObject : null;
        public float leftTime => Slot.Remaining;
        public float coolTime => Slot.Cooldown;
    }
    private LunaGame round;
    private LunaBattleRoster roster;
    private readonly List<CardData> cards = new List<CardData>();
    public List<CardData> SummonCards => cards;
    public int Money { get => roster?.Gold ?? 0; set { if (roster != null) roster.Gold = value; } }
    public int Mana { get => roster?.Mana ?? 0; set { if (roster != null) roster.Mana = value; } }
    private Image goldGauge, manaGauge;
    private Text goldText, manaText, timeText;
    private bool keyboardMovementActive;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        round = LunaScenes.CurrentGame ?? throw new InvalidOperationException("Battle HUD requires its native round.");
        roster = round.Roster ?? throw new InvalidOperationException("Battle roster is not initialized.");
        goldText = transform.Find("GoldText").GetComponent<Text>();
        manaText = transform.Find("ManaText").GetComponent<Text>();
        timeText = transform.Find("TimeText").GetComponent<Text>();
        timeText.resizeTextForBestFit = true;
        timeText.resizeTextMinSize = 14;
        timeText.resizeTextMaxSize = 24;
        goldGauge = transform.Find("GoldGaugeImage").GetComponent<Image>();
        manaGauge = transform.Find("ManaGaugeImage").GetComponent<Image>();
        transform.Find("GoldMaxText").GetComponent<Text>().text = "/" + roster.MaxGold;
        transform.Find("ManaMaxText").GetComponent<Text>().text = "/" + roster.MaxMana;
        transform.Find("NameBackImage/LevelText").GetComponent<Text>().text = "LV. <color=#D1A606>" + LunaDataTable.Instance.playerData.LunaLevel + "</color>";
        cards.Clear();
        var buttons = GetComponentsInChildren<Button>(true);
        for (int index = 0; index < 6; index++)
        {
            int slotIndex = index;
            var button = Array.Find(buttons, value => value.name == "Button_" + index);
            if (button == null) throw new InvalidOperationException("Battle card button is missing: " + index);
            var card = new CardData { button = button, cardImage = button.GetComponent<Image>(), Slot = roster.GetSlot(index), AuthoredPosition = button.transform.localPosition };
            card.slotEffect = button.transform.Find("SlotEffect")?.gameObject;
            card.levelText = button.transform.Find("LevelText")?.GetComponent<Text>();
            button.onClick.AddListener(() => OnClickSummon(slotIndex));
            var portrait = button.transform.Find("Image");
            var stars = button.transform.Find("Stars");
            var slider = button.transform.Find("Slider");
            bool hasUnit = card.Definition != null;
            portrait.gameObject.SetActive(hasUnit && card.Definition.profilePicture != null);
            if (stars != null) stars.gameObject.SetActive(hasUnit);
            if (slider != null) slider.gameObject.SetActive(hasUnit && card.Definition.UnitType == eUnitType.Hero);
            if (hasUnit)
            {
                card.cardImage.sprite = card.Definition.cardPicture;
                portrait.GetComponent<Image>().sprite = card.Definition.profilePicture;
                button.transform.Find("Summons/Text").GetComponent<Text>().text = card.Definition.Price.ToString();
                if (card.levelText != null) card.levelText.text = card.Definition.Level.ToString();
            }
            else
            {
                button.transform.Find("Summons/Text").GetComponent<Text>().text = string.Empty;
                if (card.levelText != null) card.levelText.text = string.Empty;
            }
            cards.Add(card);
        }
        RefreshDisplay();
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        RefreshDisplay();
        if (round == null || !round.IsActive || round.isStageStop) { StopMovement(); return; }
        for (int i = 0; i < 6; i++) if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) OnClickSummon(i);
        if (Input.GetKeyDown(KeyCode.Escape)) OnClickEscape();
        ApplyKeyboardMovement(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow), Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
    }
    private void RefreshDisplay()
    {
        if (roster == null) return;
        goldText.text = roster.Gold.ToString(); manaText.text = roster.Mana.ToString();
        goldGauge.fillAmount = (float)roster.Gold / roster.MaxGold;
        manaGauge.fillAmount = (float)roster.Mana / roster.MaxMana;
        float timeLeft;
        if (round is BossDungeon boss)
        {
            timeLeft = boss.dungeonCount;
            timeText.text = Mathf.CeilToInt(timeLeft) + "초 · " + Mathf.CeilToInt(boss.BossHealth * 100f) + "%";
            if (boss.Boss is Rugani rugani && rugani.StrikeMarker.HasValue)
                timeText.text += "\n낙뢰!";
        }
        else if (round is EndlessDungeon endless)
        {
            timeLeft = endless.FloorTimeRemaining;
            timeText.text = (endless.dungeonLevel + 1) + "층 · " + Mathf.CeilToInt(timeLeft) + "초";
        }
        else
        {
            timeLeft = Mathf.Max(0f, round.TimeLimit - round.StageCount);
            timeText.text = TimeSpan.FromSeconds(Mathf.Ceil(timeLeft)).ToString(@"mm\:ss");
            if (round is BasicDungeon basic && basic.SecondsUntilWave <= 4f)
                timeText.text += "\n증원 " + Mathf.CeilToInt(basic.SecondsUntilWave) + "초";
        }
        timeText.color = timeLeft <= 10f ? new Color(1f, 0.38f, 0.34f) : Color.white;
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var unit = card.Slot.LiveUnit;
            bool liveHero = card.Definition != null && card.Definition.UnitType == eUnitType.Hero && unit != null;
            bool affordable = card.Definition != null && roster.Gold >= card.Definition.Price;
            bool cooling = card.Pending || card.leftTime > 0f;
            card.button.enabled = true;
            card.button.interactable = roster.CanSummon(i);
            card.cardImage.color = affordable && !cooling && !liveHero ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            card.button.transform.localPosition = card.AuthoredPosition + (!affordable && !liveHero ? new Vector3(0f, -43f, 0f) : Vector3.zero);
            if (card.slotEffect != null) card.slotEffect.SetActive(cooling);
            var slider = card.button.transform.Find("Slider")?.GetComponent<Slider>();
            if (slider != null) slider.value = unit != null ? Mathf.Clamp01(unit.stat.Mp / 100f) : 0f;
        }
    }
    public void ButtonDisableChecker() => RefreshDisplay();
    public void OnClickSummon(int index) { if (roster != null) roster.TrySummon(index); }
    public void ApplyKeyboardMovement(bool leftHeld, bool rightHeld)
    {
        bool held = leftHeld || rightHeld;
        if (!held && !keyboardMovementActive) return;
        keyboardMovementActive = held;
        var player = UnitLogic.From(LunaSession.Current?.LunaPlayer) as PlayerLogic;
        if (player == null || !player.Available || player.IsHiding) return;
        if (leftHeld == rightHeld) player.NMove();
        else if (leftHeld) player.LMove();
        else player.RMove();
    }
    private void StopMovement()
    {
        keyboardMovementActive = false;
        (UnitLogic.From(LunaSession.Current?.LunaPlayer) as PlayerLogic)?.NMove();
    }
    protected override void OnPause() { StopMovement(); base.OnPause(); }
    protected override void OnClose(bool isShutdown, object userData)
    {
        StopMovement(); cards.Clear(); roster = null; round = null;
        base.OnClose(isShutdown, userData);
    }
    public void OnClickEscape()
    {
        StopMovement();
        LunaScenes.FadeIn(1.5f, () => LunaScenes.Load("Lobby"));
    }
}

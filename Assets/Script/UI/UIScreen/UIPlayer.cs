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
        public Text StatusText;
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
    private Text guidanceText, warningText;
    private Image warningArea;
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
        guidanceText = CreateBattleText(transform, "BattleGuidance", new Vector2(800f, 48f),
            new Vector2(0.5f, 1f), new Vector2(0f, -137f), 18);
        warningArea = new GameObject("AttackWarning", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        warningArea.transform.SetParent(transform, false);
        warningArea.color = new Color(1f, 0.25f, 0.08f, 0.65f);
        warningArea.raycastTarget = false;
        warningText = CreateBattleText(warningArea.transform, "WarningText", new Vector2(200f, 30f),
            new Vector2(0.5f, 1f), new Vector2(0f, 22f), 19);
        warningArea.gameObject.SetActive(false);
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
            card.StatusText = CreateBattleText(button.transform, "SummonStatus", new Vector2(118f, 27f),
                new Vector2(0.5f, 1f), new Vector2(0f, 16f), 16);
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
        guidanceText.text = round.isStageStop ? string.Empty : round.GuidanceText;
        RefreshAttackWarning();
        float timeLeft;
        if (round is BossDungeon boss)
        {
            timeLeft = boss.dungeonCount;
            timeText.text = Mathf.CeilToInt(timeLeft) + "초 · " + Mathf.CeilToInt(boss.BossHealth * 100f) + "%";
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
            card.button.transform.localPosition = card.AuthoredPosition;
            if (card.slotEffect != null) card.slotEffect.SetActive(cooling);
            card.StatusText.text = SummonStatus(i);
            card.StatusText.color = liveHero ? new Color(0.65f, 1f, 0.65f) : Color.white;
            if (liveHero && unit.stat.Hp <= unit.HealthBarHpPoint * 0.3f)
                card.StatusText.color = new Color(1f, 0.45f, 0.35f);
            var slider = card.button.transform.Find("Slider")?.GetComponent<Slider>();
            if (slider != null) slider.value = unit != null ? Mathf.Clamp01(unit.stat.Mp / 100f) : 0f;
        }
    }
    public string SummonStatus(int index)
    {
        var slot = roster.GetSlot(index);
        if (slot.Definition == null) return "빈 슬롯";
        if (slot.Pending) return "합류 중";
        var unit = slot.LiveUnit;
        if (slot.Definition.UnitType == eUnitType.Hero && unit != null)
            return unit.stat.Hp <= 0f ? "전사" : "출전 · HP " +
                Mathf.CeilToInt(Mathf.Clamp01(unit.stat.Hp / Mathf.Max(1f, unit.HealthBarHpPoint)) * 100f) + "%";
        if (round.isStageStop) return "전투 대기";
        if (round is EndlessDungeon && LunaSession.Current.doNotSummonMode) return "자동 편성";
        if (slot.Remaining > 0f) return "대기 " + Mathf.CeilToInt(slot.Remaining / 5f) + "초";
        int missing = slot.Definition.Price - roster.Gold;
        return missing > 0 ? "골드 " + missing + " 부족" : "소환 가능";
    }

    private Text CreateBattleText(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 position, int fontSize)
    {
        var label = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow)).GetComponent<Text>();
        var rect = label.rectTransform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        label.font = timeText.font;
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
        label.color = Color.white;
        return label;
    }

    private void RefreshAttackWarning()
    {
        EnemySkillLogic threat = null;
        foreach (var unit in UnitLogic.ActiveUnits)
            if (unit is EnemySkillLogic enemy && enemy.Data != null && enemy.Data.RoundId == round.RoundId &&
                enemy.UnitSide == eUnitSide.Enemy && enemy.WarningPosition.HasValue)
            { threat = enemy; break; }
        var camera = LunaBootstrap.GameplayCamera;
        bool visible = !round.isStageStop && threat != null && camera != null;
        warningArea.gameObject.SetActive(visible);
        if (!visible) return;
        Vector3 point = threat.WarningPosition.Value;
        Vector3 left = camera.WorldToViewportPoint(point - Vector3.right * threat.WarningRadius);
        Vector3 right = camera.WorldToViewportPoint(point + Vector3.right * threat.WarningRadius);
        if (right.x < 0f || left.x > 1f || left.z <= 0f)
        { warningArea.gameObject.SetActive(false); return; }
        var root = (RectTransform)transform;
        float min = Mathf.Clamp01(left.x), max = Mathf.Clamp01(right.x);
        var rect = warningArea.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(Mathf.Max(8f, (max - min) * root.rect.width), 24f);
        rect.anchoredPosition = new Vector2(((min + max) * 0.5f - 0.5f) * root.rect.width,
            (left.y - 0.5f) * root.rect.height);
        float labelX = Mathf.Clamp(rect.anchoredPosition.x, -root.rect.width * 0.5f + 105f, root.rect.width * 0.5f - 105f);
        warningText.rectTransform.anchoredPosition = new Vector2(labelX - rect.anchoredPosition.x, 22f);
        warningText.text = threat.WarningName + " " + threat.WarningSeconds.ToString("F1") + "초";
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

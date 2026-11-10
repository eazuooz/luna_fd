using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UIGameShop : UILuna
{
    private Button singleButton, bundleButton;
    private Text status;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        var board = transform.Find("Board");
        foreach (string tab in new[] { "Gold_Tab", "StarStone_Tab", "Ruby_Tab", "RainbowStone_Tab" })
            board.Find(tab)?.gameObject.SetActive(false);
        var view = board.Find("Summon_Tab/Summon_ScrollView");
        view.gameObject.SetActive(true);
        var cards = view.Find("Summon_Scroll View_Viewport/Summon_Content");
        cards.Find("ViewAD").gameObject.SetActive(false);
        cards.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        var layout = cards.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth = false;
        layout.spacing = 100f;
        singleButton = cards.Find("BuyOnce/SlimeOnceButton").GetComponent<Button>();
        bundleButton = cards.Find("BuyTens/SlimeOnceButton").GetComponent<Button>();
        SetOfferCaption(singleButton, "한 장씩 성장", "영웅과 병사 강화 카드 1장");
        SetOfferCaption(bundleButton, "덱을 키우는 묶음", "첫 5장 무료 · 이후 영웅 보장 10장");
        var label = new GameObject("ShopStatus", typeof(RectTransform), typeof(Text));
        var rect = (RectTransform)label.transform;
        rect.SetParent(board, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(38f, -16f);
        rect.sizeDelta = new Vector2(820f, 70f);
        status = label.GetComponent<Text>();
        status.font = singleButton.transform.Find("Text").GetComponent<Text>().font;
        status.fontSize = 23;
        status.alignment = TextAnchor.UpperCenter;
        status.raycastTarget = false;
        RefreshShop();
    }

    private static void SetOfferCaption(Button button, string title, string detail)
    {
        button.transform.Find("Image").gameObject.SetActive(false);
        var price = button.transform.Find("KeyCount").GetComponent<Text>();
        price.rectTransform.anchoredPosition = new Vector2(0f, -15f);
        price.alignment = TextAnchor.MiddleCenter;
        var panel = new GameObject("OfferCaption", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)panel.transform;
        rect.SetParent(button.transform.parent, false);
        rect.anchorMin = new Vector2(0.035f, 0.025f);
        rect.anchorMax = new Vector2(0.965f, 0.285f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var background = panel.GetComponent<Image>();
        background.color = new Color32(62, 53, 41, 255);
        background.raycastTarget = false;
        var caption = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var label = caption.GetComponent<Text>();
        label.rectTransform.SetParent(rect, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(8f, 0f);
        label.rectTransform.offsetMax = new Vector2(-8f, 0f);
        label.font = price.font;
        label.fontSize = 16;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
        label.text = "<size=23>" + title + "</size>\n" + detail;
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (status != null) RefreshShop();
    }

    public void OnClickSummon() => Draw(false);
    public void OnClickSummons() => Draw(true);

    private void Draw(bool bundle)
    {
        if (!LunaUI.IsTop(type) || LunaUI.IsOpenOrLoading(UI_TYPE.LOTTERY)) return;
        if (!LunaEconomy.TryDraw(bundle, out string message))
        {
            RefreshShop(message);
            return;
        }
        RefreshShop();
        LunaUI.Open(UI_TYPE.LOTTERY);
    }

    private void RefreshShop(string message = "")
    {
        var profile = LunaDataTable.Instance.playerData;
        singleButton.transform.Find("Text").GetComponent<Text>().text = "1장 소환";
        singleButton.transform.Find("KeyCount").GetComponent<Text>().text = LunaEconomy.SingleDrawGold + " G";
        bundleButton.transform.Find("Text").GetComponent<Text>().text = profile.IsBegining ? "첫 5장" : "10장 소환";
        bundleButton.transform.Find("KeyCount").GetComponent<Text>().text = profile.IsBegining ? "무료" : LunaEconomy.BundleDrawGold + " G";
        singleButton.interactable = profile.Gold >= LunaEconomy.SingleDrawGold;
        bundleButton.interactable = profile.IsBegining || profile.Gold >= LunaEconomy.BundleDrawGold;
        status.text = "골드 " + profile.Gold.ToString("N0") + " · 별석 " + profile.StarStone + "\n" +
            (string.IsNullOrEmpty(message) ? $"영웅 {LunaEconomy.HeroChance}% · 덱 우선 {LunaEconomy.DeckFocusChance}% · 10장 소환 영웅 1장 보장" : message);
    }

    // These names are bound to the old prefab. The prototype buttons no longer grant currency.
    public void OnClickBuyStarStone() => RefreshShop("별석은 첫 클리어와 무한던전 기록에서 얻습니다.");
    public void EquipLunaSlimeSkin()
    {
        LunaSession.Current.EquipDesignNumber = 3;
        LunaUI.Open(UI_TYPE.EQUIPDESIGN);
    }
    public void OnReleaseButton() => LunaUI.Close(type);
}

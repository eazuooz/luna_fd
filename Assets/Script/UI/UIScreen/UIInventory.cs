using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : UILuna
{
    private readonly Dictionary<string, GameObject> views = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, ItemData.ItemType> types = new Dictionary<string, ItemData.ItemType>
    {
        { "MaterialTab", ItemData.ItemType.MATERIAL }, { "RelicsTab", ItemData.ItemType.RELICS }, { "DesignTab", ItemData.ItemType.DESIGN },
        { "ChestTab", ItemData.ItemType.CHEST }, { "KeyTab", ItemData.ItemType.KEY }
    };
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData); views.Clear();
        foreach (var label in GetComponentsInChildren<Text>(true))
            if (label.text.Contains("수집한 모든") || label.text.Contains("아이템을 확인")) label.gameObject.SetActive(false);
        var board = transform.Find("BackGround");
        var plate = LunaUIPresentation.Panel(board, "OwnedItemsPanel", new Vector2(-224f, -52f), new Vector2(670f, 490f), LunaUIPresentation.Ink);
        plate.transform.SetAsFirstSibling();
        LunaUIPresentation.Label(plate.transform, "Title", "보유 아이템", new Vector2(0f, 229f), new Vector2(630f, 22f), 18);
        foreach (var entry in types)
        {
            var tab = transform.Find("BackGround/" + entry.Key);
            var scroll = tab.GetComponentInChildren<ScrollRect>(true);
            views[entry.Key] = scroll.gameObject;
            foreach (Transform child in scroll.content) child.gameObject.SetActive(false);
            string key = entry.Key; tab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(key));
            var grid = scroll.content.GetComponent<GridLayoutGroup>();
            if (grid != null) { grid.cellSize = new Vector2(94f, 104f); grid.spacing = new Vector2(8f, 10f); }
            foreach (var item in LunaDataTable.Instance.itemTable.Values.Where(item => item.type == entry.Value && (item.itemCount > 0 || item.isActive)).OrderBy(item => item.itemNumber))
                AddItem(scroll.content, item);
        }
        LunaUIPresentation.Label(board, "InventoryTitle", "보유 아이템 · 수량 / 사용처", new Vector2(270f, 248f), new Vector2(560f, 36f), 20);
        OnTabClick("MaterialTab");
    }
    private void AddItem(Transform parent, ItemData item)
    {
        var rect = LunaUIPresentation.Rect(parent, "OwnedItem_" + item.itemNumber, Vector2.zero, new Vector2(94f, 104f));
        var image = rect.gameObject.AddComponent<Image>(); image.sprite = LunaUIViews.CreateSprite(item.IconImage); image.preserveAspect = true;
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        button.onClick.AddListener(() => ShowItem(item.itemNumber));
        var backdrop = LunaUIPresentation.Panel(rect, "QuantityBackground", new Vector2(0f, -36f), new Vector2(94f, 28f), LunaUIPresentation.Ink, false);
        LunaUIPresentation.Label(backdrop.transform, "Count", "× " + item.itemCount, Vector2.zero, new Vector2(88f, 26f), 18, TextAnchor.MiddleCenter);
    }
    public void OnTabClick(string tabName)
    {
        if (!views.ContainsKey(tabName)) return;
        LunaUI.Close(UI_TYPE.ITEMINFO);
        foreach (var pair in views)
        {
            pair.Value.SetActive(pair.Key == tabName);
            transform.Find("BackGround/" + pair.Key).GetComponent<Image>().color = pair.Key == tabName ? LunaUIPresentation.Gold : Color.white;
        }
        var items = LunaDataTable.Instance.itemTable.Values.Where(item => item.type == types[tabName] && (item.itemCount > 0 || item.isActive)).OrderBy(item => item.itemNumber).ToArray();
        var previous = transform.Find("InventoryEmptyState"); if (previous != null) { previous.gameObject.SetActive(false); Destroy(previous.gameObject); }
        if (items.Length > 0) ShowItem(items[0].itemNumber);
        else LunaUIPresentation.Label(transform, "InventoryEmptyState", "아직 보유한 아이템이 없습니다.\n스테이지의 보상 목록에서 획득처를 확인하세요.", new Vector2(190f, -40f), new Vector2(580f, 170f), 24, TextAnchor.MiddleCenter);
    }
    private static void ShowItem(int id)
    {
        LunaUI.Close(UI_TYPE.ITEMINFO); LunaSession.Current.pickItemNumber = id; LunaUI.Open(UI_TYPE.ITEMINFO);
    }
    public void PushPopupUI() => LunaUI.Open(type);
    public void ReleaseThisUI() { LunaUI.Close(UI_TYPE.ITEMINFO); LunaUI.Close(type); }
}

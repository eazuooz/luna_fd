using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInfo : UILuna
{
    private ItemData item;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        if (!LunaDataTable.Instance.itemTable.TryGetValue(LunaSession.Current.pickItemNumber, out item)) { LunaUI.Close(type); return; }
        var board = transform.Find("BackGround");
        board.Find("Profile").GetComponent<Image>().sprite = LunaUIViews.CreateSprite(item.IconImage);
        board.Find("Profile/NameText").GetComponent<Text>().text = item.itemName;
        var description = board.Find("Profile/MainText").GetComponent<Text>();
        description.fontSize = 20; description.lineSpacing = 1f;
        string hint;
        if (item.itemNumber == 1001) hint = "스킬 상세에서 강화 재료로 사용합니다.";
        else if (item.itemNumber == LunaEquipmentEnhancements.StoneId) hint = "장비 상세에서 해골 도끼병의 갑옷을 강화할 때 사용합니다.";
        else if (item.type == ItemData.ItemType.DESIGN) hint = "장비 정보에서 파츠와 효과를 확인하세요.";
        else if (item.type == ItemData.ItemType.MATERIAL) hint = "현재 보관 가능한 수집 재료입니다.\n이 재료의 소비 기능은 준비 중입니다.";
        else hint = "스테이지 보상에서 획득할 수 있습니다.";
        description.text = "보유 수량  " + item.itemCount + "개\n\n" + item.itemDiscription + "\n\n" + hint;
        var button = board.Find("Button").GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(OnClickStartGame);
        button.transform.Find("Image").gameObject.SetActive(false);
        string caption = item.itemNumber == 1001 ? "스킬 강화 열기" : "스테이지 보상 보기";
        if (item.itemNumber == LunaEquipmentEnhancements.StoneId) caption = "장비 강화 열기";
        else if (item.itemNumber != 1001 && (item.type == ItemData.ItemType.DESIGN || item.type == ItemData.ItemType.MATERIAL)) caption = "장비 정보 보기";
        LunaUIPresentation.Caption(button, caption);
        button.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
    public void OnClickStartGame()
    {
        if (item == null) return;
        LunaUI.Close(type); LunaUI.Close(UI_TYPE.INVENTORY);
        if (item.type == ItemData.ItemType.MATERIAL || item.type == ItemData.ItemType.DESIGN)
        {
            LunaSession.Current.PreferredUpgradeTab = item.itemNumber == 1001 ? "Skill_ScrollView" : "Equip_ScrollView";
            LunaUI.Open(UI_TYPE.UPGRADE);
        }
        else LunaUI.Open(UI_TYPE.SELECTSTAGE);
    }
    public void PushPopupUI() => LunaUI.Open(type);
    public void ReleaseThisUI() => LunaUI.Close(type);
}

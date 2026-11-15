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
        description.text = "보유 수량  " + item.itemCount + "개\n\n" + item.itemDiscription + "\n\n" +
            (item.type == ItemData.ItemType.DESIGN ? "장비 정보에서 파츠와 효과를 확인하세요." : item.itemNumber == 1001 ? "스킬 상세에서 실제 강화 재료로 사용합니다." : item.type == ItemData.ItemType.MATERIAL ? "현재 보관 가능한 수집 재료입니다.\n이 재료의 소비 기능은 준비 중입니다." : "스테이지 보상에서 획득할 수 있습니다.");
        var button = board.Find("Button").GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(OnClickStartGame);
        button.transform.Find("Image").gameObject.SetActive(false);
        LunaUIPresentation.Caption(button, item.itemNumber == 1001 ? "스킬 강화 열기" : item.type == ItemData.ItemType.DESIGN || item.type == ItemData.ItemType.MATERIAL ? "장비 정보 보기" : "스테이지 보상 보기");
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

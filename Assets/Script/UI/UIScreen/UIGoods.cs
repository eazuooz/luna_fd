using UnityEngine;
using Luna.Gameplay;
using UnityEngine.UI;

public class UIGoods : UILuna
{
    private Text goldText;
    private Text starStoneText;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        transform.Find("Ruby").gameObject.SetActive(false);
        var stones = (RectTransform)transform.Find("StarStone");
        stones.Find("AddStarStone").gameObject.SetActive(false);
        stones.anchoredPosition = new Vector2(476f, stones.anchoredPosition.y);
        var gold = (RectTransform)transform.Find("Gold");
        gold.anchoredPosition = new Vector2(268f, gold.anchoredPosition.y);
        goldText = gold.Find("GoldText").GetComponent<Text>();
        starStoneText = stones.Find("StarStoneText").GetComponent<Text>();
        RefreshWallet();
    }

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        RefreshWallet();
    }

    private void RefreshWallet()
    {
        var profile = LunaDataTable.Instance.playerData;
        goldText.text = profile.Gold.ToString("N0");
        starStoneText.text = profile.StarStone.ToString("N0");
    }

    public void OnClickAddButton() => LunaUI.Open(UI_TYPE.GAMESHOP);
}

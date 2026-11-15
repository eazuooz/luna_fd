using System;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    private bool isUnlocked;
    public void Select(bool selected)
    {
        var check = transform.Find("CheckImage"); if (check != null) check.gameObject.SetActive(selected);
        var outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
        outline.effectColor = LunaUIPresentation.Gold; outline.effectDistance = new Vector2(3f, -3f); outline.enabled = selected;
        var image = GetComponent<Image>(); if (image != null) image.color = selected ? new Color(1f, 0.94f, 0.75f) : isUnlocked ? Color.white : new Color(0.65f, 0.68f, 0.69f);
    }
    public void Bind(LunaStageRow row, bool unlocked, bool selected, Action clicked)
    {
        isUnlocked = unlocked;
        transform.Find("CheckImage").gameObject.SetActive(selected);
        transform.Find("BossImage").gameObject.SetActive(row.IsBossStage);
        transform.Find("DungeonInfos")?.gameObject.SetActive(false);
        var label = transform.Find("Text").GetComponent<Text>();
        label.rectTransform.offsetMin = new Vector2(45f, 0f);
        label.rectTransform.offsetMax = new Vector2(-55f, 0f);
        var check = (RectTransform)transform.Find("CheckImage");
        check.anchorMin = check.anchorMax = new Vector2(0f, 0.5f);
        check.anchoredPosition = new Vector2(22f, 0f);
        var boss = (RectTransform)transform.Find("BossImage");
        boss.anchorMin = boss.anchorMax = new Vector2(1f, 0.5f);
        boss.anchoredPosition = new Vector2(-24f, 0f);
        bool cleared = LunaDataTable.Instance.StageTable[row.Key].isClear;
        label.text = row.DisplayName + "\n" + (unlocked ? cleared ? "클리어 · 재도전 가능" : "도전 가능" : "잠김 · " + (row.Id - 1) + "층 클리어 후");
        label.fontSize = 19;
        label.lineSpacing = 1f;
        label.resizeTextMaxSize = label.fontSize;
        label.resizeTextMinSize = 15;
        label.resizeTextForBestFit = true;
        GetComponent<Button>().onClick.AddListener(() => clicked());
        Select(selected);
    }

    public void BindDungeon(LunaStageRow row, bool unlocked, Action clicked)
    {
        isUnlocked = unlocked;
        var button = gameObject.AddComponent<Button>();
        button.targetGraphic = GetComponent<Image>();
        button.onClick.AddListener(() => clicked());
        var start = transform.Find("StartButton").GetComponent<Button>();
        start.onClick = new Button.ButtonClickedEvent();
        start.onClick.AddListener(() => clicked());
        transform.Find("Name/Text").GetComponent<Text>().text = row.DisplayName;
        var subtitle = transform.Find("ExplaneText").GetComponent<Text>();
        subtitle.text = (row.Kind == LunaStageKind.Endless ? "무한 생존" : "보스 토벌") + (unlocked ? " · 입장 가능" : " · 잠김");
        var manual = transform.Find("Manual").GetComponent<Text>();
        manual.rectTransform.anchoredPosition = new Vector2(0f, -52f);
        manual.rectTransform.sizeDelta = new Vector2(315f, 160f);
        manual.fontSize = 19;
        manual.lineSpacing = 1f;
        manual.text = "권장 Lv. " + row.RecommendedLevel + "\n" + row.Tip + "\n" +
            (row.Kind == LunaStageKind.Endless ? "완료 층당 " + row.Gold + " G" : "첫 승리 " + (row.Gold + row.FirstClearGold) + " G");
        start.transform.Find("Text").GetComponent<Text>().text = unlocked ? "정보 확인 · 출전 준비" : "해금 조건 보기 · " + row.UnlockAfter + "층";
        foreach (var text in new[] { subtitle, transform.Find("Name/Text").GetComponent<Text>() })
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = 24;
            text.lineSpacing = 1f;
        }
        if (row.Kind == LunaStageKind.Boss && row.Key != "Dungeon01")
        {
            var data = LunaStageTables.RequireAsset<GameObject>(row.Boss).GetComponent<UnitView>().Data;
            var image = transform.Find("DungeonImage").GetComponent<Image>();
            image.sprite = data.illustPicture != null ? data.illustPicture : data.profilePicture;
            image.preserveAspect = true;
        }
        var art = transform.Find("DungeonImage").GetComponent<Image>();
        art.preserveAspect = true;
        var artRect = art.rectTransform;
        artRect.anchoredPosition = new Vector2(0f, 112f); artRect.sizeDelta = new Vector2(320f, 94f);
        var plate = LunaUIPresentation.Panel(transform, "ArtworkPlate", artRect.anchoredPosition, artRect.sizeDelta, LunaUIPresentation.Ink);
        plate.transform.SetSiblingIndex(art.transform.GetSiblingIndex());
    }
}

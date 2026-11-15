using System.Collections;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UISelectDungeon : UILuna
{
    private GameObject stageView, dungeonView;
    private LunaStageRow selected, selectedCampaign;
    private GameObject listPanel, detailPanel;
    private Text progress;
    private bool returningFromParty;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        selected = selectedCampaign = null; listPanel = detailPanel = null; progress = null;
        returningFromParty = false;
        var stages = transform.Find("Stage_Tab");
        var dungeons = transform.Find("Dungeon_Tab");
        stageView = stages.Find("Stage_ScrollView").gameObject;
        dungeonView = dungeons.Find("Dungeon_ScrollView").gameObject;
        var dungeonRect = (RectTransform)dungeonView.transform;
        dungeonRect.anchoredPosition = new Vector2(413f, -323f);
        dungeonRect.sizeDelta = new Vector2(1060f, 510f);
        dungeonView.GetComponent<ScrollRect>().horizontal = true;
        dungeonRect.Find("KeyInfo").gameObject.SetActive(false);
        var dungeonHint = dungeonRect.Find("ExplaneText").GetComponent<Text>();
        dungeonHint.rectTransform.anchoredPosition = new Vector2(120f, 290f);
        dungeonHint.rectTransform.sizeDelta = new Vector2(720f, 40f);
        dungeonHint.alignment = TextAnchor.MiddleRight;
        dungeonHint.text = "좌우로 넘겨 던전의 이미지와 설명을 확인하세요.";
        var navigation = LunaUIPresentation.Rect(transform, "DungeonNavigation", new Vector2(370f, -315f), new Vector2(330f, 34f));
        var dungeonScroll = dungeonView.GetComponent<ScrollRect>();
        LunaUIPresentation.Button(navigation, "Previous", "이전", new Vector2(-100f, 0f), new Vector2(100f, 32f), () => dungeonScroll.horizontalNormalizedPosition = Mathf.Clamp01(dungeonScroll.horizontalNormalizedPosition - 0.5f));
        LunaUIPresentation.Button(navigation, "Next", "다음", new Vector2(100f, 0f), new Vector2(100f, 32f), () => dungeonScroll.horizontalNormalizedPosition = Mathf.Clamp01(dungeonScroll.horizontalNormalizedPosition + 0.5f));
        LunaUIPresentation.Label(navigation, "Count", "총 5곳", Vector2.zero, new Vector2(95f, 32f), 17, TextAnchor.MiddleCenter);
        stages.GetComponent<Button>().onClick.AddListener(() => SetTab(false));
        dungeons.GetComponent<Button>().onClick.AddListener(() => SetTab(true));
        SetTab(false);

        var profile = LunaDataTable.Instance.playerData;
        selected = LunaStageTables.Get(profile.SaveStageName);
        listPanel = LunaUIPresentation.Panel(transform.Find("BG"), "StageListPanel", new Vector2(-352f, -47f), new Vector2(426f, 536f), LunaUIPresentation.Ink).gameObject;
        detailPanel = LunaUIPresentation.Panel(transform.Find("BG"), "StageDetailPanel", new Vector2(210f, -47f), new Vector2(703f, 536f), LunaUIPresentation.Paper).gameObject;
        LunaUIPresentation.Label(listPanel.transform, "ListTitle", "스테이지 목록", new Vector2(0f, 241f), new Vector2(390f, 30f), 23);
        progress = LunaUIPresentation.Label(transform, "MapProgress", "", new Vector2(100f, 260f), new Vector2(620f, 50f), 22);
        LunaUIPresentation.Label(detailPanel.transform, "DetailTitle", "스테이지 정보", new Vector2(0f, 241f), new Vector2(660f, 30f), 23);
        transform.Find("LineImage").gameObject.SetActive(false);
        stageView.transform.Find("Text").gameObject.SetActive(false);
        ((RectTransform)stageView.transform).anchoredPosition = new Vector2(158f, -328f);
        ((RectTransform)stageView.transform).sizeDelta = new Vector2(400f, 460f);
        LunaUIPresentation.Button(transform, "JumpToProgress", "진행 중인 층으로", new Vector2(-336f, -315f), new Vector2(300f, 27f), () =>
        {
            var row = LunaStageTables.Get(profile.SaveStageName);
            Preview(row); StartCoroutine(ScrollToFloor(LunaSession.CampaignFloor(row.Key)));
        });
        var stageRoot = stageView.transform.Find("Stage_Viewport/Stage_Content");
        var dungeonRoot = dungeonView.transform.Find("Dungeon_Scroll View_Viewport/Dungeon_Content");
        var stageRect = (RectTransform)stageRoot;
        stageRect.anchorMin = new Vector2(0f, 1f);
        stageRect.anchorMax = Vector2.one;
        stageRect.pivot = new Vector2(0.5f, 1f);
        stageRect.anchoredPosition = stageRect.sizeDelta = Vector2.zero;
        stageRoot.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        stageRoot.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
        var viewport = (RectTransform)dungeonRoot.parent;
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        var content = (RectTransform)dungeonRoot;
        content.anchorMin = content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 510f);
        var layout = content.GetComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        layout.spacing = 20f;
        layout.padding = new RectOffset(6, 6, 4, 4);
        content.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        var bossTemplate = dungeonRoot.Find("RedDragon").gameObject;
        var endlessTemplate = dungeonRoot.Find("SixHeroes").gameObject;
        foreach (Transform old in dungeonRoot) old.gameObject.SetActive(false);
        var buttonPrefab = Resources.Load<GameObject>("Prefabs/09UI/StageButton");
        foreach (var row in LunaStageTables.Stages.GetAllDataRows().OrderBy(value => value.Id))
        {
            if (row.Key.StartsWith("TEST", System.StringComparison.Ordinal)) continue;
            bool campaign = row.Kind == LunaStageKind.Basic;
            StageButton view;
            if (campaign) view = Instantiate(buttonPrefab, stageRoot, false).GetComponent<StageButton>();
            else view = Instantiate(row.Kind == LunaStageKind.Endless ? endlessTemplate : bossTemplate,
                dungeonRoot, false).AddComponent<StageButton>();
            view.name = row.Key;
            if (campaign) view.Bind(row, LunaStageTables.IsUnlocked(row, profile), row.Key == profile.SaveStageName, () => Preview(row));
            else view.BindDungeon(row, LunaStageTables.IsUnlocked(row, profile), () => Preview(row));
            view.gameObject.SetActive(true);
        }
        StartCoroutine(ScrollToFloor(LunaSession.CampaignFloor(profile.SaveStageName)));
        UpdateTabAppearance(false);
        Preview(selected);
    }

    private IEnumerator ScrollToFloor(int floor)
    {
        yield return null;
        stageView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f - (floor - 1) / 29f;
    }

    private void SetTab(bool dungeon)
    {
        LunaUI.Close(UI_TYPE.STAGEINFO);
        LunaUI.Close(UI_TYPE.DUNGEONINFO);
        stageView.SetActive(!dungeon);
        dungeonView.SetActive(dungeon);
        UpdateTabAppearance(dungeon);
        if (!dungeon && selectedCampaign != null) Preview(selectedCampaign);
    }

    private void UpdateTabAppearance(bool dungeon)
    {
        if (listPanel != null) listPanel.SetActive(!dungeon);
        if (detailPanel != null) detailPanel.SetActive(!dungeon);
        var jump = transform.Find("JumpToProgress"); if (jump != null) jump.gameObject.SetActive(!dungeon);
        var navigation = transform.Find("DungeonNavigation"); if (navigation != null) navigation.gameObject.SetActive(dungeon);
        transform.Find("Stage_Tab").GetComponent<Image>().color = dungeon ? Color.gray : LunaUIPresentation.Gold;
        transform.Find("Dungeon_Tab").GetComponent<Image>().color = dungeon ? LunaUIPresentation.Gold : Color.gray;
        if (progress != null) progress.text = dungeon ? "보스 토벌 · 무한 생존" :
            "악몽의 성 · " + LunaDataTable.Instance.StageTable.Values.Count(row => row.isClear) + " / 30층 클리어";
    }

    public void OpenParty()
    {
        LunaUI.Close(UI_TYPE.STAGEINFO); LunaUI.Close(UI_TYPE.DUNGEONINFO);
        LunaUI.Open(UI_TYPE.CHARACTERLIST); returningFromParty = true;
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        if (!returningFromParty || LunaUI.IsOpenOrLoading(UI_TYPE.CHARACTERLIST)) return;
        returningFromParty = false;
        if (selected != null) Preview(selected);
    }

    private void Preview(LunaStageRow row)
    {
        selected = row;
        foreach (var button in GetComponentsInChildren<StageButton>(true)) button.Select(button.name == row.Key);
        if (row.Kind == LunaStageKind.Basic)
        {
            selectedCampaign = row;
            LunaUI.Close(UI_TYPE.STAGEINFO);
            LunaSession.Current.CurrentStageName = row.Key;
            LunaSession.Current.SetStagePrefab(row.Key);
            LunaUI.Open(UI_TYPE.STAGEINFO);
        }
        else
        {
            LunaUI.Close(UI_TYPE.DUNGEONINFO);
            LunaSession.Current.SelectDungeonString = row.Key;
            LunaSession.Current.selectDungeon = LunaStageTables.RequireAsset<GameObject>("Prefabs/11Dungeons/" + row.Key);
            LunaUI.Open(UI_TYPE.DUNGEONINFO);
        }
    }

    // Inspector callbacks on the original dungeon cards.
    public void OnClickSelectDungeon(int index) => Preview(LunaStageTables.Get("Dungeon" + index.ToString("D2")));

    public void OnClickReleaseButton()
    {
        LunaUI.Close(UI_TYPE.STAGEINFO);
        LunaUI.Close(UI_TYPE.DUNGEONINFO);
        LunaUI.Close(type);
    }
}

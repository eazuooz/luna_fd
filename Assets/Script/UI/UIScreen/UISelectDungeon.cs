using System.Collections;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UISelectDungeon : UILuna
{
    private GameObject stageView, dungeonView;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
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
        stages.GetComponent<Button>().onClick.AddListener(() => SetTab(false));
        dungeons.GetComponent<Button>().onClick.AddListener(() => SetTab(true));
        SetTab(false);

        var profile = LunaDataTable.Instance.playerData;
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
    }

    private static void Preview(LunaStageRow row)
    {
        if (row.Kind == LunaStageKind.Basic)
        {
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

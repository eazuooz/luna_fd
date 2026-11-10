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
        dungeonRect.offsetMax -= Vector2.right * 190f;
        dungeonView.GetComponent<ScrollRect>().horizontal = false;
        dungeonRect.Find("KeyInfo").gameObject.SetActive(false);
        var dungeonHint = dungeonRect.Find("ExplaneText").GetComponent<Text>();
        dungeonHint.rectTransform.anchoredPosition = new Vector2(150f, 339f);
        dungeonHint.rectTransform.sizeDelta = new Vector2(720f, 40f);
        dungeonHint.alignment = TextAnchor.MiddleRight;
        dungeonHint.text = "기본 스테이지를 클리어하면 던전이 열립니다.";
        stages.GetComponent<Button>().onClick.AddListener(() => SetTab(false));
        dungeons.GetComponent<Button>().onClick.AddListener(() => SetTab(true));
        SetTab(false);

        var profile = LunaDataTable.Instance.playerData;
        var stageRoot = stageView.transform.Find("Stage_Viewport/Stage_Content");
        var dungeonRoot = dungeonView.transform.Find("Dungeon_Scroll View_Viewport/Dungeon_Content");
        foreach (Transform old in dungeonRoot) old.gameObject.SetActive(false);
        var buttonPrefab = Resources.Load<GameObject>("Prefabs/09UI/StageButton");
        foreach (var row in LunaStageTables.Stages.GetAllDataRows().OrderBy(value => value.Id))
        {
            if (row.Key.StartsWith("TEST", System.StringComparison.Ordinal)) continue;
            bool campaign = row.Kind == LunaStageKind.Basic;
            var view = Instantiate(buttonPrefab, campaign ? stageRoot : dungeonRoot, false).GetComponent<StageButton>();
            view.name = row.Key;
            if (!campaign)
                ((RectTransform)view.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200f);
            view.Bind(row, LunaStageTables.IsUnlocked(row, profile), row.Key == profile.SaveStageName,
                () => Preview(row));
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

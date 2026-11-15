#if UNITY_EDITOR || DEBUG
using System.Collections;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class LunaNativeValidation
{
    private IEnumerator PresentationContracts()
    {
        yield return EnsureFixtureLobby();
        BeginCase("presentation", "initial-selection-and-party-return");
        var progress = LunaDataTable.Instance.StageTable.ToDictionary(pair => pair.Key, pair => pair.Value.isClear);
        string originalStage = LunaDataTable.Instance.playerData.SaveStageName;
        int originalUnlocked = LunaDataTable.Instance.playerData.UnlockedFloor;
        bool originalComplete = LunaDataTable.Instance.playerData.CampaignComplete;
        try
        {
        foreach (var stage in LunaDataTable.Instance.StageTable.Values) stage.isClear = false;
        LunaDataTable.Instance.playerData.SaveStageName = "Stage01-01";
        LunaDataTable.Instance.playerData.UnlockedFloor = 1; LunaDataTable.Instance.playerData.CampaignComplete = false;
        yield return OpenNativeUi(UI_TYPE.SELECTSTAGE);
        yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Initial map detail");
        Require(LunaSession.Current.CurrentStageName == LunaDataTable.Instance.playerData.SaveStageName,
            "First open did not select the saved campaign stage.");
        var map = NativeUi(UI_TYPE.SELECTSTAGE);
        string finalStage = LunaStageTables.Stages.GetAllDataRows().Where(row => row.Kind == LunaStageKind.Basic).OrderBy(row => row.Id).Last().Key;
        map.GetComponentsInChildren<StageButton>().Single(row => row.name == finalStage).GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Locked stage detail");
        var detail = NativeUi(UI_TYPE.STAGEINFO);
        Require(!detail.transform.Find("BackGround/StartButton").GetComponent<Button>().interactable, "Locked floor enabled battle entry.");
        Require(detail.transform.Find("BackGround/StageBriefing").GetComponent<Text>().text.Contains("목표:"), "Stage goal was not presented.");
        var briefing = detail.transform.Find("BackGround/StageBriefing").GetComponent<Text>();
        using (var generator = new TextGenerator())
            Require(generator.GetPreferredHeight(briefing.text, briefing.GetGenerationSettings(briefing.rectTransform.rect.size)) <= briefing.rectTransform.rect.height + 0.1f,
                "The locked final stage briefing clips its goal, reward or tactical tip.");
        detail.transform.Find("BackGround/SelectButton").GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.CHARACTERLIST) != null && !LunaUI.IsOpenOrLoading(UI_TYPE.STAGEINFO), "Map party action");
        yield return CloseNativeUi(UI_TYPE.CHARACTERLIST);
        yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Return to selected detail");
        Require(LunaSession.Current.CurrentStageName == finalStage, "Party return lost the selected stage.");
        map.transform.Find("Dungeon_Tab").GetComponent<Button>().onClick.Invoke();
        map.GetComponentsInChildren<StageButton>().Single(row => row.name == "Dungeon01").GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.DUNGEONINFO) != null, "Dungeon detail round trip");
        yield return CloseNativeUi(UI_TYPE.DUNGEONINFO);
        map.transform.Find("Stage_Tab").GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Campaign detail after dungeon tab");
        Require(LunaSession.Current.CurrentStageName == finalStage, "Dungeon preview replaced the saved campaign selection or left an empty detail panel.");
        yield return CloseNativeUi(UI_TYPE.SELECTSTAGE); yield return CloseNativeUi(UI_TYPE.STAGEINFO);
        }
        finally
        {
            foreach (var pair in progress) LunaDataTable.Instance.StageTable[pair.Key].isClear = pair.Value;
            LunaDataTable.Instance.playerData.SaveStageName = originalStage;
            LunaDataTable.Instance.playerData.UnlockedFloor = originalUnlocked; LunaDataTable.Instance.playerData.CampaignComplete = originalComplete;
        }
        EndCase("First open populated the right panel; selecting a locked floor and opening/closing formation retained the selection.");

        BeginCase("presentation", "skill-upgrade-material-transaction");
        var table = LunaDataTable.Instance;
        var skill = table.skiiTable["FireBall"]; var stones = table.itemTable[1001];
        int oldLevel = skill.level, oldStones = stones.itemCount;
        try
        {
            skill.level = 4; stones.itemCount = LunaEconomy.SkillStoneCost(4);
            yield return OpenNativeUi(UI_TYPE.UPGRADE);
            LunaSession.Current.OnClickSkillName = "FireBall";
            yield return OpenNativeUi(UI_TYPE.SKILLINFO);
            var info = (UISkillInfo)NativeUi(UI_TYPE.SKILLINFO);
            var button = info.transform.Find("UpgradeSkill").GetComponent<Button>();
            Require(button.interactable, "Affordable skill upgrade is disabled.");
            button.onClick.Invoke();
            Require(skill.level == 5 && stones.itemCount == 0 && !button.interactable, "Skill upgrade cost/next state differs from its actual transaction.");
            info.OnClickSkillLevelUp();
            Require(skill.level == 5 && stones.itemCount == 0, "Insufficient materials allowed a free skill level.");
            skill.level = LunaEconomy.SkillMaxLevel; stones.itemCount = 1000;
            Require(!LunaEconomy.TryLevelUpSkill(skill) && stones.itemCount == 1000, "Max skill level charged materials or advanced.");
        }
        finally { skill.level = oldLevel; stones.itemCount = oldStones; }
        yield return CloseNativeUi(UI_TYPE.SKILLINFO); yield return CloseNativeUi(UI_TYPE.UPGRADE);
        EndCase("The real upgrade callback charged exactly the displayed stones, crossed LV5, rejected an unaffordable repeat and respected LV20.");

        BeginCase("presentation", "inventory-selection-and-real-destination");
        yield return OpenNativeUi(UI_TYPE.INVENTORY);
        yield return Until(() => NativeUi(UI_TYPE.ITEMINFO) != null, "First inventory detail");
        var cell = NativeUi(UI_TYPE.INVENTORY).GetComponentsInChildren<Button>(true).First(button => button.name == "OwnedItem_1001");
        cell.onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.ITEMINFO) != null, "Selected skill stones");
        Require(NativeUi(UI_TYPE.ITEMINFO).transform.Find("BackGround/Profile/MainText").GetComponent<Text>().text.Contains("보유 수량"), "Item count missing from detail.");
        NativeUi(UI_TYPE.ITEMINFO).transform.Find("BackGround/Button").GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.UPGRADE) != null && !LunaUI.IsOpenOrLoading(UI_TYPE.INVENTORY), "Inventory use destination");
        Require(NativeUi(UI_TYPE.UPGRADE).GetComponentsInChildren<ScrollRect>().Any(scroll => scroll.name == "Skill_ScrollView"), "Skill materials routed to the wrong upgrade tab.");
        yield return CloseNativeUi(UI_TYPE.UPGRADE);
        EndCase("The inventory selected an owned item on open, exposed quantity and navigated to actual skill growth without a fictitious currency charge.");

        BeginCase("presentation", "growth-affordability-and-party-slot-guards");
        var profile = table.playerData;
        int oldGold = profile.Gold;
        try
        {
            profile.Gold = LunaEconomy.UnitLevelCost(profile.LunaLevel) - 1;
            yield return OpenNativeUi(UI_TYPE.UPGRADE);
            var row = NativeUi(UI_TYPE.UPGRADE).GetComponentsInChildren<Transform>(true).Single(child => child.name == "Luna_LevelUp");
            var button = row.GetComponentsInChildren<Button>(true).First();
            Require(!button.interactable, "Below-cost gold enabled permanent growth.");
            profile.Gold++;
            yield return null;
            Require(button.interactable && button.GetComponentsInChildren<Text>(true).Any(label => label.text.Contains(profile.Gold.ToString("N0"))), "Displayed growth price or affordability differs from the actual cost.");
            yield return CloseNativeUi(UI_TYPE.UPGRADE);
            yield return OpenNativeUi(UI_TYPE.CHARACTERLIST);
            var list = (UICharacterList)NativeUi(UI_TYPE.CHARACTERLIST);
            foreach (var card in list.GetComponentsInChildren<UICharacterButton>())
            {
                var nameLabel = card.GetComponentsInChildren<Text>().Single(label => label.name == "NameText");
                var settings = nameLabel.GetGenerationSettings(nameLabel.rectTransform.rect.size);
                using (var generator = new TextGenerator())
                    Require(generator.GetPreferredHeight(nameLabel.text, settings) <= nameLabel.rectTransform.rect.height + 0.1f,
                        "Collection name exceeds its text rectangle: " + nameLabel.text);
            }
            string hero = table.heroTable.First(key => !string.IsNullOrEmpty(key) && !profile.UnitNamesInDeck.Contains(key));
            var unit = table.UnitTable[hero]; int oldCards = unit.cardCount;
            var deck = profile.UnitNamesInDeck.ToArray();
            try
            {
                unit.cardCount = Mathf.Max(1, oldCards);
                list.ReadyToJoinMyDeck(hero); list.OnClickDeckUnitButton(4);
                list.OnClickReleaseButton();
                Require(profile.UnitNamesInDeck.SequenceEqual(deck), "An owned hero entered the first soldier slot.");
            }
            finally { unit.cardCount = oldCards; }
        }
        finally { profile.Gold = oldGold; }
        yield return CloseNativeUi(UI_TYPE.CHARACTERLIST);
        EndCase("Displayed growth cost matched the real wallet threshold and formation rejected an owned hero in a soldier slot.");

        BeginCase("presentation", "result-party-destination");
        yield return LoadStage("Stage01-01");
        var round = (BasicDungeon)LunaScenes.CurrentGame;
        UnitLogic.From(round.gameAllyTower).stat.Hp = 0f;
        yield return Until(() => NativeUi(UI_TYPE.RESULT) != null, "Defeat summary");
        Require(NativeUi(UI_TYPE.RESULT).transform.Find("BattleSummary").GetComponent<Text>().text.Contains("소환"), "Result omitted battle actions.");
        ((UIResult)NativeUi(UI_TYPE.RESULT)).PartyBt();
        yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning && NativeUi(UI_TYPE.CHARACTERLIST) != null, "Result to lobby formation");
        Require(NativeUi(UI_TYPE.RESULT) == null && LunaScenes.CurrentGame == null, "Formation stayed behind a completed battle popup.");
        yield return CloseNativeUi(UI_TYPE.CHARACTERLIST);
        EndCase("Defeat summary counted actions and the formation button returned to a ready lobby instead of opening beneath the result.");
    }

    private IEnumerator CollectionPanelCaptures()
    {
        var oldSize = new Vector2Int(Screen.width, Screen.height);
        var progress = LunaDataTable.Instance.StageTable.ToDictionary(pair => pair.Key, pair => pair.Value.isClear);
        string originalStage = LunaDataTable.Instance.playerData.SaveStageName;
        int originalUnlocked = LunaDataTable.Instance.playerData.UnlockedFloor;
        bool originalComplete = LunaDataTable.Instance.playerData.CampaignComplete;
        try
        {
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(1024, 768) })
            {
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.2f);
                yield return EnsureFixtureLobby();
                foreach (var stage in LunaDataTable.Instance.StageTable.Values) stage.isClear = false;
                LunaDataTable.Instance.playerData.SaveStageName = "Stage01-01";
                LunaDataTable.Instance.playerData.UnlockedFloor = 1; LunaDataTable.Instance.playerData.CampaignComplete = false;
                yield return OpenNativeUi(UI_TYPE.SELECTSTAGE);
                yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Initial selection capture");
                CaptureFlowPanel("map-initial-" + size.x + "x" + size.y, NativeUi(UI_TYPE.STAGEINFO), size.x, size.y,
                    "BackGround/StageBriefing", "BackGround/StageArtwork", "BackGround/StartButton");
                yield return CloseNativeUi(UI_TYPE.SELECTSTAGE); yield return CloseNativeUi(UI_TYPE.STAGEINFO);
                foreach (var pair in progress) LunaDataTable.Instance.StageTable[pair.Key].isClear = pair.Value;
                LunaDataTable.Instance.playerData.SaveStageName = originalStage;
                LunaDataTable.Instance.playerData.UnlockedFloor = originalUnlocked; LunaDataTable.Instance.playerData.CampaignComplete = originalComplete;
                yield return OpenNativeUi(UI_TYPE.CHARACTERLIST);
                CaptureFlowPanel("collection-" + size.x + "x" + size.y, NativeUi(UI_TYPE.CHARACTERLIST), size.x, size.y, "PartyHelp", "SortOwned");
                ((UICharacterList)NativeUi(UI_TYPE.CHARACTERLIST)).PickedUnitName = "Alice_Card";
                yield return OpenNativeUi(UI_TYPE.CHARACTERINFO);
                CaptureFlowPanel("character-info-" + size.x + "x" + size.y, NativeUi(UI_TYPE.CHARACTERINFO), size.x, size.y, "levelupTab", "wakeUpTab");
                yield return CloseNativeUi(UI_TYPE.CHARACTERINFO); yield return CloseNativeUi(UI_TYPE.CHARACTERLIST);
                yield return OpenNativeUi(UI_TYPE.UPGRADE);
                CaptureFlowPanel("growth-" + size.x + "x" + size.y, NativeUi(UI_TYPE.UPGRADE), size.x, size.y, "Board");
                LunaSession.Current.OnClickSkillName = "FireBall";
                yield return OpenNativeUi(UI_TYPE.SKILLINFO);
                CaptureFlowPanel("skill-info-" + size.x + "x" + size.y, NativeUi(UI_TYPE.SKILLINFO), size.x, size.y, "UpgradeSkill", "SkillResourceHint");
                yield return CloseNativeUi(UI_TYPE.SKILLINFO); yield return CloseNativeUi(UI_TYPE.UPGRADE);
                yield return OpenNativeUi(UI_TYPE.INVENTORY);
                yield return Until(() => NativeUi(UI_TYPE.ITEMINFO) != null, "Inventory detail capture");
                CaptureFlowPanel("inventory-" + size.x + "x" + size.y, NativeUi(UI_TYPE.ITEMINFO), size.x, size.y, "BackGround/Button", "BackGround/Profile/MainText");
                yield return CloseNativeUi(UI_TYPE.INVENTORY); yield return CloseNativeUi(UI_TYPE.ITEMINFO);
            }
        }
        finally
        {
            foreach (var pair in progress) LunaDataTable.Instance.StageTable[pair.Key].isClear = pair.Value;
            LunaDataTable.Instance.playerData.SaveStageName = originalStage;
            LunaDataTable.Instance.playerData.UnlockedFloor = originalUnlocked; LunaDataTable.Instance.playerData.CampaignComplete = originalComplete;
            Screen.SetResolution(oldSize.x, oldSize.y, FullScreenMode.Windowed);
        }
    }
}
#endif

#if UNITY_EDITOR || DEBUG
using System.Collections;
using System.IO;
using System.Linq;
using GameFramework;
using GameFramework.Setting;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class LunaNativeValidation
{
    private IEnumerator LevelDesignContracts()
    {
        yield return EnsureFixtureLobby();
        yield return CardShopContract();
        PrototypeSaveContract();
        yield return StageSelectionContract();
        yield return BossPhaseContract();
        yield return TurnToThreatContract();
        yield return CampaignReplayContract();
        yield return CampaignFinaleContract();
    }

    private IEnumerator CardShopContract()
    {
        BeginCase("economy", "finite-card-shop");
        var data = LunaDataTable.Instance;
        var profile = data.playerData;
        profile.IsBegining = true;
        profile.Gold = LunaEconomy.SingleDrawGold + LunaEconomy.BundleDrawGold;
        int initialCards = data.UnitTable.Values.Sum(unit => unit.cardCount), initialGold = profile.Gold;
        int stones = profile.StarStone;
        yield return OpenNativeUi(UI_TYPE.GAMESHOP);
        var shop = (UIGameShop)NativeUi(UI_TYPE.GAMESHOP);
        var goods = Active<UIGoods>();
        Require(goods != null && !goods.transform.Find("Ruby").gameObject.activeSelf &&
            !shop.transform.Find("Board/Summon_Tab/Summon_ScrollView/Summon_Scroll View_Viewport/Summon_Content/ViewAD").gameObject.activeSelf,
            "The wallet or shop still exposes a prototype currency or ad button.");
        shop.OnClickBuyStarStone();
        Require(profile.StarStone == stones, "The prototype shop granted free stones.");
        shop.OnClickSummons();
        shop.OnClickSummons();
        yield return Until(() => NativeUi(UI_TYPE.LOTTERY) != null, "Free starter reveal");
        Require(!profile.IsBegining && profile.Gold == initialGold && data.LotteryResult.Count == 5 &&
            data.LotteryResult.Distinct().Count() == 5 &&
            data.UnitTable.Values.Sum(unit => unit.cardCount) == initialCards + 5,
            "Starter pack repeated, charged, or granted the wrong number of cards.");
        yield return CloseNativeUi(UI_TYPE.LOTTERY);
        shop.OnClickSummon();
        yield return Until(() => NativeUi(UI_TYPE.LOTTERY) != null, "Paid single reveal");
        Require(profile.Gold == LunaEconomy.BundleDrawGold && data.LotteryResult.Count == 1, "Single draw did not charge once.");
        yield return CloseNativeUi(UI_TYPE.LOTTERY);
        shop.OnClickSummons();
        yield return Until(() => NativeUi(UI_TYPE.LOTTERY) != null, "Paid bundle reveal");
        Require(profile.Gold == 0 && data.LotteryResult.Count == 10 &&
            data.LotteryResult.Any(name => data.UnitTable[name].UnitType == eUnitType.Hero) &&
            data.UnitTable.Values.Sum(unit => unit.cardCount) == initialCards + 16, "Bundle gold, quantity or hero guarantee failed.");
        yield return CloseNativeUi(UI_TYPE.LOTTERY);
        shop.OnClickSummon();
        Require(!LunaUI.IsOpenOrLoading(UI_TYPE.LOTTERY) && profile.Gold == 0 &&
            data.UnitTable.Values.Sum(unit => unit.cardCount) == initialCards + 16, "Insufficient gold yielded an award.");
        yield return CloseNativeUi(UI_TYPE.GAMESHOP);
        Require(LunaRuntimeData.Reload() && profile.Gold == 0 && !profile.IsBegining &&
            data.UnitTable.Values.Sum(unit => unit.cardCount) == initialCards + 16, "Paid shop progress did not survive reload.");

        var unit = data.UnitTable["Alice"];
        var oldRank = unit.ultState;
        int oldCards = unit.cardCount, oldStones = profile.StarStone;
        unit.ultState = eUltState.STAR5;
        unit.cardCount = LunaEconomy.RankCardCost(unit.ultState);
        profile.StarStone = 0;
        Require(!LunaEconomy.TryRankUp(profile, unit), "A rank upgrade borrowed missing stones.");
        profile.StarStone = LunaEconomy.RankStoneCost(unit.ultState);
        Require(LunaEconomy.TryRankUp(profile, unit) && unit.ultState == eUltState.ULT1 &&
            unit.cardCount == 0 && profile.StarStone == 0, "Rank upgrade did not deduct exact cards and stones.");
        unit.ultState = oldRank; unit.cardCount = oldCards; profile.StarStone = oldStones;
        EndCase("Real shop UI paid once, revealed 5/1/10 cards, guaranteed a bundle hero, rejected overspending and persisted the purchase. Rank costs use both balances.");
    }

    private void PrototypeSaveContract()
    {
        BeginCase("economy", "v1-wallet-and-progress-migration");
        var data = LunaDataTable.Instance;
        var profile = data.playerData;
        var settings = GameFrameworkEntry.GetModule<ISettingManager>();
        const string key = "Luna.Progress.v1";
        Require(LunaRuntimeData.SaveNow(), "Seed before migration fixture.");
        string original = settings.GetString(key);
        try
        {
            profile.Gold = 1186567 + 2400; profile.StarStone = 1234567 + 4000;
            profile.SaveStageName = "Stage01-06";
            var alice = data.UnitTable["Alice"];
            alice.cardCount = 103; alice.ultState = eUltState.ULT2; alice.Level = 4;
            Require(LunaRuntimeData.SaveNow(), "Seed prototype wallet.");
            string legacy = settings.GetString(key).Replace("\"version\":2", "\"version\":1");
            Require(legacy.Contains("\"version\":1"), "Fixture did not produce a v1 document.");
            settings.SetString(key, legacy);
            Require(settings.Save() && LunaRuntimeData.Reload(), "v1 progress reload failed.");
            Require(profile.Gold == LunaEconomy.StartingGold + 2400 && profile.StarStone == 0 &&
                profile.UnlockedFloor >= 6 && alice.cardCount == 4 && alice.ultState == eUltState.STAR1 && alice.Level == 4,
                "Prototype migration lost earned progress or kept the debug stock.");
            Require(LunaRuntimeData.SaveNow() && LunaRuntimeData.Reload() && alice.cardCount == 4 &&
                profile.Gold == LunaEconomy.StartingGold + 2400, "Economy migration was applied twice.");
        }
        finally
        {
            settings.SetString(key, original);
            Require(settings.Save() && LunaRuntimeData.Reload(), "Restore isolated progress after migration fixture.");
        }
        EndCase("A v1 save with the shipped prototype grants preserved earned excess, unit level and campaign access. The migrated v2 data reloaded unchanged.");
    }

    private IEnumerator StageSelectionContract()
    {
        BeginCase("level-design", "locked-map-and-dungeon-list");
        var profile = LunaDataTable.Instance.playerData;
        int previous = profile.UnlockedFloor;
        profile.UnlockedFloor = 1;
        yield return OpenNativeUi(UI_TYPE.SELECTSTAGE);
        var map = NativeUi(UI_TYPE.SELECTSTAGE);
        var rows = map.GetComponentsInChildren<StageButton>(true);
        Require(rows.Length == 35 && rows.Select(row => row.name).Distinct().Count() == 35, "Map omitted a public encounter or included a test arena.");
        rows.Single(row => row.name == "Stage03-10").GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Locked campaign preview");
        var stage = (UIStageInfo)NativeUi(UI_TYPE.STAGEINFO);
        Require(!stage.transform.Find("BackGround/StartButton").GetComponent<Button>().interactable, "Locked stage start is enabled.");
        stage.OnClickStartGame();
        Require(LunaScenes.CurrentScene == "Lobby" && LunaScenes.RequestedScene == null, "Locked campaign entry bypassed the map.");
        map.transform.Find("Dungeon_Tab").GetComponent<Button>().onClick.Invoke();
        rows.Single(row => row.name == "Dungeon05_old").GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.DUNGEONINFO) != null, "Second Endless preview");
        var dungeon = (UIDungeonInfo)NativeUi(UI_TYPE.DUNGEONINFO);
        Require(!dungeon.transform.Find("BackGround/StartButton").GetComponent<Button>().interactable &&
            LunaSession.Current.SelectDungeonString == "Dungeon05_old", "Dungeon gate or native encounter identity differs.");
        dungeon.OnClickSelectDungeon(0);
        Require(LunaScenes.RequestedScene == null, "Locked dungeon entry bypassed progression.");
        profile.UnlockedFloor = previous;
        yield return CloseNativeUi(UI_TYPE.DUNGEONINFO);
        yield return CloseNativeUi(UI_TYPE.SELECTSTAGE);
        EndCase("Campaign and dungeon tabs listed 30/5 encounters. Locked previews were readable and rejected real entry callbacks.");
    }

    private IEnumerator BossPhaseContract()
    {
        BeginCase("level-design", "boss-two-health-thresholds");
        yield return LoadDungeonFixture("Dungeon01");
        var game = (BossDungeon)LunaScenes.CurrentGame;
        Time.timeScale = 4f;
        var phases = LunaStageTables.GetWaves(game.Definition.Row.Id, "BossPhase");
        int first = phases.Where(wave => wave.At > 0.5f).Sum(wave => wave.Count);
        int total = phases.Sum(wave => wave.Count);
        int CountSupports() => UnitLogic.ActiveUnits.Count(actor => actor != game.Boss && actor.Data != null &&
            actor.Data.RoundId == game.RoundId && actor.UnitSide == eUnitSide.Enemy);
        Require(CountSupports() == 0 && first > 0 && total > first, "Boss started with untelegraphed reinforcements or invalid thresholds.");
        game.Boss.stat.Hp = game.Boss.HealthBarHpPoint * 0.64f;
        yield return Until(() => CountSupports() >= first && game.PendingSpawnCount == 0, "First boss reinforcements");
        yield return new WaitForSeconds(2f);
        Require(CountSupports() == first, "First health threshold spawned repeatedly.");
        game.Boss.stat.Hp = game.Boss.HealthBarHpPoint * 0.29f;
        yield return Until(() => CountSupports() >= total && game.PendingSpawnCount == 0, "Second boss reinforcements");
        Require(CountSupports() == total && !game.isStageStop, "Second health threshold did not preserve the encounter.");
        yield return EnsureFixtureLobby();
        EndCase("Boss reinforcement groups appeared once per descending health threshold.");
    }

    private IEnumerator CampaignFinaleContract()
    {
        BeginCase("level-design", "final-floor-completion");
        yield return LoadStage("Stage03-10");
        var game = (BasicDungeon)LunaScenes.CurrentGame;
        HoldRoundActors(game);
        UnitLogic.From(game.gameEnemyTower).stat.Hp = 0f;
        yield return Until(() => Active<UIResult>() != null, "Final campaign result");
        var profile = LunaDataTable.Instance.playerData;
        Require(game.isStageWin && !game.HasNextStage && profile.CampaignComplete && profile.UnlockedFloor == 30 &&
            profile.SaveStageName == "Stage03-10" &&
            !Active<UIResult>().transform.Find("ImagePlate/NextStage").gameObject.activeSelf, "The campaign finale wrapped or failed to mark completion.");
        Active<UIResult>().GoNextStage();
        Require(LunaScenes.RequestedScene == null, "Final stage next action restarted the campaign.");
        yield return EnsureFixtureLobby();
        Require(LunaRuntimeData.Reload() && profile.CampaignComplete, "Campaign completion did not persist.");
        EndCase("30th-floor victory persisted completion and offered no wraparound next floor.");
    }

    private IEnumerator TurnToThreatContract()
    {
        BeginCase("level-design", "luna-turns-to-enemy-behind");
        yield return LoadStage("Stage01-01");
        var round = LunaScenes.CurrentGame;
        var player = (PlayerLogic)round.Player;
        player.Data.TargetId = 0;
        UnitLogic threat = null;
        round.SpawnUnit(Resources.Load<GameObject>("Prefabs/04Monsters/SkeletonSword"),
            player.transform.position + Vector3.left * 0.8f, shown: actor => threat = actor);
        yield return Until(() => threat != null, "Enemy behind player");
        HoldAutomaticActions(threat);
        float before = threat.stat.Hp;
        yield return Until(() => threat.stat.Hp < before, "Native attack facing the enemy behind", 8f);
        Require(player.Animation.Skeleton.ScaleX < 0f, "Luna did not face her acquired target.");
        yield return EnsureFixtureLobby();
        EndCase("A real stationary Luna attack acquired a newly spawned threat behind her and turned toward it.");
    }

    private IEnumerator CampaignReplayContract()
    {
        BeginCase("level-design", "first-win-next-and-replay");
        var data = LunaDataTable.Instance;
        var profile = data.playerData;
        const string first = "Stage01-01";
        data.StageTable[first].isClear = false;
        profile.UnlockedFloor = 1;
        yield return LoadStage(first);
        var game = (BasicDungeon)LunaScenes.CurrentGame;
        var stage = game.Definition.Row;
        int before = profile.Gold;
        int firstRound = game.RoundId;
        HoldRoundActors(game);
        UnitLogic.From(game.gameEnemyTower).stat.Hp = 0f;
        yield return Until(() => Active<UIResult>() != null, "First victory reward");
        Require(profile.Gold - before == stage.Gold + stage.FirstClearGold &&
            profile.UnlockedFloor == 2 && data.StageTable[first].isClear, "First victory did not pay/unlock once.");
        Active<UIResult>().GoNextStage();
        yield return Until(() => LunaScenes.CurrentGame is BasicDungeon next &&
            next.RoundId != firstRound && next.StageKey == "Stage01-02" && next.PhaseName == "Combat",
            "Normal next-floor scene transition");
        Require(!Entities().Any(actor => actor.SpawnData.RoundId == firstRound), "Next floor retained the retired combat actors.");
        yield return LoadStage(first);
        game = (BasicDungeon)LunaScenes.CurrentGame;
        HoldRoundActors(game);
        before = profile.Gold;
        UnitLogic.From(game.gameEnemyTower).stat.Hp = 0f;
        yield return Until(() => Active<UIResult>() != null, "Replay reward");
        Require(profile.Gold - before == stage.Gold && profile.UnlockedFloor == 2, "Replay duplicated the first-clear bonus or advanced the frontier.");
        yield return EnsureFixtureLobby();
        EndCase("Normal next-floor transition retired the completed round. Replay paid only its repeat reward.");
    }

    private IEnumerator DesignPanelCaptures()
    {
        yield return OpenNativeUi(UI_TYPE.GAMESHOP);
        CaptureDesignPanel("shop", NativeUi(UI_TYPE.GAMESHOP), "Board/ShopStatus",
            "Board/Summon_Tab/Summon_ScrollView/Summon_Scroll View_Viewport/Summon_Content/BuyOnce/SlimeOnceButton",
            "Board/Summon_Tab/Summon_ScrollView/Summon_Scroll View_Viewport/Summon_Content/BuyTens/SlimeOnceButton");
        yield return CloseNativeUi(UI_TYPE.GAMESHOP);

        yield return OpenNativeUi(UI_TYPE.SELECTSTAGE);
        var map = NativeUi(UI_TYPE.SELECTSTAGE);
        map.transform.Find("Dungeon_Tab").GetComponent<Button>().onClick.Invoke();
        yield return null;
        const string dungeonView = "Dungeon_Tab/Dungeon_ScrollView";
        const string dungeonCards = dungeonView + "/Dungeon_Scroll View_Viewport/Dungeon_Content/";
        CaptureDesignPanel("dungeon-list", map, dungeonView, dungeonCards + "Dungeon01", dungeonCards + "Dungeon05_old");
        var firstBoss = map.GetComponentsInChildren<StageButton>().Single(row => row.name == "Dungeon01");
        firstBoss.GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.DUNGEONINFO) != null, "Boss preview render");
        CaptureDesignPanel("boss-briefing", NativeUi(UI_TYPE.DUNGEONINFO), "BackGround/StageBriefing", "BackGround/StartButton");
        yield return CloseNativeUi(UI_TYPE.DUNGEONINFO);
        yield return CloseNativeUi(UI_TYPE.SELECTSTAGE);
    }

    private void CaptureDesignPanel(string name, UILuna form, params string[] paths)
    {
        BeginCase("render", name);
        renderBoundsFailures.Clear();
        var texture = CaptureOffscreen(1280, 720, (width, height) =>
        {
            foreach (string path in paths) MeasureUiRect(form, path, width, height);
        });
        try
        {
            File.WriteAllBytes(Path.Combine(PlayerOutputDirectory, "render-design-" + name + ".png"), texture.EncodeToPNG());
            Require(renderBoundsFailures.Count == 0, "Design controls exceed render bounds: " + string.Join("; ", renderBoundsFailures));
            EndCase("Rendered the actual panel offscreen and measured its new controls at 1280x720.");
        }
        finally { Destroy(texture); }
    }
}
#endif

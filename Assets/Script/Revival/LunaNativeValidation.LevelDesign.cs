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
        yield return BattleFeedbackContracts();
        yield return PresentationRepairContracts();
    }

    private IEnumerator PresentationRepairContracts()
    {
        BeginCase("presentation", "shop-reveal-layer-and-close");
        yield return EnsureFixtureLobby();
        var profile = LunaDataTable.Instance.playerData;
        profile.Gold = LunaEconomy.SingleDrawGold;
        yield return OpenNativeUi(UI_TYPE.GAMESHOP);
        var shop = (UIGameShop)NativeUi(UI_TYPE.GAMESHOP);
        shop.OnClickSummon();
        yield return Until(() => NativeUi(UI_TYPE.LOTTERY) != null, "Paid draw result");
        var reveal = (UILottery)NativeUi(UI_TYPE.LOTTERY);
        Require(LunaUI.IsTop(UI_TYPE.LOTTERY) && !shop.gameObject.activeInHierarchy &&
            reveal.GetComponentsInChildren<Canvas>().Count(canvas => canvas.overrideSorting) == 1,
            "The paid reveal is behind its shop or an avatar bypasses its form depth.");
        Require(profile.Gold == 0, "Paid reveal did not charge once.");
        yield return new WaitForSecondsRealtime(0.6f);
        reveal.Continue();
        yield return Until(() => !LunaUI.IsOpenOrLoading(UI_TYPE.LOTTERY) && shop.gameObject.activeInHierarchy,
            "Reveal close restores shop");
        yield return CloseNativeUi(UI_TYPE.GAMESHOP);
        EndCase("A real paid purchase covered the shop, inherited canvas depth and returned to the shop without residual avatars.");

        BeginCase("presentation", "map-viewport-and-dungeon-artwork");
        yield return OpenNativeUi(UI_TYPE.SELECTSTAGE);
        var map = NativeUi(UI_TYPE.SELECTSTAGE);
        yield return null;
        map.transform.Find("Stage_Tab/Stage_ScrollView").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        Canvas.ForceUpdateCanvases();
        var viewport = (RectTransform)map.transform.Find("Stage_Tab/Stage_ScrollView/Stage_Viewport");
        var first = map.GetComponentsInChildren<StageButton>(true).Single(row => row.name == "Stage01-01");
        var firstRect = (RectTransform)first.transform;
        Vector3[] corners = new Vector3[4]; firstRect.GetWorldCorners(corners);
        var local = corners.Select(corner => (Vector2)viewport.InverseTransformPoint(corner)).ToArray();
        current.observations.Add("Stage row in viewport " + viewport.rect + ": " + string.Join(", ", local.Select(point => point.ToString())));
        Require(local.All(point => point.x >= viewport.rect.xMin - 0.1f && point.x <= viewport.rect.xMax + 0.1f &&
            point.y >= viewport.rect.yMin - 0.1f && point.y <= viewport.rect.yMax + 0.1f),
            "The first stage row is clipped by its viewport.");
        first.GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Map side preview");
        var detail = NativeUi(UI_TYPE.STAGEINFO);
        Require(!detail.transform.Find("Image").gameObject.activeInHierarchy, "The side preview covers the whole map.");
        map.transform.Find("Dungeon_Tab").GetComponent<Button>().onClick.Invoke();
        var cards = map.GetComponentsInChildren<StageButton>().ToArray();
        Require(cards.Length == 5 && cards.All(card => card.transform.Find("DungeonImage").GetComponent<Image>().sprite != null &&
            !string.IsNullOrWhiteSpace(card.transform.Find("Manual").GetComponent<Text>().text)),
            "A public dungeon lost its artwork or description.");
        yield return CloseNativeUi(UI_TYPE.SELECTSTAGE);
        EndCase("The original illustrated dungeon cards listed all five encounters, and the stage row fit its map viewport.");

        BeginCase("presentation", "initial-camera-follow-with-ui-and-shake");
        yield return LoadStage("Stage01-01");
        var game = LunaScenes.CurrentGame;
        HoldRoundActors(game);
        var player = (PlayerLogic)game.Player;
        var camera = LunaBootstrap.GameplayCamera;
        float startX = camera.transform.position.x;
        player.SetMovement(1);
        yield return new WaitForSeconds(2f);
        game.ShakeCameraOn(2);
        Active<UIPlayer>().OnClickSummon(4);
        yield return new WaitForSeconds(4f);
        player.NMove();
        var point = camera.WorldToViewportPoint(player.transform.position);
        Require(camera.transform.position.x > startX + 1f && point.x >= 0.05f && point.x <= 0.95f &&
            (game.AttachCamera || game.FollowCamera), "First-life movement or camera shake lost camera tracking.");
        yield return EnsureFixtureLobby();
        EndCase("Normal first-life movement stayed inside the following camera after a summon action and camera shake.");
    }

    private IEnumerator BattleFeedbackContracts()
    {
        foreach (var reason in new[] { LunaGame.EndReason.TimeExpired, LunaGame.EndReason.TowerDestroyed, LunaGame.EndReason.PlayerDefeated })
        {
            BeginCase("feedback", "result-" + reason);
            if (reason == LunaGame.EndReason.PlayerDefeated) yield return LoadDungeonFixture("Dungeon01");
            else yield return LoadStage("Stage01-01");
            var game = LunaScenes.CurrentGame;
            HoldRoundActors(game);
            Time.timeScale = 5f;
            if (reason == LunaGame.EndReason.TimeExpired)
            {
                var basic = (BasicDungeon)game;
                UnitLogic.From(basic.gameEnemyTower).stat.Hp = UnitLogic.From(basic.gameEnemyTower).HealthBarHpPoint * 0.42f;
                float limit = game.Definition.Row.TimeLimit;
                try
                {
                    game.Definition.Row.TimeLimit = game.StageCount + 0.2f;
                    yield return Until(() => Active<UIResult>() != null, "Timeout result");
                }
                finally { game.Definition.Row.TimeLimit = limit; }
                Require(Mathf.Abs(game.RemainingObjectiveHealth - 0.42f) < 0.001f &&
                    Active<UIResult>().transform.Find("ResultBoard/ResultText").GetComponent<Text>().text.Contains("42%"),
                    "Timeout result lost the frozen objective health.");
            }
            else
            {
                if (game is BasicDungeon basic) UnitLogic.From(basic.gameAllyTower).stat.Hp = 0f;
                else game.Player.stat.Hp = 0f;
                yield return Until(() => Active<UIResult>() != null, "Defeat result");
            }
            Require(!game.isStageWin && game.ResultReason == reason, "The real end condition produced the wrong defeat reason.");
            Canvas.ForceUpdateCanvases();
            var detail = Active<UIResult>().transform.Find("ResultBoard/ResultText").GetComponent<Text>();
            Require(detail.cachedTextGenerator.lines.Count == 2,
                "The rendered result clipped its objective-health line.");
            yield return EnsureFixtureLobby();
            EndCase("Normal timeout, tower destruction or player death produced its distinct result and retained objective progress.");
        }
        yield return SummonFeedbackContract();
        yield return OpeningGuidanceContract();
        yield return AttackWarningContract();
    }

    private IEnumerator SummonFeedbackContract()
    {
        BeginCase("feedback", "summon-status-and-health");
        yield return LoadStage("Stage01-01");
        var game = LunaScenes.CurrentGame;
        HoldRoundActors(game);
        var hud = Active<UIPlayer>();
        var roster = game.Roster;
        roster.Gold = 0;
        yield return Until(() => roster.GetSlot(0).Remaining <= 0f, "Normal summon cooldown");
        Require(hud.SummonStatus(0).Contains("부족"), "Unaffordable summon is not explained.");
        roster.Gold = 100;
        Require(hud.SummonStatus(0) == "소환 가능", "Ready summon is not distinguished.");
        hud.OnClickSummon(0);
        yield return Until(() => roster.GetSlot(0).LiveUnit != null, "Real hero summon");
        var hero = roster.GetSlot(0).LiveUnit;
        hero.Invincible(0f);
        hero.stat.Hp = hero.HealthBarHpPoint * 0.25f;
        yield return null;
        Require(hud.SummonStatus(0).Contains("HP 25%") && !roster.CanSummon(0) &&
            hud.SummonCards[0].StatusText.text.Contains("HP 25%"),
            "Live hero health is absent or the status enables a duplicate summon.");
        hud.OnClickSummon(4);
        yield return Until(() => !roster.GetSlot(4).Pending, "Real soldier summon");
        Require(roster.GetSlot(4).Remaining > 0f && hud.SummonStatus(4).StartsWith("대기 "),
            "Soldier cooldown has no duration.");
        yield return EnsureFixtureLobby();
        EndCase("Actual HUD requests separated unaffordable/ready/live/cooldown states and showed injured hero health.");
    }

    private IEnumerator OpeningGuidanceContract()
    {
        BeginCase("feedback", "first-three-floors-guidance-and-opening-skill");
        var profile = LunaDataTable.Instance.playerData;
        var oldSkills = profile.equippedSkill.ToList();
        profile.equippedSkill = new[] { "FireBall", "Healing", "Thunder" }.ToList();
        try
        {
            for (int floor = 1; floor <= 3; floor++)
            {
                string key = "Stage01-0" + floor;
                var progress = LunaDataTable.Instance.StageTable[key];
                bool cleared = progress.isClear;
                progress.isClear = false;
                try
                {
                    yield return LoadStage(key);
                    var game = LunaScenes.CurrentGame;
                    HoldRoundActors(game);
                    Time.timeScale = 4f;
                    if (floor == 1)
                    {
                        yield return Until(() => game.Roster.CanSummon(4), "Starter soldier affordability");
                        Active<UIPlayer>().OnClickSummon(4);
                        yield return Until(() => game.Roster.SummonsCompleted > 0, "Guided summon native completion");
                        Require(game.GuidanceText.Contains("동료 합류 완료"), "Summon guidance did not advance.");
                    }
                    else
                    {
                        var player = (PlayerLogic)game.Player;
                        if (floor == 2)
                        {
                            player.SetMovement(-1);
                            yield return Until(() => game.HasRetreated, "Normal player retreat");
                            player.NMove();
                            Require(game.GuidanceText.Contains("후퇴 완료"), "Movement guidance did not advance.");
                            player.stat.Hp = player.HealthBarHpPoint * 0.5f;
                        }
                        string skill = floor == 2 ? "Healing" : "FireBall";
                        var snapshot = player.Abilities.Loadout.Skills.Single(value => value.name == skill);
                        yield return Until(() => player.Abilities.Ready(snapshot) && game.Roster.Mana >= snapshot.mana,
                            "Opening skill with natural mana and cooldown", 12f);
                        int mana = game.Roster.Mana;
                        Active<UISkill>().transform.Find("Skill_Button_" + (floor == 2 ? 1 : 0)).GetComponent<Button>().onClick.Invoke();
                        Require(game.SkillsUsed == 1 && game.Roster.Mana == mana - snapshot.mana &&
                            player.Abilities.Remaining(snapshot) == snapshot.coolTime && game.FirstSkillTime <= 10f,
                            "Opening skill did not use the real cost or restore its full reuse cooldown.");
                        Require(game.GuidanceText.Contains(floor == 2 ? "회복 완료" : "스킬 사용 완료"), "Skill guidance did not advance.");
                    }
                    yield return null;
                    Require(Active<UIPlayer>().transform.Find("BattleGuidance").GetComponent<Text>().text == game.GuidanceText,
                        "The tutorial action was not reflected in the visible HUD.");
                    yield return EnsureFixtureLobby();
                }
                finally { progress.isClear = cleared; }
            }
        }
        finally { profile.equippedSkill = oldSkills; }
        EndCase("Real summon, movement, heal and attack callbacks advanced opening guidance. Natural first skills were affordable by 10 seconds and paid once.");
    }

    private IEnumerator AttackWarningContract()
    {
        BeginCase("feedback", "dragon-mark-dodge-hit-and-death-cancel");
        yield return EnsureFixtureLobby();
        int round = NewFixtureRound();
        var prefab = Resources.Load<GameObject>("Prefabs/03EliteMonters/RedDragon");
        RedDragon dragon = null;
        PlayerLogic victim = null;
        try
        {
            var playerPrefab = LunaSession.Current.Players[0];
            yield return ShowFixture<PlayerLogic>(playerPrefab, ControlledData(playerPrefab, new Vector3(-1f, 0f), round, eUnitSide.Allied), actor => victim = actor);
            yield return ShowFixture<RedDragon>(prefab, ControlledData(prefab, Vector3.zero, round, eUnitSide.Enemy), actor => dragon = actor);
            HoldAutomaticActions(victim);
            HoldAutomaticActions(dragon);
            NoDefence(victim);
            float before = victim.stat.Hp;
            dragon.BeginSkill();
            yield return Until(() => dragon.WarningPosition.HasValue, "Natural dragon skill warning");
            Require(dragon.WarningSeconds >= 0.6f && victim.stat.Hp == before, "The warning arrived after damage or left no reaction time.");
            Vector3 marked = dragon.WarningPosition.Value;
            victim.SetMovement(-1);
            yield return Until(() => Mathf.Abs(victim.transform.position.x - marked.x) > dragon.WarningRadius + 0.05f,
                "Normal player movement leaves the marked area", 1f);
            victim.NMove();
            yield return Until(() => !dragon.WarningPosition.HasValue, "Marked strike resolves");
            Require(victim.stat.Hp == before, "The marked strike followed an escaped target.");
            yield return Until(() => dragon.WarningPosition.HasValue, "Second marked strike");
            yield return Until(() => victim.stat.Hp < before, "Remaining in the marked area takes damage");
            yield return Until(() => dragon.WarningPosition.HasValue, "Third marked strike");
            float afterHit = victim.stat.Hp;
            dragon.stat.Hp = 0f;
            yield return new WaitForSeconds(1.2f);
            Require(victim.stat.Hp == afterHit && !dragon.WarningPosition.HasValue, "A defeated caster left a damaging pending strike or warning.");
        }
        finally { LunaSpawn.CancelRound(round); }
        yield return RetireFixture(round);
        EndCase("The actual native skill warned before damage, missed an escaped target, hit a stationary target, and cancelled on caster death.");
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
        yield return RepairedFlowCaptures();
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
        CaptureDesignPanel("dungeon-list", map, dungeonView, dungeonCards + "Dungeon01", dungeonCards + "Dungeon02");
        var dungeonScroll = map.transform.Find(dungeonView).GetComponent<ScrollRect>();
        dungeonScroll.horizontalNormalizedPosition = 1f;
        Canvas.ForceUpdateCanvases();
        CaptureDesignPanel("dungeon-list-last", map, dungeonView, dungeonCards + "Dungeon05_old");
        dungeonScroll.horizontalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
        var firstBoss = map.GetComponentsInChildren<StageButton>().Single(row => row.name == "Dungeon01");
        firstBoss.GetComponent<Button>().onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.DUNGEONINFO) != null, "Boss preview render");
        CaptureDesignPanel("boss-briefing", NativeUi(UI_TYPE.DUNGEONINFO), "BackGround/StageBriefing", "BackGround/StartButton");
        yield return CloseNativeUi(UI_TYPE.DUNGEONINFO);
        yield return CloseNativeUi(UI_TYPE.SELECTSTAGE);
        var opening = LunaDataTable.Instance.StageTable["Stage01-01"];
        bool cleared = opening.isClear;
        opening.isClear = false;
        try
        {
            yield return LoadStage("Stage01-01");
            HoldRoundActors(LunaScenes.CurrentGame);
            yield return null;
            var hud = Active<UIPlayer>();
            CaptureDesignPanel("battle-guidance", hud, "BattleGuidance", "Button_0/SummonStatus", "Button_5/SummonStatus",
                "Button_0/Summons/Text", "Button_5/Summons/Text");
            var game = (BasicDungeon)LunaScenes.CurrentGame;
            UnitLogic.From(game.gameAllyTower).stat.Hp = 0f;
            yield return Until(() => Active<UIResult>() != null, "Result detail render");
            CaptureDesignPanel("defeat-detail", Active<UIResult>(), "ResultBoard/ResultText");
        }
        finally { opening.isClear = cleared; }
        yield return LoadDungeonFixture("Dungeon01");
        var dragon = (RedDragon)((BossDungeon)LunaScenes.CurrentGame).Boss;
        dragon.BeginSkill();
        yield return Until(() => dragon.WarningPosition.HasValue, "Boss warning render");
        yield return null;
        CaptureDesignPanel("boss-warning", Active<UIPlayer>(), "AttackWarning", "AttackWarning/WarningText");
        yield return EnsureFixtureLobby();
    }

    private IEnumerator RepairedFlowCaptures()
    {
        var oldSize = new Vector2Int(Screen.width, Screen.height);
        try
        {
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(1024, 768) })
            {
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.2f);
                yield return OpenNativeUi(UI_TYPE.SELECTSTAGE);
                var map = NativeUi(UI_TYPE.SELECTSTAGE);
                yield return null;
                map.transform.Find("Stage_Tab/Stage_ScrollView").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
                Canvas.ForceUpdateCanvases();
                map.GetComponentsInChildren<StageButton>().Single(row => row.name == "Stage01-05")
                    .GetComponent<Button>().onClick.Invoke();
                yield return Until(() => NativeUi(UI_TYPE.STAGEINFO) != null, "Side-by-side map render");
                var side = NativeUi(UI_TYPE.STAGEINFO);
                var viewport = (RectTransform)map.transform.Find("Stage_Tab/Stage_ScrollView/Stage_Viewport");
                var board = (RectTransform)side.transform.Find("BackGround");
                var corners = new Vector3[4]; viewport.GetWorldCorners(corners);
                var detailCorners = new Vector3[4]; board.GetWorldCorners(detailCorners);
                Require(corners[2].x < detailCorners[0].x, "The map list overlaps its detail board.");
                CaptureFlowPanel("map-" + size.x + "x" + size.y, side, size.x, size.y,
                    "BackGround", "BackGround/StageBriefing", "Monster_ScrollView", "Item_ScrollView");
                yield return CloseNativeUi(UI_TYPE.STAGEINFO);
                map.transform.Find("Dungeon_Tab").GetComponent<Button>().onClick.Invoke();
                CaptureFlowPanel("dungeons-" + size.x + "x" + size.y, map, size.x, size.y,
                    "Dungeon_Tab/Dungeon_ScrollView");
                yield return CloseNativeUi(UI_TYPE.SELECTSTAGE);
                foreach (int count in new[] { 5, 1, 10 })
                {
                    var profile = LunaDataTable.Instance.playerData;
                    profile.Gold = 10000;
                    profile.IsBegining = count == 5;
                    yield return OpenNativeUi(UI_TYPE.GAMESHOP);
                    var shop = (UIGameShop)NativeUi(UI_TYPE.GAMESHOP);
                    if (count == 1) shop.OnClickSummon(); else shop.OnClickSummons();
                    yield return Until(() => NativeUi(UI_TYPE.LOTTERY) != null, "Draw reveal render");
                    var reveal = (UILottery)NativeUi(UI_TYPE.LOTTERY);
                    Canvas.ForceUpdateCanvases();
                    var content = reveal.transform.Find("LotteryScroll View/LotteryViewport/LotteryContent");
                    Require(content.childCount == count && !shop.gameObject.activeInHierarchy, "Wrong reveal count or shop overlap.");
                    CaptureFlowPanel("draw-" + count + "-" + size.x + "x" + size.y, reveal, size.x, size.y,
                        "RevealTitle", "RevealContinue");
                    yield return new WaitForSecondsRealtime(0.6f);
                    reveal.Continue();
                    yield return Until(() => !LunaUI.IsOpenOrLoading(UI_TYPE.LOTTERY), "Close draw reveal");
                    yield return CloseNativeUi(UI_TYPE.GAMESHOP);
                }
            }
        }
        finally { Screen.SetResolution(oldSize.x, oldSize.y, FullScreenMode.Windowed); }
    }

    private void CaptureFlowPanel(string name, UILuna form, int width, int height, params string[] paths)
    {
        BeginCase("render-flow", name);
        renderBoundsFailures.Clear();
        var texture = CaptureOffscreen(width, height, (w, h) =>
        {
            foreach (string path in paths) MeasureUiRect(form, path, w, h);
            if (form is UILottery)
            {
                var viewport = (RectTransform)form.transform.Find("LotteryScroll View/LotteryViewport");
                foreach (RectTransform card in form.transform.Find("LotteryScroll View/LotteryViewport/LotteryContent"))
                {
                    MeasureUiRect(card, "reveal/" + card.name, w, h);
                    RequireInsideViewport(card, viewport, true);
                }
            }
            if (form is UIStageInfo || form is UIDungeonInfo)
                foreach (string path in new[] { "Monster_ScrollView", "Item_ScrollView" })
                {
                    var scroll = form.transform.Find(path).GetComponent<ScrollRect>();
                    foreach (RectTransform icon in scroll.content)
                    {
                        RequireInsideViewport(icon, scroll.viewport, false);
                        RequireInsideViewport((RectTransform)icon.Find("Button"), scroll.viewport, false);
                    }
                }
        });
        try
        {
            File.WriteAllBytes(Path.Combine(PlayerOutputDirectory, "render-flow-" + name + ".png"), texture.EncodeToPNG());
            Require(renderBoundsFailures.Count == 0, "Repaired flow exceeds screen bounds: " + string.Join("; ", renderBoundsFailures));
            EndCase("Actual selection/purchase callbacks rendered the repaired flow at the requested aspect ratio.");
        }
        finally { Destroy(texture); }
    }

    private void RequireInsideViewport(RectTransform item, RectTransform viewport, bool horizontal)
    {
        var corners = new Vector3[4];
        item.GetWorldCorners(corners);
        foreach (var world in corners)
        {
            var point = viewport.InverseTransformPoint(world);
            Require(point.y >= viewport.rect.yMin - 0.1f && point.y <= viewport.rect.yMax + 0.1f &&
                (!horizontal || point.x >= viewport.rect.xMin - 0.1f && point.x <= viewport.rect.xMax + 0.1f),
                item.name + " is clipped by its viewport.");
        }
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

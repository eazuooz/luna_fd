#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    [Serializable]
    private sealed class BalanceSample
    {
        public string stage, policy, outcome;
        public int seed, attempt, level, floors, deaths, summons, skills, peakEnemies, goldReward, wallet, phaseReinforcements, dodgeInputs;
        public float seconds, allyHealth, objectiveHealth, lowestPlayerHealth = 1f, firstContact = -1f;
    }

    private static readonly int[] TacticalOrder = { 1, 2, 0, 3, 4, 5 };
    private static readonly int[] CasualOrder = { 0, 1, 2, 3, 4, 5 };

    private IEnumerator BalancePlaythrough()
    {
        var profile = LunaDataTable.Instance.playerData;
        string policy = Environment.GetEnvironmentVariable("LUNA_PLAY_POLICY") ?? "tactical";
        Require(policy == "tactical" || policy == "casual" || policy == "idle", "Unknown playthrough policy: " + policy);
        int seed = int.TryParse(Environment.GetEnvironmentVariable("LUNA_PLAY_SEED"), out int specified) ? specified : 421;
        bool naturalTime = Environment.GetEnvironmentVariable("LUNA_PLAY_REALTIME") == "1";
        bool progression = Environment.GetEnvironmentVariable("LUNA_PLAY_PROGRESSION") == "1";
        var random = UnityEngine.Random.state;
        float oldScale = Time.timeScale, oldCapture = Time.captureDeltaTime;
        try
        {
            // The accelerated probe still runs the real frame/FSM/physics path at small game-time steps.
            if (!naturalTime) Time.captureDeltaTime = 1f / 60f;
            if (progression) Require(LunaEconomy.TryDraw(true, out _), "Fresh profile could not collect its starter pack.");
            var definitions = LunaStageTables.Stages.GetAllDataRows().OrderBy(row => row.Id).ToArray();
            foreach (var stage in definitions)
            {
                if (!Includes("playthrough", stage.Key)) continue;
                yield return EnsureFixtureLobby();
                UnityEngine.Random.InitState(seed + stage.Id);
                if (progression) Require(LunaStageTables.IsUnlocked(stage, profile), "Earned progression has not unlocked " + stage.Key);
                if (progression && stage.Kind == LunaStageKind.Basic)
                {
                    while (profile.LunaLevel < stage.RecommendedLevel &&
                        profile.LunaLevelUp(LunaEconomy.UnitLevelCost(profile.LunaLevel))) { }
                    if (stage.Id == 21)
                    {
                        profile.UnitNamesInDeck[3] = "Roni";
                        LunaSession.Current.RefreshDeckPrefabs();
                    }
                    // Buy selective upgrades from earned gold. Repeated cards improve the units actually played.
                    var units = LunaDataTable.Instance.UnitTable;
                    foreach (string key in new[] { "Alice", "Lene", "PorongSword" })
                    {
                        var unit = units[key];
                        int target = 1 + (stage.Id - 1) / 10;
                        if (unit.Level < target) LunaEconomy.TryLevelUp(profile, unit);
                        LunaEconomy.TryRankUp(profile, unit);
                    }
                    if (stage.Id % 5 == 0 && profile.Gold >= LunaEconomy.BundleDrawGold + 1600)
                        Require(LunaEconomy.TryDraw(true, out _), "Paid pack could not be purchased with earned gold.");
                }
                else if (!progression) profile.LunaLevel = stage.RecommendedLevel;
                int attempts = progression && stage.Kind != LunaStageKind.Endless ? 2 : 1;
                for (int attempt = 1; attempt <= attempts; attempt++)
                {
                    int roundSeed = seed + stage.Id + (attempt - 1) * 997;
                    if (attempt > 1)
                    {
                        // A retry may buy one more Luna level with the existing wallet.
                        profile.LunaLevelUp(LunaEconomy.UnitLevelCost(profile.LunaLevel));
                        UnityEngine.Random.InitState(roundSeed);
                    }
                    LunaDataTable.Instance.SetLunaDatasLevel(profile.LunaLevel);
                    BeginCase("playthrough", attempt == 1 ? stage.Key : stage.Key + "/retry");
                    Time.timeScale = naturalTime ? 1.5f : 5f;
                    if (stage.Kind == LunaStageKind.Basic) yield return LoadStage(stage.Key);
                    else
                    {
                        LunaSession.Current.selectDungeon = Resources.Load<GameObject>("Prefabs/11Dungeons/" + stage.Key);
                        LunaSession.Current.SelectDungeonString = stage.Key;
                        LunaScenes.Load("Dungeon");
                        yield return Until(() => LunaScenes.CurrentGame?.StageKey == stage.Key &&
                            !LunaScenes.IsTransitioning && Active<UIPlayer>() != null && Active<UISkill>() != null, "Playable dungeon " + stage.Key);
                    }
                    var round = LunaScenes.CurrentGame;
                    var sample = new BalanceSample { stage = round.StageKey, policy = policy, seed = roundSeed,
                        attempt = attempt, level = profile.LunaLevel };
                    Time.timeScale = naturalTime ? 1.5f : 5f;
                    yield return ObserveBattle(round, sample, profile);
                    CheckOwnership();
                    EndCase("Played with normal movement, summon costs and skill cooldowns. Outcome and pressure sampled from native actors.");
                    if (!progression || stage.Kind == LunaStageKind.Endless || sample.outcome == "win") break;
                    Require(attempt < attempts, "Earned progression could not clear after a paid retry: " + JsonUtility.ToJson(sample));
                }
            }
        }
        finally { Time.captureDeltaTime = oldCapture; Time.timeScale = oldScale; UnityEngine.Random.state = random; }
    }

    private IEnumerator ObserveBattle(LunaGame round, BalanceSample sample, PlayerData profile)
    {
        string policy = sample.policy;
        int before = profile.Gold;
        int previousLife = round.PlayerEntityId;
        var deadLives = new HashSet<int>();
        float nextDecision = 0f, deadline = Time.time + 250f;
        var basic = round as BasicDungeon;
        var endless = round as EndlessDungeon;
        var boss = round as BossDungeon;
        var reinforcementIds = new HashSet<int>();
        var hud = Active<UIPlayer>();
        var skillHud = Active<UISkill>();
        float decisionInterval = policy == "casual" ? 1.1f : 0.2f;
        while (Active<UIResult>() == null && Time.time < deadline && round.StageCount < 180f &&
            (endless == null || endless.dungeonLevel < 12))
        {
            Require(round.IsActive && round.FailureMessage == null, "Round stopped unexpectedly: " + round.FailureMessage);
            var player = round.Player as PlayerLogic;
            if (player != null && player.stat.Hp <= 0f) deadLives.Add(player.ID);
            if (round.PlayerEntityId != previousLife)
            {
                deadLives.Add(previousLife);
                previousLife = round.PlayerEntityId;
            }
            if (!round.isStageStop && round.StageCount >= nextDecision)
            {
                nextDecision = round.StageCount + decisionInterval;
                var enemies = UnitLogic.ActiveUnits.Where(unit => unit.Data != null && unit.Data.RoundId == round.RoundId &&
                    unit.UnitSide == eUnitSide.Enemy && LunaCombat.IsInCombat(unit)).ToArray();
                if (boss != null)
                    foreach (var enemy in enemies) if (enemy != boss.Boss) reinforcementIds.Add(enemy.ID);
                sample.peakEnemies = Mathf.Max(sample.peakEnemies, enemies.Count(unit => unit.hitPriority != eHitPriority.TOWER));
                if (player != null && player.stat.Hp > 0f)
                {
                    float health = HealthFraction(player);
                    sample.lowestPlayerHealth = Mathf.Min(sample.lowestPlayerHealth, health);
                    if (health < 0.99f && sample.firstContact < 0f) sample.firstContact = round.StageCount;
                    if (policy != "idle") DriveBattle(round, player, hud, skillHud, enemies, policy == "tactical", sample);
                }
                sample.allyHealth = HealthFraction(basic != null ? UnitLogic.From(basic.gameAllyTower) : player);
                var objective = basic != null ? UnitLogic.From(basic.gameEnemyTower) :
                    boss?.Boss;
                sample.objectiveHealth = HealthFraction(objective);
            }
            yield return null;
        }
        if (round.Player is PlayerLogic currentPlayer) currentPlayer.NMove();
        sample.deaths = deadLives.Count;
        sample.seconds = round.StageCount;
        sample.floors = endless?.dungeonLevel ?? 0;
        sample.outcome = Active<UIResult>() != null ? (round.isStageWin ? "win" : "loss") : sample.floors >= 12 ? "floor-cap" : "time-cap";
        yield return EnsureFixtureLobby();
        sample.goldReward = profile.Gold - before;
        sample.wallet = profile.Gold;
        sample.phaseReinforcements = reinforcementIds.Count;
        current.observations.Add(JsonUtility.ToJson(sample));
        Require(sample.seconds > 0f && float.IsFinite(sample.seconds), "Invalid playthrough duration.");
        if (endless != null)
            Require(sample.goldReward >= sample.floors * round.Definition.Row.Gold, "Completed Endless floors did not pay at result or exit.");
    }

    private static float HealthFraction(UnitLogic actor) =>
        actor != null ? Mathf.Clamp01(actor.stat.Hp / Mathf.Max(1f, actor.HealthBarHpPoint)) : 0f;

    private static void DriveBattle(LunaGame round, PlayerLogic player, UIPlayer hud, UISkill skillHud,
        UnitLogic[] enemies, bool tactical, BalanceSample sample)
    {
        float x = player.transform.position.x;
        var target = enemies.OrderBy(unit => Mathf.Abs(unit.transform.position.x - x)).FirstOrDefault();
        float attackRange = Mathf.Max(0.2f, player.stat.AttackRange * 0.92f);
        var cover = tactical && HealthFraction(player) < 0.38f &&
            UnitLogic.ActiveUnits.Any(unit => unit != player && unit.Data != null && unit.Data.RoundId == round.RoundId &&
                unit.UnitSide == eUnitSide.Allied && unit.hitPriority != eHitPriority.TOWER && LunaCombat.IsInCombat(unit) &&
                unit.transform.position.x > x + 0.15f);
        int escape = 0;
        if (tactical && round is BossDungeon boss && boss.Boss is Rugani rugani &&
            rugani.StrikeMarker is Vector3 mark && Mathf.Abs(mark.x - x) < 1.0f)
        {
            escape = x > mark.x + 0.1f && x < Defined.EnemyTowerPosition.x - 1f ? 1 : -1;
            sample.dodgeInputs++;
        }
        if (escape != 0) player.SetMovement(escape);
        else if (target == null) player.NMove();
        else
        {
            float distance = target.transform.position.x - x;
            if (cover) player.SetMovement(Mathf.Abs(distance) < attackRange ? -1 : 0);
            else player.SetMovement(Mathf.Abs(distance) <= attackRange ? 0 : Math.Sign(distance));
        }

        if (hud != null)
        {
            int[] order = tactical ? TacticalOrder : CasualOrder;
            foreach (int slot in order)
            {
                if (tactical && sample.summons == 0 && round.StageCount < 3f && slot != 4) continue;
                if (slot >= 4 && sample.summons > 0 && round.Roster.GetSlot(tactical ? 1 : 0).LiveUnit == null &&
                    round.Roster.GetSlot(tactical ? 1 : 0).Prefab != null) continue;
                if (!round.Roster.CanSummon(slot)) continue;
                int old = round.Roster.GetSlot(slot).EntityId;
                hud.OnClickSummon(slot);
                if (round.Roster.GetSlot(slot).EntityId != old) sample.summons++;
                break;
            }
        }
        if (skillHud == null || player.Abilities == null) return;
        var skills = player.Abilities.Loadout.Skills;
        foreach (var skill in skills.OrderBy(skill => skill.name == "Healing" ? 0 : 1))
        {
            if (!player.Abilities.Ready(skill) || round.Roster.Mana < skill.mana) continue;
            bool healing = skill.name == "Healing";
            if (healing ? HealthFraction(player) > (tactical ? 0.66f : 0.4f) : target == null ||
                Mathf.Abs(target.transform.position.x - x) > (tactical ? 5f : 3f)) continue;
            if (!tactical && skill.name == "Thunder") continue;
            if (player.Abilities.TryUse(skill, skill.name == "Thunder" ? target.transform.position : (Vector3?)null))
            { sample.skills++; break; }
        }
    }
}
#endif

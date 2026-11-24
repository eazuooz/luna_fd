using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luna.Gameplay
{
    public static class LunaEconomy
    {
        public const int StartingGold = 700;
        public const int SingleDrawGold = 420;
        public const int BundleDrawGold = 3800;
        public const int HeroChance = 35;
        public const int DeckFocusChance = 70;
        public const int UnitMaxLevel = 20;
        public const int SkillMaxLevel = 20;
        public static int SkillStoneCost(int level) => 2 * Mathf.Clamp(level, 1, SkillMaxLevel);
        public static bool TryLevelUpSkill(SkillData skill)
        {
            if (skill == null || skill.level >= SkillMaxLevel ||
                !LunaDataTable.Instance.itemTable.TryGetValue(1001, out var stones) || stones.itemCount < SkillStoneCost(skill.level)) return false;
            stones.itemCount -= SkillStoneCost(skill.level);
            skill.level++;
            return true;
        }

        public static bool TryEnhanceEquipment(EquipDesignItemData design, out string message)
        {
            var data = LunaDataTable.Instance;
            var rule = design == null ? null : LunaEquipmentEnhancements.Find(design.itemNumber);
            if (rule == null || !data.itemTable.TryGetValue(rule.Id, out var registered) || registered != design)
            {
                message = "강화할 수 없는 장비입니다.";
                return false;
            }
            if (design.EnhancementRank >= rule.Rank)
            {
                message = "강화를 완료한 장비입니다.";
                return false;
            }
            if (LunaRuntimeData.DeveloperSessionActive)
            {
                message = "개발자 모드를 종료한 뒤 강화할 수 있습니다.";
                return false;
            }
            if (!data.itemTable.TryGetValue(LunaEquipmentEnhancements.StoneId, out var stones) || stones.itemCount < rule.StoneCost)
            {
                message = "장비 강화석이 부족합니다.";
                return false;
            }

            int previousRank = design.EnhancementRank;
            int previousStones = stones.itemCount;
            bool saved = false;
            try
            {
                design.EnhancementRank = rule.Rank;
                stones.itemCount -= rule.StoneCost;
                saved = LunaRuntimeData.SaveEquipmentEnhancement();
            }
            finally
            {
                if (!saved)
                {
                    design.EnhancementRank = previousRank;
                    stones.itemCount = previousStones;
                }
            }
            message = saved ? "장비 강화가 완료되었습니다." : "저장할 수 없어 강화를 취소했습니다.";
            return saved;
        }

        public static int UnitLevelCost(int level) => 550 + 150 * Mathf.Clamp(level, 1, UnitMaxLevel);
        public static int RankCardCost(eUltState rank) => (int)rank < 5 ? (int)rank + 1 : 2 * (int)rank - 3;
        public static int RankStoneCost(eUltState rank) => Mathf.Max(0, (int)rank - 4) * 2;
        public static float RankCombatMultiplier(eUltState rank) => 1f + 0.05f * (Mathf.Clamp((int)rank, 1, 7) - 1);
        public static float TowerHealthMultiplier(int level) => 1f + 0.12f * (Mathf.Clamp(level, 1, 10) - 1);

        public static bool TryLevelUp(PlayerData profile, ObjectData unit)
        {
            if (unit.Level >= UnitMaxLevel || !profile.TrySpendGold(UnitLevelCost(unit.Level))) return false;
            unit.LevelUp();
            return true;
        }

        public static bool TryRankUp(PlayerData profile, ObjectData unit)
        {
            if (unit.ultState >= eUltState.ULT2) return false;
            int cards = RankCardCost(unit.ultState);
            int stones = RankStoneCost(unit.ultState);
            if (unit.cardCount < cards || profile.StarStone < stones) return false;
            unit.cardCount -= cards;
            profile.StarStone -= stones;
            unit.RankUp();
            return true;
        }

        internal static void GrantGold(PlayerData player, int count, int value)
        {
            int credit = (int)Math.Min(int.MaxValue - player.Gold, (long)count * value);
            player.Gold += credit;
            player.RewardGold = (int)Math.Min(int.MaxValue, (long)player.RewardGold + credit);
        }

        public static bool TryDraw(bool bundle, out string message)
        {
            var data = LunaDataTable.Instance;
            var profile = data.playerData;
            bool starter = bundle && profile.IsBegining;
            int cost = starter ? 0 : bundle ? BundleDrawGold : SingleDrawGold;
            if (profile.Gold < cost)
            { message = "골드가 부족합니다."; return false; }

            var heroes = AvailableNames(data.heroTable, data.UnitTable);
            var soldiers = AvailableNames(data.soliderTable, data.UnitTable);
            if (heroes.Length < 5 || soldiers.Length == 0)
                throw new InvalidOperationException("The card shop has an incomplete unit catalogue.");
            var deckHeroes = profile.UnitNamesInDeck.Where(heroes.Contains).Distinct().ToArray();
            var deckSoldiers = profile.UnitNamesInDeck.Where(soldiers.Contains).Distinct().ToArray();
            var picks = new List<string>();
            if (starter)
            {
                picks.AddRange(deckHeroes.Take(4));
                var remaining = heroes.Where(name => !picks.Contains(name)).ToList();
                while (picks.Count < 5)
                {
                    int index = UnityEngine.Random.Range(0, remaining.Count);
                    picks.Add(remaining[index]);
                    remaining.RemoveAt(index);
                }
            }
            else
            {
                int count = bundle ? 10 : 1;
                for (int i = 0; i < count; i++)
                {
                    bool hero = bundle && i == 0 || UnityEngine.Random.Range(0, 100) < HeroChance;
                    var all = hero ? heroes : soldiers;
                    var deck = hero ? deckHeroes : deckSoldiers;
                    var pool = deck.Length != 0 && UnityEngine.Random.Range(0, 100) < DeckFocusChance ? deck : all;
                    picks.Add(pool[UnityEngine.Random.Range(0, pool.Length)]);
                }
            }

            var awards = picks.GroupBy(name => name).ToArray();
            var newUnits = awards.Where(group => !LunaUIPresentation.Owned(data.UnitTable[group.Key])).Select(group => group.Key).ToArray();
            if (awards.Any(group => data.UnitTable[group.Key].cardCount > int.MaxValue - group.Count()))
            { message = "보유 카드 한도에 도달했습니다."; return false; }

            profile.TrySpendGold(cost);
            if (starter) profile.IsBegining = false;
            foreach (var group in awards) data.UnitTable[group.Key].cardCount += group.Count();
            if (!LunaRuntimeData.SaveNow())
            {
                profile.Gold += cost;
                if (starter) profile.IsBegining = true;
                foreach (var group in awards) data.UnitTable[group.Key].cardCount -= group.Count();
                message = "저장할 수 없어 소환을 취소했습니다.";
                return false;
            }

            data.LotteryResult.Clear();
            data.LotteryResult.AddRange(picks);
            data.LotteryNewUnits.Clear();
            foreach (string name in newUnits) data.LotteryNewUnits.Add(name);
            message = string.Empty;
            return true;
        }

        private static string[] AvailableNames(string[] names, Dictionary<string, ObjectData> units) =>
            names.Where(name => !string.IsNullOrEmpty(name) && units.ContainsKey(name)).Distinct().ToArray();
    }
}

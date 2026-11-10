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
            message = string.Empty;
            return true;
        }

        private static string[] AvailableNames(string[] names, Dictionary<string, ObjectData> units) =>
            names.Where(name => !string.IsNullOrEmpty(name) && units.ContainsKey(name)).Distinct().ToArray();
    }
}

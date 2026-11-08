using System.Collections.Generic;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Non-stacking party auras remain active until the last living source leaves.</summary>
    public static class HeroPartyEffects
    {
        private static readonly HashSet<HeroUnitLogic> sources = new HashSet<HeroUnitLogic>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { sources.Clear(); }
        public static void Register(HeroUnitLogic source) { sources.Add(source); Refresh(); }
        public static void Unregister(HeroUnitLogic source) { sources.Remove(source); Refresh(); }
        public static bool Has<T>(eUltState rank, int roundId) where T : HeroUnitLogic
        {
            foreach (var source in sources)
            {
                if (source == null || !(source is T) || !source.IsRankReady || !source.isActiveAndEnabled || source.Data == null
                    || source.Data.RoundId != roundId) continue;
                var status = source;
                if (status != null && status.UnitSide == eUnitSide.Allied && LunaCombat.IsInCombat(status) && status.ultState >= rank) return true;
            }
            return false;
        }
        public static float DamageReductionForRound(int roundId) => Has<Dren>(eUltState.ULT1, roundId) ? 0.1f : 0f;
        public static float GoldBonusForRound(int roundId) => Has<Goldy>(eUltState.ULT2, roundId) ? 10f : Has<Goldy>(eUltState.ULT1, roundId) ? 9f
            : Has<Goldy>(eUltState.STAR5, roundId) ? 8f : Has<Goldy>(eUltState.STAR3, roundId) ? 7f : Has<Goldy>(eUltState.STAR1, roundId) ? 6f : 0f;
        public static void Refresh()
        {
            sources.RemoveWhere(source => source == null);
        }
    }
}

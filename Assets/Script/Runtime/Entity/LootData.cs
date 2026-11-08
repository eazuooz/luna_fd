using UnityEngine;

namespace Luna.Gameplay
{
    public sealed class LootData : LunaEntityData
    {
        public int ItemId;
        public int Count = 1;
        public int CoinValue = 100;
        public bool Boss;
        public static LootData Create(int itemId, int count, Vector3 position, bool boss = false, int roundId = 0)
        {
            var data = Create<LootData>(); data.ItemId = itemId; data.Count = Mathf.Max(1, count);
            data.Position = position; data.Boss = boss; data.RoundId = roundId;
            return data;
        }
        public override void Clear() { base.Clear(); ItemId = 0; Count = 1; CoinValue = 100; Boss = false; }
    }
}

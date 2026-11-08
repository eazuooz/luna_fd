namespace Luna.Gameplay
{
    public sealed class DropCoinLogic : DropItemLogic
    {
        protected override void Grant(LootData loot)
        {
            if (GrantRoundLoot(loot)) return;
            var player = LunaDataTable.Instance.playerData;
            player.RewardGold += loot.Count;
            player.Gold += loot.Count * loot.CoinValue;
        }
    }
}

namespace Luna.Gameplay
{
    public sealed class DropCoinLogic : DropItemLogic
    {
        protected override void Grant(LootData loot)
        {
            if (GrantRoundLoot(loot)) return;
            var player = LunaDataTable.Instance.playerData;
            LunaEconomy.GrantGold(player, loot.Count, loot.CoinValue);
        }
    }
}

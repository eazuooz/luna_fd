using GameFramework;
using GameFramework.Event;

namespace Luna.Gameplay
{
    /// <summary>A completed player life. The current battle procedure decides whether and when to respawn.</summary>
    public sealed class PlayerDiedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayerDiedEventArgs).GetHashCode();
        public override int Id => EventId;
        public int EntityId { get; private set; }
        public int RoundId { get; private set; }
        public int AllyTowerId { get; private set; }
        public float RespawnDelay { get; private set; }
        public static PlayerDiedEventArgs Create(int entityId, int roundId, int allyTowerId, float respawnDelay)
        {
            var args = ReferencePool.Acquire<PlayerDiedEventArgs>();
            args.EntityId = entityId; args.RoundId = roundId; args.AllyTowerId = allyTowerId; args.RespawnDelay = respawnDelay;
            return args;
        }
        public override void Clear() { EntityId = RoundId = AllyTowerId = 0; RespawnDelay = 0f; }
    }
}

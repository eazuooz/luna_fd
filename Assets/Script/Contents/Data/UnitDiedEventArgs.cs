using GameFramework;
using GameFramework.Event;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Immutable snapshot of a completed combat life, never a pooled-object reference.</summary>
    public sealed class UnitDiedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UnitDiedEventArgs).GetHashCode();
        public override int Id => EventId;
        public int EntityId { get; private set; }
        public int RoundId { get; private set; }
        public eUnitSide Side { get; private set; }
        public eHitPriority Priority { get; private set; }
        public Vector3 Position { get; private set; }
        public static UnitDiedEventArgs Create(int entityId, int roundId, eUnitSide side, eHitPriority priority, Vector3 position)
        {
            var data = ReferencePool.Acquire<UnitDiedEventArgs>();
            data.EntityId = entityId; data.RoundId = roundId; data.Side = side; data.Priority = priority; data.Position = position;
            return data;
        }
        public override void Clear() { EntityId = RoundId = 0; Side = default; Priority = default; Position = Vector3.zero; }
    }
}

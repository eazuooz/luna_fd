using UnityEngine;

namespace Luna.Gameplay
{
    public class EffectData : LunaEntityData
    {
        public bool FollowOwner;
        public Vector3 Offset;
        public string Animation = "normal";
        public float Lifetime = 15f;
        public bool Loop;
        public float ScaleMultiplier = 1f;
        public static EffectData Create(Vector3 position, int ownerId = 0, bool attach = false, int roundId = 0)
        {
            var data = Create<EffectData>();
            data.Position = position; data.OwnerId = ownerId; data.FollowOwner = attach; data.RoundId = roundId;
            var owner = LunaSpawn.Find<EntityLogicBase>(ownerId);
            if (owner != null)
            {
                data.Offset = position - owner.CachedTransform.position;
                if (roundId == 0) data.RoundId = owner.SpawnData.RoundId;
            }
            return data;
        }
        public override void Clear()
        {
            base.Clear(); FollowOwner = Loop = false; Offset = Vector3.zero;
            Animation = "normal"; Lifetime = 15f; ScaleMultiplier = 1f;
        }
    }
    public sealed class HealthBarData : LunaEntityData
    {
        public static HealthBarData Create(int ownerId, Vector3 position)
        {
            var data = Create<HealthBarData>(); data.OwnerId = ownerId; data.Position = position;
            var owner = LunaSpawn.Find<EntityLogicBase>(ownerId);
            if (owner != null) data.RoundId = owner.SpawnData.RoundId;
            return data;
        }
    }
    public sealed class DamageTextData : LunaEntityData
    {
        public float Value;
        public bool Critical;
        public bool Recovery;
        public override void Clear() { base.Clear(); Value = 0f; Critical = Recovery = false; }
    }
}

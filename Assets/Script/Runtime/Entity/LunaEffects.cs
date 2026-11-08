using UnityEngine;

namespace Luna.Gameplay
{
    public static class LunaEffects
    {
        public static int Show(GameObject prefab, Vector3 position, int ownerId = 0, bool attach = false, int roundId = 0)
        {
            if (prefab == null) return 0;
            var data = EffectData.Create(position, ownerId, attach, roundId);
            data.Rotation = prefab.transform.rotation;
            return LunaSpawn.Show<EffectLogic>(prefab, data);
        }
        public static int ShowHealthBar(GameObject prefab, int ownerId, Vector3 position)
        {
            if (prefab == null) return 0;
            return LunaSpawn.Show<HealthBarLogic>(prefab, HealthBarData.Create(ownerId, position));
        }
        public static int ShowDamageText(GameObject prefab, int ownerId, float value, bool critical = false, bool recovery = false)
        {
            var owner = LunaSpawn.Find<EntityLogicBase>(ownerId);
            if (prefab == null || owner == null) return 0;
            var data = LunaEntityData.Create<DamageTextData>();
            data.OwnerId = ownerId; data.RoundId = owner.SpawnData.RoundId;
            data.Position = owner.CachedTransform.position + Vector3.up;
            data.Value = value; data.Critical = critical; data.Recovery = recovery;
            return LunaSpawn.Show<DamageTextLogic>(prefab, data);
        }
    }
}

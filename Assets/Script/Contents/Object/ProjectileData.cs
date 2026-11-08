using System;
using UnityEngine;

namespace Luna.Gameplay
{
    public enum ProjectileKind { Magic, Arrow, Crest }

    /// <summary>Complete launch snapshot, owned by the native projectile from OnShow to OnHide.</summary>
    public sealed class ProjectileData : LunaEntityData
    {
        public int AttackerId;
        public int TargetId;
        public float Damage;
        public bool Critical;
        public bool Skill;
        public float StunSeconds;
        public float Speed;
        public ProjectileKind Kind;
        public GameObject HitEffect;
        public float Lifetime;
        public Vector3 EndPosition;
        public float ArcHeight;
        public float TravelSeconds;

        public static ProjectileData Create(GameObject prefab, int attackerId, int targetId, Vector3 position,
            float damage, bool critical = false, bool skill = false, float stunSeconds = 0f, int roundId = 0)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            UnitView view = prefab.GetComponent<UnitView>();
            if (view == null) throw new ArgumentException("Projectile needs its authored UnitView bindings.", nameof(prefab));
            if (!float.IsFinite(damage) || !float.IsFinite(stunSeconds) || !float.IsFinite(position.x) || !float.IsFinite(position.y) || !float.IsFinite(position.z))
                throw new ArgumentException("Projectile launch values must be finite.");
            var data = LunaEntityData.Create<ProjectileData>();
            data.Position = position;
            data.Rotation = prefab.transform.rotation;
            data.AttackerId = data.OwnerId = attackerId;
            data.TargetId = targetId;
            data.Damage = Mathf.Max(0f, damage);
            data.Critical = critical;
            data.Skill = skill;
            data.StunSeconds = Mathf.Max(0f, stunSeconds);
            data.Speed = Mathf.Max(1f, view.GetFloat("projectileSpeed", 8f));
            data.Kind = (ProjectileKind)Mathf.Clamp((int)view.GetFloat("bulletType"), 0, 2);
            data.HitEffect = view.GetPrefab("hitEffect");
            data.Lifetime = Mathf.Max(0.1f, view.GetFloat("destroyTime", 10f));
            data.ArcHeight = Mathf.Max(0f, view.GetFloat("height", 2f));
            data.TravelSeconds = Mathf.Max(0.05f, view.GetFloat("moveTime", 2f));
            UnitLogic attacker = UnitLogic.Find(attackerId);
            UnitLogic target = UnitLogic.Find(targetId);
            data.RoundId = roundId != 0 ? roundId : attacker != null ? attacker.SpawnData.RoundId : 0;
            data.EndPosition = target != null ? target.transform.position : position;
            return data;
        }

        public override void Clear()
        {
            base.Clear();
            AttackerId = TargetId = 0;
            Damage = StunSeconds = Speed = Lifetime = ArcHeight = TravelSeconds = 0f;
            Critical = Skill = false;
            Kind = default;
            HitEffect = null;
            EndPosition = Vector3.zero;
        }
    }
}

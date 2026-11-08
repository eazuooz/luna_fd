using System;
using UnityEngine;
using GameFramework;

namespace Luna.Gameplay
{
    public enum UnitSpawnMode { Battle, Lobby, Staged, Preview }

    /// <summary>Complete per-spawn unit state. Prefab definitions remain immutable.</summary>
    public class UnitData : LunaEntityData
    {
        public ObjectData Definition;
        public int Level;
        public eUltState Rank;
        public eUnitSide Side;
        public UnitSpawnMode InitialMode;
        public int TargetId;
        public int AllyTowerId;
        public float BaseMoveSpeed;
        public float? BaseAttackDamageOverride;
        public int AuraLevel;
        public float Lifetime;
        public float InvincibleSeconds;
        public float BerserkSeconds;
        public float TauntRadius;
        public float SummonMoveSpeed;
        public float SummonRange;
        public float SummonDamage;
        public GameObject ImpactEffect;
        public PlayerLoadoutSnapshot PlayerLoadout;
        public float AttackMultiplier = 1f;
        public float ArmourMultiplier = 1f;
        public float HealthMultiplier = 1f;
        public Stat Stats = new Stat();
        public Coefficient Coefficients = new Coefficient();
        public UnitState State = new UnitState();

        public static UnitData CreateFrom(UnitView view, Vector3 position)
        {
            ValidateView(view);
            var data = LunaEntityData.Create<UnitData>();
            try { FillFrom(data, view, position); return data; }
            catch { ReferencePool.Release(data); throw; }
        }

        protected static void ValidateView(UnitView view)
        {
            if (view == null || view.Definition == null) throw new ArgumentException("A unit spawn requires its authored definition.");
        }

        protected static void FillFrom(UnitData data, UnitView view, Vector3 position)
        {
            data.TypeId = LunaUnitBalance.GetRow(view.Definition).Id;
            data.Definition = LunaRuntimeData.ResolveObjectData(view.Definition);
            data.Position = position;
            data.Rotation = view.transform.rotation;
            data.Level = data.Definition.Level;
            data.Rank = data.Definition.ultState;
            data.Side = data.Definition.UnitSide;
            data.InitialMode = UnitSpawnMode.Battle;
            data.BaseMoveSpeed = Mathf.Abs(data.Definition.GetMoveSpeed_Value(1f));
            data.AuraLevel = LunaDataTable.Instance.playerData.AuroraLevel;
            data.SummonMoveSpeed = view.GetFloat("moveSpeed");
            data.SummonRange = view.GetFloat("range");
            data.SummonDamage = view.GetFloat("damage");
            data.ImpactEffect = view.GetPrefab("explosionEffect");
            data.PlayerLoadout = data.Definition.TypeLunaPlayer ? PlayerLoadoutSnapshot.Capture() : null;
        }

        public override void Clear()
        {
            base.Clear();
            Definition = null;
            Level = 0; Rank = default; Side = default; InitialMode = default;
            TargetId = AllyTowerId = 0;
            BaseMoveSpeed = 0f; AuraLevel = 0;
            BaseAttackDamageOverride = null;
            Lifetime = InvincibleSeconds = BerserkSeconds = TauntRadius = 0f;
            AttackMultiplier = ArmourMultiplier = HealthMultiplier = 1f;
            SummonMoveSpeed = SummonRange = SummonDamage = 0f; ImpactEffect = null;
            PlayerLoadout = null;
            Stats = new Stat(); Coefficients = new Coefficient(); State = new UnitState();
        }
    }

    public sealed class HeroData : UnitData
    {
        public new static HeroData CreateFrom(UnitView view, Vector3 position)
        {
            ValidateView(view);
            var data = LunaEntityData.Create<HeroData>();
            try { FillFrom(data, view, position); return data; }
            catch { ReferencePool.Release(data); throw; }
        }
    }
}

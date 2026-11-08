using System.Linq;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Tower combat, including its authored turret, belongs to one native unit/FSM.</summary>
    public sealed class TowerLogic : UnitLogic
    {
        private TowerData towerData;
        private Transform turretMuzzle;
        public override bool HasAutomaticSkill => false;

        protected override void ConfigureUnitForSpawn()
        {
            base.ConfigureUnitForSpawn();
            towerData = Data as TowerData ?? throw new System.ArgumentException("TowerLogic requires TowerData.");
            turretMuzzle = GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "AttackPos");
            if (towerData.Turret != TurretKind.None)
            {
                stat.AttackRange = towerData.TurretRange;
                stat.AttackSpeed = towerData.TurretInterval;
            }
        }

        public override void BeginAttack()
        {
            if (towerData != null && towerData.Turret != TurretKind.None) stat.AttackSpeed = towerData.TurretInterval;
            base.BeginAttack();
        }

        protected override void OnAttackAnimationEvent()
        {
            if (towerData == null || towerData.Turret == TurretKind.None) { base.OnAttackAnimationEvent(); return; }
            UnitLogic target = Target;
            if (target == null || target.IsHiding || target.stat.Hp <= 0f) return;
            Vector3 position = turretMuzzle != null ? turretMuzzle.position : transform.position;
            ProjectileData shot = ProjectileData.Create(towerData.TurretProjectile, ID, target.ID, position,
                towerData.TurretDamage, false, false, towerData.TurretStun, Data.RoundId);
            LunaSpawn.Show<ProjectileLogic>(towerData.TurretProjectile, shot);
            PlayOneSound(towerData.TurretSound);
        }

        protected override void OnUnitHidden()
        {
            towerData = null; turretMuzzle = null;
            base.OnUnitHidden();
        }
    }
}

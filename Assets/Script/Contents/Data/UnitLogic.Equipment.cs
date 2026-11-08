using System;
using System.Collections.Generic;

namespace Luna.Gameplay
{
    public partial class UnitLogic
    {
        private struct EquipmentModifiers
        {
            public float Attack;
            public float Speed;
        }

        private readonly Dictionary<int, EquipmentModifiers> equipmentModifiers = new Dictionary<int, EquipmentModifiers>();
        private float equipmentAttackMultiplier = 1f;
        private float equipmentSpeedMultiplier = 1f;

        /// <summary>Equipment belongs to the source player's current native identity, independently of timed additive buffs.</summary>
        public void SetEquipmentModifiers(int sourceOwnerId, float attackFactor, float speedFactor)
        {
            if (sourceOwnerId == 0) throw new ArgumentOutOfRangeException(nameof(sourceOwnerId));
            if (!float.IsFinite(attackFactor) || attackFactor < 0f) throw new ArgumentOutOfRangeException(nameof(attackFactor));
            if (!float.IsFinite(speedFactor) || speedFactor < 0f) throw new ArgumentOutOfRangeException(nameof(speedFactor));
            if (Data == null || IsHiding) return;
            equipmentModifiers[sourceOwnerId] = new EquipmentModifiers { Attack = attackFactor, Speed = speedFactor };
            RecalculateEquipmentModifiers();
        }

        public void RemoveEquipmentModifiers(int sourceOwnerId)
        {
            if (equipmentModifiers.Remove(sourceOwnerId)) RecalculateEquipmentModifiers();
        }

        private void RecalculateEquipmentModifiers()
        {
            equipmentAttackMultiplier = equipmentSpeedMultiplier = 1f;
            foreach (var modifiers in equipmentModifiers.Values)
            {
                equipmentAttackMultiplier *= modifiers.Attack;
                equipmentSpeedMultiplier *= modifiers.Speed;
            }
            if (Data != null && !IsHiding) StatusAfterReady();
        }

        private void ResetEquipmentModifiers()
        {
            equipmentModifiers.Clear();
            equipmentAttackMultiplier = equipmentSpeedMultiplier = 1f;
        }
    }
}

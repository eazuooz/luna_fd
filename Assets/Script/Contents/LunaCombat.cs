using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>World-space combat lane limits; staged waves and scene transitions are exempt.</summary>
    public static class LunaCombat
    {
        public static bool IsAlive(GameObject unit)
        {
            if (unit == null || !unit.activeInHierarchy) return false;
            var status = unit.GetComponent<UnityGameFramework.Runtime.Entity>()?.Logic as UnitLogic;
            return status != null && status.IsStatusDataLoadComplete && !status.unitState.IsDeath && status.stat.Hp > 0f;
        }

        public static bool IsInCombat(UnitLogic status) => status != null && status.IsStatusDataLoadComplete
            && !status.unitState.IsLobbyMove && !status.unitState.IsLobbyStartMove && !status.unitState.IsLobbyBackMove
            && !status.unitState.IsDeath && status.stat.Hp > 0f;

        public static Vector3 ClampPosition(UnitLogic status, Vector3 position)
        {
            if (!IsInCombat(status)) return position;
            float left = Mathf.Min(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x);
            float right = Mathf.Max(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x);
            if (right - left > 0.1f) position.x = Mathf.Clamp(position.x, left, right);
            return position;
        }

        public static void Move(UnitLogic status, Vector3 offset)
        {
            if (status == null || float.IsNaN(offset.x) || float.IsInfinity(offset.x)
                || float.IsNaN(offset.y) || float.IsInfinity(offset.y)) return;
            status.transform.position = ClampPosition(status, status.transform.position + offset);
        }
    }
}

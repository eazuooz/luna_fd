using System.Linq;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>The authored Skull player owns its axe splash attack and the native player lifecycle.</summary>
    public sealed class SkeletonAxeLogic : PlayerLogic
    {
        protected override void OnAttackAnimationEvent()
        {
            if (Data == null || IsHiding || Target == null) return;
            // The original axe module supplemented the player's ordinary hit.
            // Keep that primary hit (including towers) and then apply its authored non-tower splash.
            base.OnAttackAnimationEvent();
            float direction = UnitSide == eUnitSide.Enemy ? -1f : 1f;
            Vector3 point = transform.position + Vector3.right * direction * stat.AttackRange;
            float damage = Mathf.Max(0f, View.GetFloat("attackDamage", 200f));
            float range = Mathf.Max(0f, View.GetFloat("splashRange", 2f));
            GameObject effect = View.GetPrefab("effect");
            if (effect != null) LunaSpawn.Show<EffectLogic>(effect, EffectData.Create(point, 0, false, Data.RoundId));
            foreach (UnitLogic unit in ActiveUnits.ToArray())
                if (LunaCombat.IsInCombat(unit) && unit.Data.RoundId == Data.RoundId && unit.UnitSide != UnitSide && unit.hitPriority != eHitPriority.TOWER &&
                    Vector3.Distance(point, unit.transform.position) <= range)
                    unit.Attack(gameObject, damage, true, true);
        }
    }
}

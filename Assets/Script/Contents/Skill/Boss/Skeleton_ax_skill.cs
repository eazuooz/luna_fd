using UnityEngine;
using Luna.Gameplay;
public class Skeleton_ax_skill : UnitLogic
{
    protected override void OnAttackAnimationEvent()
    {
        base.OnAttackAnimationEvent();
        if (Target == null) return;
        Vector3 point = transform.position - Vector3.right * stat.AttackRange;
        var effect = View.GetPrefab("effect");
        if (effect != null) LunaEffects.Show(effect, point, roundId: Data.RoundId);
        IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).Attack(gameObject, View.GetFloat("attackDamage", 40f), true, true),
            IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
            unit => IdentifyFunc.IsInDistance(point, unit, View.GetFloat("splashRange", 2f)));
    }
}

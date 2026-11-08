using Luna.Gameplay;
public class PoisonSlime : UnitLogic
{
    protected override void OnAttackAnimationEvent()
    {
        base.OnAttackAnimationEvent();
        var target = Target;
        if (target != null && target.stat.Hp > 0f) target.TakePoisoning(View.GetFloat("attackDamage", 100f), 5f);
    }
}

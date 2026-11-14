using UnityEngine;

namespace Luna.Gameplay
{
    public abstract class EnemySkillLogic : UnitLogic
    {
        private float warningUntil;
        private Vector3 warningPosition;
        public float WarningRadius { get; private set; }
        public string WarningName { get; private set; }
        public float WarningSeconds => Mathf.Max(0f, warningUntil - Time.time);
        public Vector3? WarningPosition => LunaCombat.IsInCombat(this) && WarningSeconds > 0f
            ? warningPosition : (Vector3?)null;
        protected void MarkAttack(Vector3 position, float radius, float delay, string name)
        {
            warningPosition = position;
            WarningRadius = radius;
            WarningName = name;
            warningUntil = Time.time + delay;
        }
        protected void ClearAttackWarning() => warningUntil = 0f;
        protected void RecoverAfterSkill()
        {
            unitState.IsActionDelay = true;
            stat.AttackDelayTime = Mathf.Min(stat.AttackDelayTime, -0.8f);
        }
        protected override void OnShow(object userData)
        {
            ClearAttackWarning();
            base.OnShow(userData);
        }
        protected override void OnUnitDying()
        {
            ClearAttackWarning();
            base.OnUnitDying();
        }
        protected override void OnUnitHidden()
        {
            ClearAttackWarning();
            base.OnUnitHidden();
        }
        protected UnitLogic status => this;
        public override bool HasAutomaticSkill => stat.Stamina > 0f;
        protected override void OnSkillAnimationEvent()
        {
            base.OnSkillAnimationEvent();
            unitState.IsSkillActivaion = false;
            ExecuteSkill();
        }
        protected abstract void ExecuteSkill();
    }
}

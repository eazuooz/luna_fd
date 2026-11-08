using UnityEngine;

namespace Luna.Gameplay
{
    public abstract class EnemySkillLogic : UnitLogic
    {
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

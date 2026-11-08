using Spine.Unity;
using UnityEngine;

namespace Luna.Gameplay
{
    public sealed class BackgroundDemonLogic : EntityLogicBase
    {
        private SkeletonAnimation animation;
        private Vector3 origin;
        private float age;
        protected override void OnInit(object userData) { base.OnInit(userData); animation = GetComponentInChildren<SkeletonAnimation>(true); }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData); origin = transform.position; age = 0f;
            if (animation != null) animation.AnimationState.SetAnimation(0, "idle", true);
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (IsHiding) return;
            age = Mathf.Repeat(age + elapsed, 12f);
            float offset = age < 4f ? age * 0.5f : age < 6f ? 2f : age < 10f ? (10f - age) * 0.5f : 0f;
            transform.position = origin + Vector3.right * offset;
            if (animation != null) animation.Skeleton.ScaleX = age >= 6f && age < 10f ? -1f : 1f;
        }
    }
}

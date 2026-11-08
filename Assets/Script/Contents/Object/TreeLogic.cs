using Spine.Unity;

namespace Luna.Gameplay
{
    public sealed class TreeLogic : EntityLogicBase
    {
        private SkeletonAnimation animation;
        private float remainingDelay;
        private bool started;
        protected override void OnInit(object userData) { base.OnInit(userData); animation = GetComponentInChildren<SkeletonAnimation>(true); }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            remainingDelay = (ID % 100) * 0.001f;
            started = false;
            if (animation != null) { animation.AnimationState.ClearTracks(); animation.Skeleton.SetToSetupPose(); }
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (IsHiding || started) return;
            if ((remainingDelay -= elapsed) > 0f) return;
            started = true;
            if (animation != null) animation.AnimationState.SetAnimation(0, "Idle", true);
        }
    }
}

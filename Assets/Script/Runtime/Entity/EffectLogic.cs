using UnityEngine;
using Spine.Unity;

namespace Luna.Gameplay
{
    public sealed class EffectLogic : EntityLogicBase
    {
        private SkeletonAnimation skeleton;
        private ParticleSystem[] particles;
        private AudioSource sound;
        private EffectData data;
        private float remaining;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            skeleton = GetComponentInChildren<SkeletonAnimation>(true);
            particles = GetComponentsInChildren<ParticleSystem>(true);
            sound = GetComponent<AudioSource>();
        }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            data = (EffectData)userData;
            CachedTransform.localScale *= Mathf.Max(0.01f, data.ScaleMultiplier);
            remaining = data.Lifetime <= 0f && data.OwnerId > 0 ? float.PositiveInfinity : Mathf.Max(0.1f, data.Lifetime);
            if (skeleton != null)
            {
                skeleton.Initialize(false);
                skeleton.AnimationState.Complete += Completed;
                var animation = skeleton.Skeleton.Data.FindAnimation(data.Animation);
                if (animation != null) skeleton.AnimationState.SetAnimation(0, data.Animation, data.Loop);
            }
            foreach (var particle in particles) { particle.Clear(true); particle.Play(true); }
            if (sound != null && sound.isActiveAndEnabled && sound.clip != null)
            {
                sound.volume = Defined.EffectVolumeSize;
                sound.Play();
                LunaSession.Current?.ObjectSounds.Add(sound);
            }
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (data.OwnerId > 0)
            {
                var owner = LunaSpawn.Find<EntityLogicBase>(data.OwnerId);
                if (owner == null) { SafeHide(); return; }
                if (data.FollowOwner) CachedTransform.position = owner.CachedTransform.position + data.Offset;
            }
            remaining -= elapsed;
            if (remaining <= 0f) { SafeHide(); return; }
            if (skeleton == null && particles.Length > 0 && !data.Loop)
            {
                foreach (var particle in particles) if (particle.IsAlive(true)) return;
                SafeHide();
            }
        }
        private void Completed(Spine.TrackEntry entry)
        {
            if (data != null && !data.Loop && skeleton != null &&
                ReferenceEquals(entry, skeleton.AnimationState.GetCurrent(0)) &&
                entry.Animation.Name == data.Animation) SafeHide();
        }
        protected override void OnHide(bool isShutdown, object userData)
        {
            if (skeleton != null && skeleton.AnimationState != null) skeleton.AnimationState.Complete -= Completed;
            foreach (var particle in particles) particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (sound != null) { sound.Stop(); LunaSession.Current?.ObjectSounds.Remove(sound); }
            data = null;
            base.OnHide(isShutdown, userData);
        }
    }
}

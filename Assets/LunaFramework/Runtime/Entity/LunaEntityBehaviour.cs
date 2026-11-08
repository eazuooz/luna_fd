using System.Collections;
using UnityEngine;

namespace EverydayDevup.Framework
{
    /// <summary>A game module whose lifetime and updates are owned by Game Framework Entity.</summary>
    public abstract class LunaEntityBehaviour : MonoBehaviour
    {
        private bool awakeCalled;
        private bool initialized;
        private bool released;
        private int showCount;
        public bool IsFirstEntityShow { get; private set; }
        protected virtual bool CanEntityUpdate => true;

        private IEnumerator Start()
        {
            while (!LunaEntities.IsReady) yield return null;
            if (this != null && gameObject.activeInHierarchy && !LunaEntities.IsSpawned(gameObject))
                LunaEntities.Adopt(gameObject);
        }

        internal void EntityShow()
        {
            if (!awakeCalled) { awakeCalled = true; OnEntityAwake(); }
            IsFirstEntityShow = showCount++ == 0;
            OnEntityShow();
        }
        public void EnsureEntityInitialized()
        {
            if (initialized || released) return;
            initialized = true;
            OnEntityInitialize();
            // Initialization may legitimately hide or release a transient entity.
            // Do not start work after that lifecycle has already ended.
            if (this == null || released || !isActiveAndEnabled) return;
            var routine = OnEntityStart();
            if (routine != null && this != null && !released && isActiveAndEnabled)
                StartCoroutine(routine);
        }
        internal void EntityUpdate(float elapsed, float realElapsed)
        {
            if (isActiveAndEnabled && !released && CanEntityUpdate) OnEntityUpdate(elapsed, realElapsed);
        }
        internal void EntityHide()
        {
            StopAllCoroutines();
            if (!released) OnEntityHide();
        }
        internal void EntityRelease()
        {
            if (released) return;
            released = true;
            StopAllCoroutines();
            OnEntityRelease();
        }
        protected virtual void OnEntityAwake() { }
        protected virtual void OnEntityInitialize() { }
        protected virtual IEnumerator OnEntityStart() { return null; }
        protected virtual void OnEntityShow() { }
        protected virtual void OnEntityHide() { }
        protected virtual void OnEntityUpdate(float elapsed, float realElapsed) { }
        protected virtual void OnEntityRelease() { }
    }
}

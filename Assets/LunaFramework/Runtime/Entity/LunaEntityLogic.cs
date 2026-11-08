using UnityEngine;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    /// <summary>One native GF entity owns all Luna gameplay modules on this object.</summary>
    public sealed class LunaEntityLogic : EntityLogic
    {
        private LunaEntityBehaviour[] modules;
        private bool hiding;
        private bool released;
        private bool destroying;
        private Vector3 authoredScale;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            authoredScale = transform.localScale;
            modules = GetComponents<LunaEntityBehaviour>();
        }
        protected override void OnShow(object userData)
        {
            var data = (LunaEntitySpawnData)userData;
            if (!data.Adopted)
            {
                var parent = data.Parent != null ? data.Parent : ((Component)Entity.EntityGroup.Helper).transform;
                transform.SetParent(parent, false);
                // Grow/shrink skills can be interrupted by Hide. Every spawn starts from
                // the prefab's authored scale, including prefabs whose scale is not one.
                transform.localScale = authoredScale;
                transform.SetPositionAndRotation(data.Position, data.Rotation);
            }
            hiding = false;
            data.Result = gameObject;
            LunaEntities.RegisterShown(gameObject, Entity.Id);
            base.OnShow(userData); // Spine and other Unity component Awake callbacks run on activation.
            foreach (var module in modules) if (module != null && module.enabled) module.EntityShow();
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            // Preserve Start's deferred semantics: callers can bind targets/towers after Spawn returns.
            foreach (var module in modules) if (module != null && module.isActiveAndEnabled) module.EnsureEntityInitialized();
            foreach (var module in modules) if (module != null) module.EntityUpdate(elapsed, realElapsed);
        }
        protected override void OnHide(bool isShutdown, object userData)
        {
            if (hiding) return;
            hiding = true;
            LunaEntities.UnregisterShown(gameObject);
            // Native destruction already owns the hierarchy. Only finish GF bookkeeping;
            // do not reparent or send lifecycle callbacks into partially destroyed components.
            if (destroying) return;
            foreach (var module in modules) if (module != null) module.EntityHide();
            base.OnHide(isShutdown, userData);
            // UI/scene parents can be destroyed before GF's deferred recycle update.
            // Detach adopted children too, so only the Entity helper retires them.
            if (Entity.EntityGroup.Helper is Component helper)
                transform.SetParent(helper.transform, false);
        }
        internal void ReleaseModules()
        {
            if (released) return;
            released = true;
            if (modules == null) return;
            foreach (var module in modules) if (module != null) module.EntityRelease();
        }
        private void OnDisable()
        {
            // An inactive UI tab only suspends rendering/update. Preserve its entity and
            // hierarchy so reopening the tab restores the same preview objects.
            if (!hiding && Available && LunaEntities.IsReady && !gameObject.activeSelf)
                LunaEntities.Hide(gameObject);
        }
        private void OnDestroy()
        {
            destroying = true;
            if (!hiding && Available && LunaEntities.IsReady) LunaEntities.Hide(gameObject);
            LunaEntities.UnregisterShown(gameObject);
            ReleaseModules();
        }
    }
}

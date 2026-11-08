using System;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    /// <summary>Shared ownership rules only. Each concrete game logic implements its own native callbacks.</summary>
    public abstract class EntityLogicBase : EntityLogic
    {
        private LunaEntityData ownedData;
        private Vector3 originalScale;
        public LunaEntityData SpawnData => ownedData;
        public int ID => Entity != null ? Entity.Id : 0;
        public bool IsHiding { get; private set; }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            originalScale = CachedTransform.localScale;
        }

        protected override void OnShow(object userData)
        {
            var data = userData as LunaEntityData;
            if (data == null || data.Id != ID || data.OwnedByEntity)
                throw new InvalidOperationException("Entity spawn data has an invalid identity or owner.");
            ownedData = data;
            data.OwnedByEntity = true;
            IsHiding = false;
            Transform parent = data.Parent != null ? data.Parent : ((Component)Entity.EntityGroup.Helper).transform;
            CachedTransform.SetParent(parent, false);
            CachedTransform.localScale = originalScale;
            CachedTransform.SetPositionAndRotation(data.Position, data.Rotation);
            base.OnShow(userData);
        }

        public void SafeHide()
        {
            if (IsHiding || Entity == null) return;
            IsHiding = true;
            var entities = GameEntry.GetComponent<EntityComponent>();
            if (entities != null && entities.HasEntity(ID)) entities.HideEntity(Entity);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            IsHiding = true;
            StopAllCoroutines();
            // UI forms may be released before the native entity recycle queue drains.
            if (Entity != null && Entity.EntityGroup.Helper is Component helper && helper != null)
                CachedTransform.SetParent(helper.transform, true);
            try { if (Available) base.OnHide(isShutdown, userData); }
            finally
            {
                LunaEntityData data = ownedData;
                ownedData = null;
                LunaSpawn.NotifyHidden(ID);
                if (data != null) ReferencePool.Release(data);
            }
        }

        protected override void OnRecycle()
        {
            // Also releases claimed data if a derived OnHide failed before calling its base.
            IsHiding = true;
            StopAllCoroutines();
            if (Entity != null && Entity.EntityGroup.Helper is Component helper && helper != null)
                CachedTransform.SetParent(helper.transform, true);
            if (Available) base.OnHide(false, null);
            var data = ownedData;
            ownedData = null;
            if (data != null) ReferencePool.Release(data);
            LunaSpawn.NotifyHidden(ID);
            base.OnRecycle();
        }
    }
}

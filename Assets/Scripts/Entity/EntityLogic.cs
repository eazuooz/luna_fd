//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class EntityLogic : MonoBehaviour
    {
        private bool mAvailable = false;
        private bool mVisible = false;
        private Entity mEntity = null;
        private Transform mCachedTransform = null;
        private int mOriginalLayer = 0;
        private Transform mOriginalTransform = null;

        public Entity Entity
        {
            get
            {
                return mEntity;
            }
        }

        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }

        public bool Available
        {
            get
            {
                return mAvailable;
            }
        }

        public bool Visible
        {
            get
            {
                return mAvailable && mVisible;
            }
            set
            {
                if (!mAvailable)
                {
                    Log.Warning("Entity '{0}' is not available.", Name);
                    return;
                }

                if (mVisible == value)
                {
                    return;
                }

                mVisible = value;
                InternalSetVisible(value);
            }
        }

        public Transform CachedTransform
        {
            get
            {
                return mCachedTransform;
            }
        }

        protected internal virtual void OnInit(object userData)
        {
            if (mCachedTransform == null)
            {
                mCachedTransform = transform;
            }

            mEntity = GetComponent<Entity>();
            mOriginalLayer = gameObject.layer;
            mOriginalTransform = CachedTransform.parent;
        }

        protected internal virtual void OnRecycle()
        {
        }

        protected internal virtual void OnShow(object userData)
        {
            mAvailable = true;
            Visible = true;
        }

        protected internal virtual void OnHide(bool isShutdown, object userData)
        {
            gameObject.SetLayerRecursively(mOriginalLayer);
            Visible = false;
            mAvailable = false;
        }

        protected internal virtual void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
        {
        }

        protected internal virtual void OnDetached(EntityLogic childEntity, object userData)
        {
        }

        protected internal virtual void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
        {
            CachedTransform.SetParent(parentTransform);
        }

        protected internal virtual void OnDetachFrom(EntityLogic parentEntity, object userData)
        {
            CachedTransform.SetParent(mOriginalTransform);
        }

        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        protected virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}

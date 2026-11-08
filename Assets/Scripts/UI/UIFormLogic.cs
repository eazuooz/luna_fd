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
    public abstract class UIFormLogic : MonoBehaviour
    {
        private bool mAvailable = false;
        private bool mVisible = false;
        private UIForm mUIForm = null;
        private Transform mCachedTransform = null;
        private int mOriginalLayer = 0;

        public UIForm UIForm
        {
            get
            {
                return mUIForm;
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
                    Log.Warning("UI form '{0}' is not available.", Name);
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

            mUIForm = GetComponent<UIForm>();
            mOriginalLayer = gameObject.layer;
        }

        protected internal virtual void OnRecycle()
        {
        }

        protected internal virtual void OnOpen(object userData)
        {
            mAvailable = true;
            Visible = true;
        }

        protected internal virtual void OnClose(bool isShutdown, object userData)
        {
            gameObject.SetLayerRecursively(mOriginalLayer);
            Visible = false;
            mAvailable = false;
        }

        protected internal virtual void OnPause()
        {
            Visible = false;
        }

        protected internal virtual void OnResume()
        {
            Visible = true;
        }

        protected internal virtual void OnCover()
        {
        }

        protected internal virtual void OnReveal()
        {
        }

        protected internal virtual void OnRefocus(object userData)
        {
        }

        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
        }

        protected virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}

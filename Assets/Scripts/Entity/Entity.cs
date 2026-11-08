//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Entity;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class Entity : MonoBehaviour, IEntity
    {
        private int mId;
        // GF recycles on its next update. An adopted UI/scene object may already have
        // been destroyed natively, but the pool still needs the original managed key.
        private GameObject mHandle;
        private string mEntityAssetName;
        private IEntityGroup mEntityGroup;
        private EntityLogic mEntityLogic;
        // Expose initialization failures to the game's native spawn completion handler.
        // The framework catches callbacks below, so a success event alone is insufficient.
        public string InitializationError { get; private set; }

        public int Id
        {
            get
            {
                return mId;
            }
        }

        public string EntityAssetName
        {
            get
            {
                return mEntityAssetName;
            }
        }

        public object Handle
        {
            get
            {
                return mHandle;
            }
        }

        public IEntityGroup EntityGroup
        {
            get
            {
                return mEntityGroup;
            }
        }

        public EntityLogic Logic
        {
            get
            {
                return mEntityLogic;
            }
        }

        public void OnInit(int entityId, string entityAssetName, IEntityGroup entityGroup, bool isNewInstance, object userData)
        {
            InitializationError = null;
            mId = entityId;
            mHandle = gameObject;
            mEntityAssetName = entityAssetName;
            if (isNewInstance)
            {
                mEntityGroup = entityGroup;
            }
            else if (mEntityGroup != entityGroup)
            {
                Log.Error("Entity group is inconsistent for non-new-instance entity.");
                return;
            }

            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            Type entityLogicType = showEntityInfo.EntityLogicType;
            if (entityLogicType == null)
            {
                Log.Error("Entity logic type is invalid.");
                return;
            }

            if (mEntityLogic != null)
            {
                if (mEntityLogic.GetType() == entityLogicType)
                {
                    mEntityLogic.enabled = true;
                    return;
                }

                Destroy(mEntityLogic);
                mEntityLogic = null;
            }

            mEntityLogic = gameObject.AddComponent(entityLogicType) as EntityLogic;
            if (mEntityLogic == null)
            {
                Log.Error("Entity '{0}' can not add entity logic.", entityAssetName);
                return;
            }

            try
            {
                mEntityLogic.OnInit(showEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                InitializationError = exception.ToString();
                Log.Error("Entity '[{0}]{1}' OnInit with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }

        public void OnRecycle()
        {
            try
            {
                // Native hierarchy destruction already disposed the logic. Complete
                // deferred pool bookkeeping without accessing a destroyed component.
                if (mEntityLogic != null)
                {
                    mEntityLogic.OnRecycle();
                    mEntityLogic.enabled = false;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnRecycle with exception '{2}'.", mId, mEntityAssetName, exception);
            }

            mId = 0;
        }

        public void OnShow(object userData)
        {
            if (!string.IsNullOrEmpty(InitializationError)) return;
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            try
            {
                mEntityLogic.OnShow(showEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                InitializationError = exception.ToString();
                Log.Error("Entity '[{0}]{1}' OnShow with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }

        public void OnHide(bool isShutdown, object userData)
        {
            try
            {
                mEntityLogic.OnHide(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnHide with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }

        public void OnAttached(IEntity childEntity, object userData)
        {
            AttachEntityInfo attachEntityInfo = (AttachEntityInfo)userData;
            try
            {
                mEntityLogic.OnAttached(((Entity)childEntity).Logic, attachEntityInfo.ParentTransform, attachEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttached with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }

        public void OnDetached(IEntity childEntity, object userData)
        {
            try
            {
                mEntityLogic.OnDetached(((Entity)childEntity).Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetached with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }

        public void OnAttachTo(IEntity parentEntity, object userData)
        {
            AttachEntityInfo attachEntityInfo = (AttachEntityInfo)userData;
            try
            {
                mEntityLogic.OnAttachTo(((Entity)parentEntity).Logic, attachEntityInfo.ParentTransform, attachEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttachTo with exception '{2}'.", mId, mEntityAssetName, exception);
            }

            ReferencePool.Release(attachEntityInfo);
        }

        public void OnDetachFrom(IEntity parentEntity, object userData)
        {
            try
            {
                mEntityLogic.OnDetachFrom(((Entity)parentEntity).Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetachFrom with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                mEntityLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnUpdate with exception '{2}'.", mId, mEntityAssetName, exception);
            }
        }
    }
}

//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.UI;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class UIForm : MonoBehaviour, IUIForm
    {
        private int mSerialId;
        private string mUIFormAssetName;
        private IUIGroup mUIGroup;
        private int mDepthInUIGroup;
        private bool mPauseCoveredUIForm;
        private UIFormLogic mUIFormLogic;

        public int SerialId
        {
            get
            {
                return mSerialId;
            }
        }

        public string UIFormAssetName
        {
            get
            {
                return mUIFormAssetName;
            }
        }

        public object Handle
        {
            get
            {
                return gameObject;
            }
        }

        public IUIGroup UIGroup
        {
            get
            {
                return mUIGroup;
            }
        }

        public int DepthInUIGroup
        {
            get
            {
                return mDepthInUIGroup;
            }
        }

        public bool PauseCoveredUIForm
        {
            get
            {
                return mPauseCoveredUIForm;
            }
        }

        public UIFormLogic Logic
        {
            get
            {
                return mUIFormLogic;
            }
        }

        public void OnInit(int serialId, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance, object userData)
        {
            mSerialId = serialId;
            mUIFormAssetName = uiFormAssetName;
            mUIGroup = uiGroup;
            mDepthInUIGroup = 0;
            mPauseCoveredUIForm = pauseCoveredUIForm;

            if (!isNewInstance)
            {
                return;
            }

            mUIFormLogic = GetComponent<UIFormLogic>();
            if (mUIFormLogic == null)
            {
                Log.Error("UI form '{0}' can not get UI form logic.", uiFormAssetName);
                return;
            }

            try
            {
                mUIFormLogic.OnInit(userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnInit with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnRecycle()
        {
            try
            {
                mUIFormLogic.OnRecycle();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnRecycle with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }

            mSerialId = 0;
            mDepthInUIGroup = 0;
            mPauseCoveredUIForm = true;
        }

        public void OnOpen(object userData)
        {
            try
            {
                mUIFormLogic.OnOpen(userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnOpen with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnClose(bool isShutdown, object userData)
        {
            try
            {
                mUIFormLogic.OnClose(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnClose with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnPause()
        {
            try
            {
                mUIFormLogic.OnPause();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnPause with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnResume()
        {
            try
            {
                mUIFormLogic.OnResume();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnResume with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnCover()
        {
            try
            {
                mUIFormLogic.OnCover();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnCover with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnReveal()
        {
            try
            {
                mUIFormLogic.OnReveal();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnReveal with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnRefocus(object userData)
        {
            try
            {
                mUIFormLogic.OnRefocus(userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnRefocus with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                mUIFormLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnUpdate with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }

        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            mDepthInUIGroup = depthInUIGroup;
            try
            {
                mUIFormLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnDepthChanged with exception '{2}'.", mSerialId, mUIFormAssetName, exception);
            }
        }
    }
}

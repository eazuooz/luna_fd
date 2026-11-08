//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using GameFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/UI")]
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IUIManager mUIManager = null;
        private EventComponent mEventComponent = null;

        private readonly List<IUIForm> mInternalUIFormResults = new List<IUIForm>();

        [SerializeField]
        private bool mEnableOpenUIFormSuccessEvent = true;

        [SerializeField]
        private bool mEnableOpenUIFormFailureEvent = true;

        [SerializeField]
        private bool mEnableOpenUIFormUpdateEvent = false;

        [SerializeField]
        private bool mEnableOpenUIFormDependencyAssetEvent = false;

        [SerializeField]
        private bool mEnableCloseUIFormCompleteEvent = true;

        [SerializeField]
        private float mInstanceAutoReleaseInterval = 60f;

        [SerializeField]
        private int mInstanceCapacity = 16;

        [SerializeField]
        private float mInstanceExpireTime = 60f;

        [SerializeField]
        private int mInstancePriority = 0;

        [SerializeField]
        private Transform mInstanceRoot = null;

        [SerializeField]
        private string mUIFormHelperTypeName = "UnityGameFramework.Runtime.DefaultUIFormHelper";

        [SerializeField]
        private UIFormHelperBase mCustomUIFormHelper = null;

        [SerializeField]
        private string mUIGroupHelperTypeName = "UnityGameFramework.Runtime.DefaultUIGroupHelper";

        [SerializeField]
        private UIGroupHelperBase mCustomUIGroupHelper = null;

        [SerializeField]
        private UIGroup[] mUIGroups = null;

        public int UIGroupCount
        {
            get
            {
                return mUIManager.UIGroupCount;
            }
        }

        public float InstanceAutoReleaseInterval
        {
            get
            {
                return mUIManager.InstanceAutoReleaseInterval;
            }
            set
            {
                mUIManager.InstanceAutoReleaseInterval = mInstanceAutoReleaseInterval = value;
            }
        }

        public int InstanceCapacity
        {
            get
            {
                return mUIManager.InstanceCapacity;
            }
            set
            {
                mUIManager.InstanceCapacity = mInstanceCapacity = value;
            }
        }

        public float InstanceExpireTime
        {
            get
            {
                return mUIManager.InstanceExpireTime;
            }
            set
            {
                mUIManager.InstanceExpireTime = mInstanceExpireTime = value;
            }
        }

        public int InstancePriority
        {
            get
            {
                return mUIManager.InstancePriority;
            }
            set
            {
                mUIManager.InstancePriority = mInstancePriority = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mUIManager = GameFrameworkEntry.GetModule<IUIManager>();
            if (mUIManager == null)
            {
                Log.Fatal("UI manager is invalid.");
                return;
            }

            if (mEnableOpenUIFormSuccessEvent)
            {
                mUIManager.OpenUIFormSuccess += OnOpenUIFormSuccess;
            }

            mUIManager.OpenUIFormFailure += OnOpenUIFormFailure;

            if (mEnableOpenUIFormUpdateEvent)
            {
                mUIManager.OpenUIFormUpdate += OnOpenUIFormUpdate;
            }

            if (mEnableOpenUIFormDependencyAssetEvent)
            {
                mUIManager.OpenUIFormDependencyAsset += OnOpenUIFormDependencyAsset;
            }

            if (mEnableCloseUIFormCompleteEvent)
            {
                mUIManager.CloseUIFormComplete += OnCloseUIFormComplete;
            }
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            mEventComponent = GameEntry.GetComponent<EventComponent>();
            if (mEventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            mUIManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);

            mUIManager.SetObjectPoolManager(GameFrameworkEntry.GetModule<IObjectPoolManager>());
            mUIManager.InstanceAutoReleaseInterval = mInstanceAutoReleaseInterval;
            mUIManager.InstanceCapacity = mInstanceCapacity;
            mUIManager.InstanceExpireTime = mInstanceExpireTime;
            mUIManager.InstancePriority = mInstancePriority;

            UIFormHelperBase uiFormHelper = Helper.CreateHelper(mUIFormHelperTypeName, mCustomUIFormHelper);
            if (uiFormHelper == null)
            {
                Log.Error("Can not create UI form helper.");
                return;
            }

            uiFormHelper.name = "UI Form Helper";
            Transform transform = uiFormHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mUIManager.SetUIFormHelper(uiFormHelper);

            if (mInstanceRoot == null)
            {
                mInstanceRoot = new GameObject("UI Form Instances").transform;
                mInstanceRoot.SetParent(gameObject.transform);
                mInstanceRoot.localScale = Vector3.one;
            }

            mInstanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            for (int i = 0; i < mUIGroups.Length; i++)
            {
                if (!AddUIGroup(mUIGroups[i].Name, mUIGroups[i].Depth))
                {
                    Log.Warning("Add UI group '{0}' failure.", mUIGroups[i].Name);
                    continue;
                }
            }
        }

        public bool HasUIGroup(string uiGroupName)
        {
            return mUIManager.HasUIGroup(uiGroupName);
        }

        public IUIGroup GetUIGroup(string uiGroupName)
        {
            return mUIManager.GetUIGroup(uiGroupName);
        }

        public IUIGroup[] GetAllUIGroups()
        {
            return mUIManager.GetAllUIGroups();
        }

        public void GetAllUIGroups(List<IUIGroup> results)
        {
            mUIManager.GetAllUIGroups(results);
        }

        public bool AddUIGroup(string uiGroupName)
        {
            return AddUIGroup(uiGroupName, 0);
        }

        public bool AddUIGroup(string uiGroupName, int depth)
        {
            if (mUIManager.HasUIGroup(uiGroupName))
            {
                return false;
            }

            UIGroupHelperBase uiGroupHelper = Helper.CreateHelper(mUIGroupHelperTypeName, mCustomUIGroupHelper, UIGroupCount);
            if (uiGroupHelper == null)
            {
                Log.Error("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = Utility.Text.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = uiGroupHelper.transform;
            transform.SetParent(mInstanceRoot);
            transform.localScale = Vector3.one;

            return mUIManager.AddUIGroup(uiGroupName, depth, uiGroupHelper);
        }

        public bool HasUIForm(int serialId)
        {
            return mUIManager.HasUIForm(serialId);
        }

        public bool HasUIForm(string uiFormAssetName)
        {
            return mUIManager.HasUIForm(uiFormAssetName);
        }

        public UIForm GetUIForm(int serialId)
        {
            return (UIForm)mUIManager.GetUIForm(serialId);
        }

        public UIForm GetUIForm(string uiFormAssetName)
        {
            return (UIForm)mUIManager.GetUIForm(uiFormAssetName);
        }

        public UIForm[] GetUIForms(string uiFormAssetName)
        {
            IUIForm[] uiForms = mUIManager.GetUIForms(uiFormAssetName);
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        public void GetUIForms(string uiFormAssetName, List<UIForm> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            mUIManager.GetUIForms(uiFormAssetName, mInternalUIFormResults);
            foreach (IUIForm uiForm in mInternalUIFormResults)
            {
                results.Add((UIForm)uiForm);
            }
        }

        public UIForm[] GetAllLoadedUIForms()
        {
            IUIForm[] uiForms = mUIManager.GetAllLoadedUIForms();
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        public void GetAllLoadedUIForms(List<UIForm> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            mUIManager.GetAllLoadedUIForms(mInternalUIFormResults);
            foreach (IUIForm uiForm in mInternalUIFormResults)
            {
                results.Add((UIForm)uiForm);
            }
        }

        public int[] GetAllLoadingUIFormSerialIds()
        {
            return mUIManager.GetAllLoadingUIFormSerialIds();
        }

        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            mUIManager.GetAllLoadingUIFormSerialIds(results);
        }

        public bool IsLoadingUIForm(int serialId)
        {
            return mUIManager.IsLoadingUIForm(serialId);
        }

        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            return mUIManager.IsLoadingUIForm(uiFormAssetName);
        }

        public bool IsValidUIForm(UIForm uiForm)
        {
            return mUIManager.IsValidUIForm(uiForm);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, DefaultPriority, false, null);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, false, null);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, DefaultPriority, pauseCoveredUIForm, null);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, DefaultPriority, false, userData);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, null);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, false, userData);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, DefaultPriority, pauseCoveredUIForm, userData);
        }

        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {
            return mUIManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
        }

        public void CloseUIForm(int serialId)
        {
            mUIManager.CloseUIForm(serialId);
        }

        public void CloseUIForm(int serialId, object userData)
        {
            mUIManager.CloseUIForm(serialId, userData);
        }

        public void CloseUIForm(UIForm uiForm)
        {
            mUIManager.CloseUIForm(uiForm);
        }

        public void CloseUIForm(UIForm uiForm, object userData)
        {
            mUIManager.CloseUIForm(uiForm, userData);
        }

        public void CloseAllLoadedUIForms()
        {
            mUIManager.CloseAllLoadedUIForms();
        }

        public void CloseAllLoadedUIForms(object userData)
        {
            mUIManager.CloseAllLoadedUIForms(userData);
        }

        public void CloseAllLoadingUIForms()
        {
            mUIManager.CloseAllLoadingUIForms();
        }

        public void RefocusUIForm(UIForm uiForm)
        {
            mUIManager.RefocusUIForm(uiForm);
        }

        public void RefocusUIForm(UIForm uiForm, object userData)
        {
            mUIManager.RefocusUIForm(uiForm, userData);
        }

        public void SetUIFormInstanceLocked(UIForm uiForm, bool locked)
        {
            if (uiForm == null)
            {
                Log.Warning("UI form is invalid.");
                return;
            }

            mUIManager.SetUIFormInstanceLocked(uiForm.gameObject, locked);
        }

        public void SetUIFormInstancePriority(UIForm uiForm, int priority)
        {
            if (uiForm == null)
            {
                Log.Warning("UI form is invalid.");
                return;
            }

            mUIManager.SetUIFormInstancePriority(uiForm.gameObject, priority);
        }

        private void OnOpenUIFormSuccess(object sender, GameFramework.UI.OpenUIFormSuccessEventArgs e)
        {
            mEventComponent.Fire(this, OpenUIFormSuccessEventArgs.Create(e));
        }

        private void OnOpenUIFormFailure(object sender, GameFramework.UI.OpenUIFormFailureEventArgs e)
        {
            Log.Warning("Open UI form failure, asset name '{0}', UI group name '{1}', pause covered UI form '{2}', error message '{3}'.", e.UIFormAssetName, e.UIGroupName, e.PauseCoveredUIForm, e.ErrorMessage);
            if (mEnableOpenUIFormFailureEvent)
            {
                mEventComponent.Fire(this, OpenUIFormFailureEventArgs.Create(e));
            }
        }

        private void OnOpenUIFormUpdate(object sender, GameFramework.UI.OpenUIFormUpdateEventArgs e)
        {
            mEventComponent.Fire(this, OpenUIFormUpdateEventArgs.Create(e));
        }

        private void OnOpenUIFormDependencyAsset(object sender, GameFramework.UI.OpenUIFormDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, OpenUIFormDependencyAssetEventArgs.Create(e));
        }

        private void OnCloseUIFormComplete(object sender, GameFramework.UI.CloseUIFormCompleteEventArgs e)
        {
            mEventComponent.Fire(this, CloseUIFormCompleteEventArgs.Create(e));
        }
    }
}

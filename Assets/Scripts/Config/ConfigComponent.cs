//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Config;
using GameFramework.Resource;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Config")]
    public sealed class ConfigComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IConfigManager mConfigManager = null;
        private EventComponent mEventComponent = null;

        [SerializeField]
        private bool mEnableLoadConfigUpdateEvent = false;

        [SerializeField]
        private bool mEnableLoadConfigDependencyAssetEvent = false;

        [SerializeField]
        private string mConfigHelperTypeName = "UnityGameFramework.Runtime.DefaultConfigHelper";

        [SerializeField]
        private ConfigHelperBase mCustomConfigHelper = null;

        [SerializeField]
        private int mCachedBytesSize = 0;

        public int Count
        {
            get
            {
                return mConfigManager.Count;
            }
        }

        public int CachedBytesSize
        {
            get
            {
                return mConfigManager.CachedBytesSize;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mConfigManager = GameFrameworkEntry.GetModule<IConfigManager>();
            if (mConfigManager == null)
            {
                Log.Fatal("Config manager is invalid.");
                return;
            }

            mConfigManager.ReadDataSuccess += OnReadDataSuccess;
            mConfigManager.ReadDataFailure += OnReadDataFailure;

            if (mEnableLoadConfigUpdateEvent)
            {
                mConfigManager.ReadDataUpdate += OnReadDataUpdate;
            }

            if (mEnableLoadConfigDependencyAssetEvent)
            {
                mConfigManager.ReadDataDependencyAsset += OnReadDataDependencyAsset;
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

            mConfigManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);

            ConfigHelperBase configHelper = Helper.CreateHelper(mConfigHelperTypeName, mCustomConfigHelper);
            if (configHelper == null)
            {
                Log.Error("Can not create config helper.");
                return;
            }

            configHelper.name = "Config Helper";
            Transform transform = configHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mConfigManager.SetDataProviderHelper(configHelper);
            mConfigManager.SetConfigHelper(configHelper);
            if (mCachedBytesSize > 0)
            {
                EnsureCachedBytesSize(mCachedBytesSize);
            }
        }

        public void EnsureCachedBytesSize(int ensureSize)
        {
            mConfigManager.EnsureCachedBytesSize(ensureSize);
        }

        public void FreeCachedBytes()
        {
            mConfigManager.FreeCachedBytes();
        }

        public void ReadData(string configAssetName)
        {
            mConfigManager.ReadData(configAssetName);
        }

        public void ReadData(string configAssetName, int priority)
        {
            mConfigManager.ReadData(configAssetName, priority);
        }

        public void ReadData(string configAssetName, object userData)
        {
            mConfigManager.ReadData(configAssetName, userData);
        }

        public void ReadData(string configAssetName, int priority, object userData)
        {
            mConfigManager.ReadData(configAssetName, priority, userData);
        }

        public bool ParseData(string configString)
        {
            return mConfigManager.ParseData(configString);
        }

        public bool ParseData(string configString, object userData)
        {
            return mConfigManager.ParseData(configString, userData);
        }

        public bool ParseData(byte[] configBytes)
        {
            return mConfigManager.ParseData(configBytes);
        }

        public bool ParseData(byte[] configBytes, object userData)
        {
            return mConfigManager.ParseData(configBytes, userData);
        }

        public bool ParseData(byte[] configBytes, int startIndex, int length)
        {
            return mConfigManager.ParseData(configBytes, startIndex, length);
        }

        public bool ParseData(byte[] configBytes, int startIndex, int length, object userData)
        {
            return mConfigManager.ParseData(configBytes, startIndex, length, userData);
        }

        public bool HasConfig(string configName)
        {
            return mConfigManager.HasConfig(configName);
        }

        public bool GetBool(string configName)
        {
            return mConfigManager.GetBool(configName);
        }

        public bool GetBool(string configName, bool defaultValue)
        {
            return mConfigManager.GetBool(configName, defaultValue);
        }

        public int GetInt(string configName)
        {
            return mConfigManager.GetInt(configName);
        }

        public int GetInt(string configName, int defaultValue)
        {
            return mConfigManager.GetInt(configName, defaultValue);
        }

        public float GetFloat(string configName)
        {
            return mConfigManager.GetFloat(configName);
        }

        public float GetFloat(string configName, float defaultValue)
        {
            return mConfigManager.GetFloat(configName, defaultValue);
        }

        public string GetString(string configName)
        {
            return mConfigManager.GetString(configName);
        }

        public string GetString(string configName, string defaultValue)
        {
            return mConfigManager.GetString(configName, defaultValue);
        }

        public bool AddConfig(string configName, bool boolValue, int intValue, float floatValue, string stringValue)
        {
            return mConfigManager.AddConfig(configName, boolValue, intValue, floatValue, stringValue);
        }

        public bool RemoveConfig(string configName)
        {
            return mConfigManager.RemoveConfig(configName);
        }

        public void RemoveAllConfigs()
        {
            mConfigManager.RemoveAllConfigs();
        }

        private void OnReadDataSuccess(object sender, ReadDataSuccessEventArgs e)
        {
            mEventComponent.Fire(this, LoadConfigSuccessEventArgs.Create(e));
        }

        private void OnReadDataFailure(object sender, ReadDataFailureEventArgs e)
        {
            Log.Warning("Load config failure, asset name '{0}', error message '{1}'.", e.DataAssetName, e.ErrorMessage);
            mEventComponent.Fire(this, LoadConfigFailureEventArgs.Create(e));
        }

        private void OnReadDataUpdate(object sender, ReadDataUpdateEventArgs e)
        {
            mEventComponent.Fire(this, LoadConfigUpdateEventArgs.Create(e));
        }

        private void OnReadDataDependencyAsset(object sender, ReadDataDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, LoadConfigDependencyAssetEventArgs.Create(e));
        }
    }
}

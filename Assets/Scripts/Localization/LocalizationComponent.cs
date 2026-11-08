//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Localization;
using GameFramework.Resource;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Localization")]
    public sealed class LocalizationComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private ILocalizationManager mLocalizationManager = null;
        private EventComponent mEventComponent = null;

        [SerializeField]
        private bool mEnableLoadDictionaryUpdateEvent = false;

        [SerializeField]
        private bool mEnableLoadDictionaryDependencyAssetEvent = false;

        [SerializeField]
        private string mLocalizationHelperTypeName = "UnityGameFramework.Runtime.DefaultLocalizationHelper";

        [SerializeField]
        private LocalizationHelperBase mCustomLocalizationHelper = null;

        [SerializeField]
        private int mCachedBytesSize = 0;

        public Language Language
        {
            get
            {
                return mLocalizationManager.Language;
            }
            set
            {
                mLocalizationManager.Language = value;
            }
        }

        public Language SystemLanguage
        {
            get
            {
                return mLocalizationManager.SystemLanguage;
            }
        }

        public int DictionaryCount
        {
            get
            {
                return mLocalizationManager.DictionaryCount;
            }
        }

        public int CachedBytesSize
        {
            get
            {
                return mLocalizationManager.CachedBytesSize;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mLocalizationManager = GameFrameworkEntry.GetModule<ILocalizationManager>();
            if (mLocalizationManager == null)
            {
                Log.Fatal("Localization manager is invalid.");
                return;
            }

            mLocalizationManager.ReadDataSuccess += OnReadDataSuccess;
            mLocalizationManager.ReadDataFailure += OnReadDataFailure;

            if (mEnableLoadDictionaryUpdateEvent)
            {
                mLocalizationManager.ReadDataUpdate += OnReadDataUpdate;
            }

            if (mEnableLoadDictionaryDependencyAssetEvent)
            {
                mLocalizationManager.ReadDataDependencyAsset += OnReadDataDependencyAsset;
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

            mLocalizationManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);

            LocalizationHelperBase localizationHelper = Helper.CreateHelper(mLocalizationHelperTypeName, mCustomLocalizationHelper);
            if (localizationHelper == null)
            {
                Log.Error("Can not create localization helper.");
                return;
            }

            localizationHelper.name = "Localization Helper";
            Transform transform = localizationHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mLocalizationManager.SetDataProviderHelper(localizationHelper);
            mLocalizationManager.SetLocalizationHelper(localizationHelper);
            mLocalizationManager.Language = baseComponent.EditorResourceMode && baseComponent.EditorLanguage != Language.Unspecified ? baseComponent.EditorLanguage : mLocalizationManager.SystemLanguage;
            if (mCachedBytesSize > 0)
            {
                EnsureCachedBytesSize(mCachedBytesSize);
            }
        }

        public void EnsureCachedBytesSize(int ensureSize)
        {
            mLocalizationManager.EnsureCachedBytesSize(ensureSize);
        }

        public void FreeCachedBytes()
        {
            mLocalizationManager.FreeCachedBytes();
        }

        public void ReadData(string dictionaryAssetName)
        {
            mLocalizationManager.ReadData(dictionaryAssetName);
        }

        public void ReadData(string dictionaryAssetName, int priority)
        {
            mLocalizationManager.ReadData(dictionaryAssetName, priority);
        }

        public void ReadData(string dictionaryAssetName, object userData)
        {
            mLocalizationManager.ReadData(dictionaryAssetName, userData);
        }

        public void ReadData(string dictionaryAssetName, int priority, object userData)
        {
            mLocalizationManager.ReadData(dictionaryAssetName, priority, userData);
        }

        public bool ParseData(string dictionaryString)
        {
            return mLocalizationManager.ParseData(dictionaryString);
        }

        public bool ParseData(string dictionaryString, object userData)
        {
            return mLocalizationManager.ParseData(dictionaryString, userData);
        }

        public bool ParseData(byte[] dictionaryBytes)
        {
            return mLocalizationManager.ParseData(dictionaryBytes);
        }

        public bool ParseData(byte[] dictionaryBytes, object userData)
        {
            return mLocalizationManager.ParseData(dictionaryBytes, userData);
        }

        public bool ParseData(byte[] dictionaryBytes, int startIndex, int length)
        {
            return mLocalizationManager.ParseData(dictionaryBytes, startIndex, length);
        }

        public bool ParseData(byte[] dictionaryBytes, int startIndex, int length, object userData)
        {
            return mLocalizationManager.ParseData(dictionaryBytes, startIndex, length, userData);
        }

        public string GetString(string key)
        {
            return mLocalizationManager.GetString(key);
        }

        public string GetString<T>(string key, T arg)
        {
            return mLocalizationManager.GetString(key, arg);
        }

        public string GetString<T1, T2>(string key, T1 arg1, T2 arg2)
        {
            return mLocalizationManager.GetString(key, arg1, arg2);
        }

        public string GetString<T1, T2, T3>(string key, T1 arg1, T2 arg2, T3 arg3)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3);
        }

        public string GetString<T1, T2, T3, T4>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4);
        }

        public string GetString<T1, T2, T3, T4, T5>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5);
        }

        public string GetString<T1, T2, T3, T4, T5, T6>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            return mLocalizationManager.GetString(key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public bool HasRawString(string key)
        {
            return mLocalizationManager.HasRawString(key);
        }

        public string GetRawString(string key)
        {
            return mLocalizationManager.GetRawString(key);
        }

        public bool RemoveRawString(string key)
        {
            return mLocalizationManager.RemoveRawString(key);
        }

        public void RemoveAllRawStrings()
        {
            mLocalizationManager.RemoveAllRawStrings();
        }

        private void OnReadDataSuccess(object sender, ReadDataSuccessEventArgs e)
        {
            mEventComponent.Fire(this, LoadDictionarySuccessEventArgs.Create(e));
        }

        private void OnReadDataFailure(object sender, ReadDataFailureEventArgs e)
        {
            Log.Warning("Load dictionary failure, asset name '{0}', error message '{1}'.", e.DataAssetName, e.ErrorMessage);
            mEventComponent.Fire(this, LoadDictionaryFailureEventArgs.Create(e));
        }

        private void OnReadDataUpdate(object sender, ReadDataUpdateEventArgs e)
        {
            mEventComponent.Fire(this, LoadDictionaryUpdateEventArgs.Create(e));
        }

        private void OnReadDataDependencyAsset(object sender, ReadDataDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, LoadDictionaryDependencyAssetEventArgs.Create(e));
        }
    }
}

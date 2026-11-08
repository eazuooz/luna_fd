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
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Base")]
    public sealed class BaseComponent : GameFrameworkComponent
    {
        private const int DefaultDpi = 96;  // default windows dpi

        private float mGameSpeedBeforePause = 1f;

        [SerializeField]
        private bool mEditorResourceMode = true;

        [SerializeField]
        private Language mEditorLanguage = Language.Unspecified;

        [SerializeField]
        private string mTextHelperTypeName = "UnityGameFramework.Runtime.DefaultTextHelper";

        [SerializeField]
        private string mVersionHelperTypeName = "UnityGameFramework.Runtime.DefaultVersionHelper";

        [SerializeField]
        private string mLogHelperTypeName = "UnityGameFramework.Runtime.DefaultLogHelper";

        [SerializeField]
        private string mCompressionHelperTypeName = "UnityGameFramework.Runtime.DefaultCompressionHelper";

        [SerializeField]
        private string mJsonHelperTypeName = "UnityGameFramework.Runtime.DefaultJsonHelper";

        [SerializeField]
        private int mFrameRate = 30;

        [SerializeField]
        private float mGameSpeed = 1f;

        [SerializeField]
        private bool mRunInBackground = true;

        [SerializeField]
        private bool mNeverSleep = true;

        public bool EditorResourceMode
        {
            get
            {
                return mEditorResourceMode;
            }
            set
            {
                mEditorResourceMode = value;
            }
        }

        public Language EditorLanguage
        {
            get
            {
                return mEditorLanguage;
            }
            set
            {
                mEditorLanguage = value;
            }
        }

        public IResourceManager EditorResourceHelper
        {
            get;
            set;
        }

        public int FrameRate
        {
            get
            {
                return mFrameRate;
            }
            set
            {
                Application.targetFrameRate = mFrameRate = value;
            }
        }

        public float GameSpeed
        {
            get
            {
                return mGameSpeed;
            }
            set
            {
                Time.timeScale = mGameSpeed = value >= 0f ? value : 0f;
            }
        }

        public bool IsGamePaused
        {
            get
            {
                return mGameSpeed <= 0f;
            }
        }

        public bool IsNormalGameSpeed
        {
            get
            {
                return mGameSpeed == 1f;
            }
        }

        public bool RunInBackground
        {
            get
            {
                return mRunInBackground;
            }
            set
            {
                Application.runInBackground = mRunInBackground = value;
            }
        }

        public bool NeverSleep
        {
            get
            {
                return mNeverSleep;
            }
            set
            {
                mNeverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            InitTextHelper();
            InitVersionHelper();
            InitLogHelper();
            Log.Info("Game Framework Version: {0}", GameFramework.Version.GameFrameworkVersion);
            Log.Info("Game Version: {0} ({1})", GameFramework.Version.GameVersion, GameFramework.Version.InternalGameVersion);
            Log.Info("Unity Version: {0}", Application.unityVersion);

#if UNITY_5_3_OR_NEWER || UNITY_5_3
            InitCompressionHelper();
            InitJsonHelper();

            Utility.Converter.ScreenDpi = Screen.dpi;
            if (Utility.Converter.ScreenDpi <= 0)
            {
                Utility.Converter.ScreenDpi = DefaultDpi;
            }

            mEditorResourceMode &= Application.isEditor;
            if (mEditorResourceMode)
            {
                Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
            }

            Application.targetFrameRate = mFrameRate;
            Time.timeScale = mGameSpeed;
            Application.runInBackground = mRunInBackground;
            Screen.sleepTimeout = mNeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
#else
            Log.Error("Game Framework only applies with Unity 5.3 and above, but current Unity version is {0}.", Application.unityVersion);
            GameEntry.Shutdown(ShutdownType.Quit);
#endif
#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif
        }

        private void Start()
        {
        }

        private void Update()
        {
            GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnApplicationQuit()
        {
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            GameFrameworkEntry.Shutdown();
        }

        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            mGameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = mGameSpeedBeforePause;
        }

        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        internal void Shutdown()
        {
            Destroy(gameObject);
        }

        private void InitTextHelper()
        {
            if (string.IsNullOrEmpty(mTextHelperTypeName))
            {
                return;
            }

            Type textHelperType = Utility.Assembly.GetType(mTextHelperTypeName);
            if (textHelperType == null)
            {
                Log.Error("Can not find text helper type '{0}'.", mTextHelperTypeName);
                return;
            }

            Utility.Text.ITextHelper textHelper = (Utility.Text.ITextHelper)Activator.CreateInstance(textHelperType);
            if (textHelper == null)
            {
                Log.Error("Can not create text helper instance '{0}'.", mTextHelperTypeName);
                return;
            }

            Utility.Text.SetTextHelper(textHelper);
        }

        private void InitVersionHelper()
        {
            if (string.IsNullOrEmpty(mVersionHelperTypeName))
            {
                return;
            }

            Type versionHelperType = Utility.Assembly.GetType(mVersionHelperTypeName);
            if (versionHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find version helper type '{0}'.", mVersionHelperTypeName));
            }

            GameFramework.Version.IVersionHelper versionHelper = (GameFramework.Version.IVersionHelper)Activator.CreateInstance(versionHelperType);
            if (versionHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create version helper instance '{0}'.", mVersionHelperTypeName));
            }

            GameFramework.Version.SetVersionHelper(versionHelper);
        }

        private void InitLogHelper()
        {
            if (string.IsNullOrEmpty(mLogHelperTypeName))
            {
                return;
            }

            Type logHelperType = Utility.Assembly.GetType(mLogHelperTypeName);
            if (logHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find log helper type '{0}'.", mLogHelperTypeName));
            }

            GameFrameworkLog.ILogHelper logHelper = (GameFrameworkLog.ILogHelper)Activator.CreateInstance(logHelperType);
            if (logHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create log helper instance '{0}'.", mLogHelperTypeName));
            }

            GameFrameworkLog.SetLogHelper(logHelper);
        }

        private void InitCompressionHelper()
        {
            if (string.IsNullOrEmpty(mCompressionHelperTypeName))
            {
                return;
            }

            Type compressionHelperType = Utility.Assembly.GetType(mCompressionHelperTypeName);
            if (compressionHelperType == null)
            {
                Log.Error("Can not find compression helper type '{0}'.", mCompressionHelperTypeName);
                return;
            }

            Utility.Compression.ICompressionHelper compressionHelper = (Utility.Compression.ICompressionHelper)Activator.CreateInstance(compressionHelperType);
            if (compressionHelper == null)
            {
                Log.Error("Can not create compression helper instance '{0}'.", mCompressionHelperTypeName);
                return;
            }

            Utility.Compression.SetCompressionHelper(compressionHelper);
        }

        private void InitJsonHelper()
        {
            if (string.IsNullOrEmpty(mJsonHelperTypeName))
            {
                return;
            }

            Type jsonHelperType = Utility.Assembly.GetType(mJsonHelperTypeName);
            if (jsonHelperType == null)
            {
                Log.Error("Can not find JSON helper type '{0}'.", mJsonHelperTypeName);
                return;
            }

            Utility.Json.IJsonHelper jsonHelper = (Utility.Json.IJsonHelper)Activator.CreateInstance(jsonHelperType);
            if (jsonHelper == null)
            {
                Log.Error("Can not create JSON helper instance '{0}'.", mJsonHelperTypeName);
                return;
            }

            Utility.Json.SetJsonHelper(jsonHelper);
        }

        private void OnLowMemory()
        {
            Log.Info("Low memory reported...");

            ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
            if (objectPoolComponent != null)
            {
                objectPoolComponent.ReleaseAllUnused();
            }

            ResourceComponent resourceCompoent = GameEntry.GetComponent<ResourceComponent>();
            if (resourceCompoent != null)
            {
                resourceCompoent.ForceUnloadUnusedAssets(true);
            }
        }
    }
}

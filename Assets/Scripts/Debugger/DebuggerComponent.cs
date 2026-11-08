//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Debugger;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Debugger")]
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        internal static readonly Rect DefaultIconRect = new Rect(10f, 10f, 60f, 60f);

        internal static readonly Rect DefaultWindowRect = new Rect(10f, 10f, 640f, 480f);

        internal static readonly float DefaultWindowScale = 1f;

        private static TextEditor s_TextEditor;
        private IDebuggerManager mDebuggerManager = null;
        private Rect mDragRect = new Rect(0f, 0f, float.MaxValue, 25f);
        private Rect mIconRect = DefaultIconRect;
        private Rect mWindowRect = DefaultWindowRect;
        private float mWindowScale = DefaultWindowScale;

        [SerializeField]
        private GUISkin mSkin = null;

        [SerializeField]
        private DebuggerActiveWindowType mActiveWindow = DebuggerActiveWindowType.AlwaysOpen;

        [SerializeField]
        private bool mShowFullWindow = false;

        [SerializeField]
        private ConsoleWindow mConsoleWindow = new ConsoleWindow();

        private SystemInformationWindow mSystemInformationWindow = new SystemInformationWindow();
        private EnvironmentInformationWindow mEnvironmentInformationWindow = new EnvironmentInformationWindow();
        private ScreenInformationWindow mScreenInformationWindow = new ScreenInformationWindow();
        private GraphicsInformationWindow mGraphicsInformationWindow = new GraphicsInformationWindow();
        private InputSummaryInformationWindow mInputSummaryInformationWindow = new InputSummaryInformationWindow();
        private InputTouchInformationWindow mInputTouchInformationWindow = new InputTouchInformationWindow();
        private InputLocationInformationWindow mInputLocationInformationWindow = new InputLocationInformationWindow();
        private InputAccelerationInformationWindow mInputAccelerationInformationWindow = new InputAccelerationInformationWindow();
        private InputGyroscopeInformationWindow mInputGyroscopeInformationWindow = new InputGyroscopeInformationWindow();
        private InputCompassInformationWindow mInputCompassInformationWindow = new InputCompassInformationWindow();
        private PathInformationWindow mPathInformationWindow = new PathInformationWindow();
        private SceneInformationWindow mSceneInformationWindow = new SceneInformationWindow();
        private TimeInformationWindow mTimeInformationWindow = new TimeInformationWindow();
        private QualityInformationWindow mQualityInformationWindow = new QualityInformationWindow();
        private ProfilerInformationWindow mProfilerInformationWindow = new ProfilerInformationWindow();
        private WebPlayerInformationWindow mWebPlayerInformationWindow = new WebPlayerInformationWindow();
        private RuntimeMemorySummaryWindow mRuntimeMemorySummaryWindow = new RuntimeMemorySummaryWindow();
        private RuntimeMemoryInformationWindow<Object> mRuntimeMemoryAllInformationWindow = new RuntimeMemoryInformationWindow<Object>();
        private RuntimeMemoryInformationWindow<Texture> mRuntimeMemoryTextureInformationWindow = new RuntimeMemoryInformationWindow<Texture>();
        private RuntimeMemoryInformationWindow<Mesh> mRuntimeMemoryMeshInformationWindow = new RuntimeMemoryInformationWindow<Mesh>();
        private RuntimeMemoryInformationWindow<Material> mRuntimeMemoryMaterialInformationWindow = new RuntimeMemoryInformationWindow<Material>();
        private RuntimeMemoryInformationWindow<Shader> mRuntimeMemoryShaderInformationWindow = new RuntimeMemoryInformationWindow<Shader>();
        private RuntimeMemoryInformationWindow<AnimationClip> mRuntimeMemoryAnimationClipInformationWindow = new RuntimeMemoryInformationWindow<AnimationClip>();
        private RuntimeMemoryInformationWindow<AudioClip> mRuntimeMemoryAudioClipInformationWindow = new RuntimeMemoryInformationWindow<AudioClip>();
        private RuntimeMemoryInformationWindow<Font> mRuntimeMemoryFontInformationWindow = new RuntimeMemoryInformationWindow<Font>();
        private RuntimeMemoryInformationWindow<TextAsset> mRuntimeMemoryTextAssetInformationWindow = new RuntimeMemoryInformationWindow<TextAsset>();
        private RuntimeMemoryInformationWindow<ScriptableObject> mRuntimeMemoryScriptableObjectInformationWindow = new RuntimeMemoryInformationWindow<ScriptableObject>();
        private ObjectPoolInformationWindow mObjectPoolInformationWindow = new ObjectPoolInformationWindow();
        private ReferencePoolInformationWindow mReferencePoolInformationWindow = new ReferencePoolInformationWindow();
        private NetworkInformationWindow mNetworkInformationWindow = new NetworkInformationWindow();
        private SettingsWindow mSettingsWindow = new SettingsWindow();
        private OperationsWindow mOperationsWindow = new OperationsWindow();

        private FpsCounter mFpsCounter = null;

        public bool ActiveWindow
        {
            get
            {
                return mDebuggerManager.ActiveWindow;
            }
            set
            {
                mDebuggerManager.ActiveWindow = value;
                enabled = value;
            }
        }

        public bool ShowFullWindow
        {
            get
            {
                return mShowFullWindow;
            }
            set
            {
                mShowFullWindow = value;
            }
        }

        public Rect IconRect
        {
            get
            {
                return mIconRect;
            }
            set
            {
                mIconRect = value;
            }
        }

        public Rect WindowRect
        {
            get
            {
                return mWindowRect;
            }
            set
            {
                mWindowRect = value;
            }
        }

        public float WindowScale
        {
            get
            {
                return mWindowScale;
            }
            set
            {
                mWindowScale = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mDebuggerManager = GameFrameworkEntry.GetModule<IDebuggerManager>();
            if (mDebuggerManager == null)
            {
                Log.Fatal("Debugger manager is invalid.");
                return;
            }

            mFpsCounter = new FpsCounter(0.5f);

            if (s_TextEditor == null)
            {
                s_TextEditor = new TextEditor();
            }
        }

        private void Start()
        {
            RegisterDebuggerWindow("Console", mConsoleWindow);
            RegisterDebuggerWindow("Information/System", mSystemInformationWindow);
            RegisterDebuggerWindow("Information/Environment", mEnvironmentInformationWindow);
            RegisterDebuggerWindow("Information/Screen", mScreenInformationWindow);
            RegisterDebuggerWindow("Information/Graphics", mGraphicsInformationWindow);
            RegisterDebuggerWindow("Information/Input/Summary", mInputSummaryInformationWindow);
            RegisterDebuggerWindow("Information/Input/Touch", mInputTouchInformationWindow);
            RegisterDebuggerWindow("Information/Input/Location", mInputLocationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Acceleration", mInputAccelerationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Gyroscope", mInputGyroscopeInformationWindow);
            RegisterDebuggerWindow("Information/Input/Compass", mInputCompassInformationWindow);
            RegisterDebuggerWindow("Information/Other/Scene", mSceneInformationWindow);
            RegisterDebuggerWindow("Information/Other/Path", mPathInformationWindow);
            RegisterDebuggerWindow("Information/Other/Time", mTimeInformationWindow);
            RegisterDebuggerWindow("Information/Other/Quality", mQualityInformationWindow);
            RegisterDebuggerWindow("Information/Other/Web Player", mWebPlayerInformationWindow);
            RegisterDebuggerWindow("Profiler/Summary", mProfilerInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Summary", mRuntimeMemorySummaryWindow);
            RegisterDebuggerWindow("Profiler/Memory/All", mRuntimeMemoryAllInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Texture", mRuntimeMemoryTextureInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Mesh", mRuntimeMemoryMeshInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Material", mRuntimeMemoryMaterialInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Shader", mRuntimeMemoryShaderInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AnimationClip", mRuntimeMemoryAnimationClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AudioClip", mRuntimeMemoryAudioClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Font", mRuntimeMemoryFontInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/TextAsset", mRuntimeMemoryTextAssetInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/ScriptableObject", mRuntimeMemoryScriptableObjectInformationWindow);
            RegisterDebuggerWindow("Profiler/Object Pool", mObjectPoolInformationWindow);
            RegisterDebuggerWindow("Profiler/Reference Pool", mReferencePoolInformationWindow);
            RegisterDebuggerWindow("Profiler/Network", mNetworkInformationWindow);
            RegisterDebuggerWindow("Other/Settings", mSettingsWindow);
            RegisterDebuggerWindow("Other/Operations", mOperationsWindow);

            switch (mActiveWindow)
            {
                case DebuggerActiveWindowType.AlwaysOpen:
                    ActiveWindow = true;
                    break;

                case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                    ActiveWindow = Debug.isDebugBuild;
                    break;

                case DebuggerActiveWindowType.OnlyOpenInEditor:
                    ActiveWindow = Application.isEditor;
                    break;

                default:
                    ActiveWindow = false;
                    break;
            }
        }

        private void Update()
        {
            mFpsCounter.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (mDebuggerManager == null || !mDebuggerManager.ActiveWindow)
            {
                return;
            }

            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedMatrix = GUI.matrix;

            GUI.skin = mSkin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(mWindowScale, mWindowScale, 1f));

            bool showFullWindow = mShowFullWindow;
            if (showFullWindow)
            {
                mWindowRect = GUILayout.Window(0, mWindowRect, DrawWindow, "<b>GAME FRAMEWORK DEBUGGER</b>");
            }
            else
            {
                mIconRect = GUILayout.Window(0, mIconRect, DrawDebuggerWindowIcon, "<b>DEBUGGER</b>");
            }

            GUI.matrix = cachedMatrix;
            GUI.skin = cachedGuiSkin;
        }

        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            mDebuggerManager.RegisterDebuggerWindow(path, debuggerWindow, args);
        }

        public bool UnregisterDebuggerWindow(string path)
        {
            return mDebuggerManager.UnregisterDebuggerWindow(path);
        }

        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return mDebuggerManager.GetDebuggerWindow(path);
        }

        public bool SelectDebuggerWindow(string path)
        {
            return mDebuggerManager.SelectDebuggerWindow(path);
        }

        public void ResetLayout()
        {
            IconRect = DefaultIconRect;
            WindowRect = DefaultWindowRect;
            WindowScale = DefaultWindowScale;
        }

        public void GetRecentLogs(List<LogNode> results)
        {
            mConsoleWindow.GetRecentLogs(results);
        }

        public void GetRecentLogs(List<LogNode> results, int count)
        {
            mConsoleWindow.GetRecentLogs(results, count);
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(mDragRect);
            DrawDebuggerWindowGroup(mDebuggerManager.DebuggerWindowRoot);
        }

        private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
        {
            if (debuggerWindowGroup == null)
            {
                return;
            }

            List<string> names = new List<string>();
            string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
            for (int i = 0; i < debuggerWindowNames.Length; i++)
            {
                names.Add(Utility.Text.Format("<b>{0}</b>", debuggerWindowNames[i]));
            }

            if (debuggerWindowGroup == mDebuggerManager.DebuggerWindowRoot)
            {
                names.Add("<b>Close</b>");
            }

            int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(), GUILayout.Height(30f), GUILayout.MaxWidth(Screen.width));
            if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
            {
                mShowFullWindow = false;
                return;
            }

            if (debuggerWindowGroup.SelectedWindow == null)
            {
                return;
            }

            if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
            {
                debuggerWindowGroup.SelectedWindow.OnLeave();
                debuggerWindowGroup.SelectedIndex = toolbarIndex;
                debuggerWindowGroup.SelectedWindow.OnEnter();
            }

            IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
            if (subDebuggerWindowGroup != null)
            {
                DrawDebuggerWindowGroup(subDebuggerWindowGroup);
            }

            debuggerWindowGroup.SelectedWindow.OnDraw();
        }

        private void DrawDebuggerWindowIcon(int windowId)
        {
            GUI.DragWindow(mDragRect);
            GUILayout.Space(5);
            Color32 color = Color.white;
            mConsoleWindow.RefreshCount();
            if (mConsoleWindow.FatalCount > 0)
            {
                color = mConsoleWindow.GetLogStringColor(LogType.Exception);
            }
            else if (mConsoleWindow.ErrorCount > 0)
            {
                color = mConsoleWindow.GetLogStringColor(LogType.Error);
            }
            else if (mConsoleWindow.WarningCount > 0)
            {
                color = mConsoleWindow.GetLogStringColor(LogType.Warning);
            }
            else
            {
                color = mConsoleWindow.GetLogStringColor(LogType.Log);
            }

            string title = Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}><b>FPS: {4:F2}</b></color>", color.r, color.g, color.b, color.a, mFpsCounter.CurrentFps);
            if (GUILayout.Button(title, GUILayout.Width(100f), GUILayout.Height(40f)))
            {
                mShowFullWindow = true;
            }
        }

        private static void CopyToClipboard(string content)
        {
            s_TextEditor.text = content;
            s_TextEditor.OnFocus();
            s_TextEditor.Copy();
            s_TextEditor.text = string.Empty;
        }
    }
}

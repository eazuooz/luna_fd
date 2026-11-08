//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Debugger;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class ConsoleWindow : IDebuggerWindow
        {
            private readonly Queue<LogNode> mLogNodes = new Queue<LogNode>();

            private SettingComponent mSettingComponent = null;
            private Vector2 mLogScrollPosition = Vector2.zero;
            private Vector2 mStackScrollPosition = Vector2.zero;
            private int mInfoCount = 0;
            private int mWarningCount = 0;
            private int mErrorCount = 0;
            private int mFatalCount = 0;
            private LogNode mSelectedNode = null;
            private bool mLastLockScroll = true;
            private bool mLastInfoFilter = true;
            private bool mLastWarningFilter = true;
            private bool mLastErrorFilter = true;
            private bool mLastFatalFilter = true;

            [SerializeField]
            private bool mLockScroll = true;

            [SerializeField]
            private int mMaxLine = 100;

            [SerializeField]
            private bool mInfoFilter = true;

            [SerializeField]
            private bool mWarningFilter = true;

            [SerializeField]
            private bool mErrorFilter = true;

            [SerializeField]
            private bool mFatalFilter = true;

            [SerializeField]
            private Color32 mInfoColor = Color.white;

            [SerializeField]
            private Color32 mWarningColor = Color.yellow;

            [SerializeField]
            private Color32 mErrorColor = Color.red;

            [SerializeField]
            private Color32 mFatalColor = new Color(0.7f, 0.2f, 0.2f);

            public bool LockScroll
            {
                get
                {
                    return mLockScroll;
                }
                set
                {
                    mLockScroll = value;
                }
            }

            public int MaxLine
            {
                get
                {
                    return mMaxLine;
                }
                set
                {
                    mMaxLine = value;
                }
            }

            public bool InfoFilter
            {
                get
                {
                    return mInfoFilter;
                }
                set
                {
                    mInfoFilter = value;
                }
            }

            public bool WarningFilter
            {
                get
                {
                    return mWarningFilter;
                }
                set
                {
                    mWarningFilter = value;
                }
            }

            public bool ErrorFilter
            {
                get
                {
                    return mErrorFilter;
                }
                set
                {
                    mErrorFilter = value;
                }
            }

            public bool FatalFilter
            {
                get
                {
                    return mFatalFilter;
                }
                set
                {
                    mFatalFilter = value;
                }
            }

            public int InfoCount
            {
                get
                {
                    return mInfoCount;
                }
            }

            public int WarningCount
            {
                get
                {
                    return mWarningCount;
                }
            }

            public int ErrorCount
            {
                get
                {
                    return mErrorCount;
                }
            }

            public int FatalCount
            {
                get
                {
                    return mFatalCount;
                }
            }

            public Color32 InfoColor
            {
                get
                {
                    return mInfoColor;
                }
                set
                {
                    mInfoColor = value;
                }
            }

            public Color32 WarningColor
            {
                get
                {
                    return mWarningColor;
                }
                set
                {
                    mWarningColor = value;
                }
            }

            public Color32 ErrorColor
            {
                get
                {
                    return mErrorColor;
                }
                set
                {
                    mErrorColor = value;
                }
            }

            public Color32 FatalColor
            {
                get
                {
                    return mFatalColor;
                }
                set
                {
                    mFatalColor = value;
                }
            }

            public void Initialize(params object[] args)
            {
                mSettingComponent = GameEntry.GetComponent<SettingComponent>();
                if (mSettingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                Application.logMessageReceived += OnLogMessageReceived;
                mLockScroll = mLastLockScroll = mSettingComponent.GetBool("Debugger.Console.LockScroll", true);
                mInfoFilter = mLastInfoFilter = mSettingComponent.GetBool("Debugger.Console.InfoFilter", true);
                mWarningFilter = mLastWarningFilter = mSettingComponent.GetBool("Debugger.Console.WarningFilter", true);
                mErrorFilter = mLastErrorFilter = mSettingComponent.GetBool("Debugger.Console.ErrorFilter", true);
                mFatalFilter = mLastFatalFilter = mSettingComponent.GetBool("Debugger.Console.FatalFilter", true);
            }

            public void Shutdown()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                Clear();
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (mLastLockScroll != mLockScroll)
                {
                    mLastLockScroll = mLockScroll;
                    mSettingComponent.SetBool("Debugger.Console.LockScroll", mLockScroll);
                }

                if (mLastInfoFilter != mInfoFilter)
                {
                    mLastInfoFilter = mInfoFilter;
                    mSettingComponent.SetBool("Debugger.Console.InfoFilter", mInfoFilter);
                }

                if (mLastWarningFilter != mWarningFilter)
                {
                    mLastWarningFilter = mWarningFilter;
                    mSettingComponent.SetBool("Debugger.Console.WarningFilter", mWarningFilter);
                }

                if (mLastErrorFilter != mErrorFilter)
                {
                    mLastErrorFilter = mErrorFilter;
                    mSettingComponent.SetBool("Debugger.Console.ErrorFilter", mErrorFilter);
                }

                if (mLastFatalFilter != mFatalFilter)
                {
                    mLastFatalFilter = mFatalFilter;
                    mSettingComponent.SetBool("Debugger.Console.FatalFilter", mFatalFilter);
                }
            }

            public void OnDraw()
            {
                RefreshCount();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Clear All", GUILayout.Width(100f)))
                    {
                        Clear();
                    }
                    mLockScroll = GUILayout.Toggle(mLockScroll, "Lock Scroll", GUILayout.Width(90f));
                    GUILayout.FlexibleSpace();
                    mInfoFilter = GUILayout.Toggle(mInfoFilter, Utility.Text.Format("Info ({0})", mInfoCount), GUILayout.Width(90f));
                    mWarningFilter = GUILayout.Toggle(mWarningFilter, Utility.Text.Format("Warning ({0})", mWarningCount), GUILayout.Width(90f));
                    mErrorFilter = GUILayout.Toggle(mErrorFilter, Utility.Text.Format("Error ({0})", mErrorCount), GUILayout.Width(90f));
                    mFatalFilter = GUILayout.Toggle(mFatalFilter, Utility.Text.Format("Fatal ({0})", mFatalCount), GUILayout.Width(90f));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    if (mLockScroll)
                    {
                        mLogScrollPosition.y = float.MaxValue;
                    }

                    mLogScrollPosition = GUILayout.BeginScrollView(mLogScrollPosition);
                    {
                        bool selected = false;
                        foreach (LogNode logNode in mLogNodes)
                        {
                            switch (logNode.LogType)
                            {
                                case LogType.Log:
                                    if (!mInfoFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogType.Warning:
                                    if (!mWarningFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogType.Error:
                                    if (!mErrorFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogType.Exception:
                                    if (!mFatalFilter)
                                    {
                                        continue;
                                    }
                                    break;
                            }
                            if (GUILayout.Toggle(mSelectedNode == logNode, GetLogString(logNode)))
                            {
                                selected = true;
                                if (mSelectedNode != logNode)
                                {
                                    mSelectedNode = logNode;
                                    mStackScrollPosition = Vector2.zero;
                                }
                            }
                        }
                        if (!selected)
                        {
                            mSelectedNode = null;
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    mStackScrollPosition = GUILayout.BeginScrollView(mStackScrollPosition, GUILayout.Height(100f));
                    {
                        if (mSelectedNode != null)
                        {
                            Color32 color = GetLogStringColor(mSelectedNode.LogType);
                            if (GUILayout.Button(Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}><b>{4}</b></color>{6}{6}{5}", color.r, color.g, color.b, color.a, mSelectedNode.LogMessage, mSelectedNode.StackTrack, Environment.NewLine), "label"))
                            {
                                CopyToClipboard(Utility.Text.Format("{0}{2}{2}{1}", mSelectedNode.LogMessage, mSelectedNode.StackTrack, Environment.NewLine));
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }

            private void Clear()
            {
                mLogNodes.Clear();
            }

            public void RefreshCount()
            {
                mInfoCount = 0;
                mWarningCount = 0;
                mErrorCount = 0;
                mFatalCount = 0;
                foreach (LogNode logNode in mLogNodes)
                {
                    switch (logNode.LogType)
                    {
                        case LogType.Log:
                            mInfoCount++;
                            break;

                        case LogType.Warning:
                            mWarningCount++;
                            break;

                        case LogType.Error:
                            mErrorCount++;
                            break;

                        case LogType.Exception:
                            mFatalCount++;
                            break;
                    }
                }
            }

            public void GetRecentLogs(List<LogNode> results)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                results.Clear();
                foreach (LogNode logNode in mLogNodes)
                {
                    results.Add(logNode);
                }
            }

            public void GetRecentLogs(List<LogNode> results, int count)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                if (count <= 0)
                {
                    Log.Error("Count is invalid.");
                    return;
                }

                int position = mLogNodes.Count - count;
                if (position < 0)
                {
                    position = 0;
                }

                int index = 0;
                results.Clear();
                foreach (LogNode logNode in mLogNodes)
                {
                    if (index++ < position)
                    {
                        continue;
                    }

                    results.Add(logNode);
                }
            }

            private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
            {
                if (logType == LogType.Assert)
                {
                    logType = LogType.Error;
                }

                mLogNodes.Enqueue(LogNode.Create(logType, logMessage, stackTrace));
                while (mLogNodes.Count > mMaxLine)
                {
                    ReferencePool.Release(mLogNodes.Dequeue());
                }
            }

            private string GetLogString(LogNode logNode)
            {
                Color32 color = GetLogStringColor(logNode.LogType);
                return Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}>[{4:HH:mm:ss.fff}][{5}] {6}</color>", color.r, color.g, color.b, color.a, logNode.LogTime.ToLocalTime(), logNode.LogFrameCount, logNode.LogMessage);
            }

            internal Color32 GetLogStringColor(LogType logType)
            {
                Color32 color = Color.white;
                switch (logType)
                {
                    case LogType.Log:
                        color = mInfoColor;
                        break;

                    case LogType.Warning:
                        color = mWarningColor;
                        break;

                    case LogType.Error:
                        color = mErrorColor;
                        break;

                    case LogType.Exception:
                        color = mFatalColor;
                        break;
                }

                return color;
            }
        }
    }
}

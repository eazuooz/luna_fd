//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        public sealed class LogNode : IReference
        {
            private DateTime mLogTime;
            private int mLogFrameCount;
            private LogType mLogType;
            private string mLogMessage;
            private string mStackTrack;

            public LogNode()
            {
                mLogTime = default(DateTime);
                mLogFrameCount = 0;
                mLogType = LogType.Error;
                mLogMessage = null;
                mStackTrack = null;
            }

            public DateTime LogTime
            {
                get
                {
                    return mLogTime;
                }
            }

            public int LogFrameCount
            {
                get
                {
                    return mLogFrameCount;
                }
            }

            public LogType LogType
            {
                get
                {
                    return mLogType;
                }
            }

            public string LogMessage
            {
                get
                {
                    return mLogMessage;
                }
            }

            public string StackTrack
            {
                get
                {
                    return mStackTrack;
                }
            }

            public static LogNode Create(LogType logType, string logMessage, string stackTrack)
            {
                LogNode logNode = ReferencePool.Acquire<LogNode>();
                logNode.mLogTime = DateTime.UtcNow;
                logNode.mLogFrameCount = Time.frameCount;
                logNode.mLogType = logType;
                logNode.mLogMessage = logMessage;
                logNode.mStackTrack = stackTrack;
                return logNode;
            }

            public void Clear()
            {
                mLogTime = default(DateTime);
                mLogFrameCount = 0;
                mLogType = LogType.Error;
                mLogMessage = null;
                mStackTrack = null;
            }
        }
    }
}

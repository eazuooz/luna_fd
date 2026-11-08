//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadConfigSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadConfigSuccessEventArgs).GetHashCode();

        public LoadConfigSuccessEventArgs()
        {
            ConfigAssetName = null;
            Duration = 0f;
            UserData = null;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public string ConfigAssetName
        {
            get;
            private set;
        }

        public float Duration
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadConfigSuccessEventArgs Create(ReadDataSuccessEventArgs e)
        {
            LoadConfigSuccessEventArgs loadConfigSuccessEventArgs = ReferencePool.Acquire<LoadConfigSuccessEventArgs>();
            loadConfigSuccessEventArgs.ConfigAssetName = e.DataAssetName;
            loadConfigSuccessEventArgs.Duration = e.Duration;
            loadConfigSuccessEventArgs.UserData = e.UserData;
            return loadConfigSuccessEventArgs;
        }

        public override void Clear()
        {
            ConfigAssetName = null;
            Duration = 0f;
            UserData = null;
        }
    }
}

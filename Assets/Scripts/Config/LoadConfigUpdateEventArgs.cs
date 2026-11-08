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
    public sealed class LoadConfigUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadConfigUpdateEventArgs).GetHashCode();

        public LoadConfigUpdateEventArgs()
        {
            ConfigAssetName = null;
            Progress = 0f;
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

        public float Progress
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadConfigUpdateEventArgs Create(ReadDataUpdateEventArgs e)
        {
            LoadConfigUpdateEventArgs loadConfigUpdateEventArgs = ReferencePool.Acquire<LoadConfigUpdateEventArgs>();
            loadConfigUpdateEventArgs.ConfigAssetName = e.DataAssetName;
            loadConfigUpdateEventArgs.Progress = e.Progress;
            loadConfigUpdateEventArgs.UserData = e.UserData;
            return loadConfigUpdateEventArgs;
        }

        public override void Clear()
        {
            ConfigAssetName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}

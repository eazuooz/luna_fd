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
    public sealed class LoadDataTableSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDataTableSuccessEventArgs).GetHashCode();

        public LoadDataTableSuccessEventArgs()
        {
            DataTableAssetName = null;
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

        public string DataTableAssetName
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

        public static LoadDataTableSuccessEventArgs Create(ReadDataSuccessEventArgs e)
        {
            LoadDataTableSuccessEventArgs loadDataTableSuccessEventArgs = ReferencePool.Acquire<LoadDataTableSuccessEventArgs>();
            loadDataTableSuccessEventArgs.DataTableAssetName = e.DataAssetName;
            loadDataTableSuccessEventArgs.Duration = e.Duration;
            loadDataTableSuccessEventArgs.UserData = e.UserData;
            return loadDataTableSuccessEventArgs;
        }

        public override void Clear()
        {
            DataTableAssetName = null;
            Duration = 0f;
            UserData = null;
        }
    }
}

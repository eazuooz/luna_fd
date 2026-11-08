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
    public sealed class LoadConfigDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadConfigDependencyAssetEventArgs).GetHashCode();

        public LoadConfigDependencyAssetEventArgs()
        {
            ConfigAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
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

        public string DependencyAssetName
        {
            get;
            private set;
        }

        public int LoadedCount
        {
            get;
            private set;
        }

        public int TotalCount
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadConfigDependencyAssetEventArgs Create(ReadDataDependencyAssetEventArgs e)
        {
            LoadConfigDependencyAssetEventArgs loadConfigDependencyAssetEventArgs = ReferencePool.Acquire<LoadConfigDependencyAssetEventArgs>();
            loadConfigDependencyAssetEventArgs.ConfigAssetName = e.DataAssetName;
            loadConfigDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            loadConfigDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            loadConfigDependencyAssetEventArgs.TotalCount = e.TotalCount;
            loadConfigDependencyAssetEventArgs.UserData = e.UserData;
            return loadConfigDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            ConfigAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}

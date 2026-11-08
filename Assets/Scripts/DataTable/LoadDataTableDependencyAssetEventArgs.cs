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
    public sealed class LoadDataTableDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDataTableDependencyAssetEventArgs).GetHashCode();

        public LoadDataTableDependencyAssetEventArgs()
        {
            DataTableAssetName = null;
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

        public string DataTableAssetName
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

        public static LoadDataTableDependencyAssetEventArgs Create(ReadDataDependencyAssetEventArgs e)
        {
            LoadDataTableDependencyAssetEventArgs loadDataTableDependencyAssetEventArgs = ReferencePool.Acquire<LoadDataTableDependencyAssetEventArgs>();
            loadDataTableDependencyAssetEventArgs.DataTableAssetName = e.DataAssetName;
            loadDataTableDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            loadDataTableDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            loadDataTableDependencyAssetEventArgs.TotalCount = e.TotalCount;
            loadDataTableDependencyAssetEventArgs.UserData = e.UserData;
            return loadDataTableDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            DataTableAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}

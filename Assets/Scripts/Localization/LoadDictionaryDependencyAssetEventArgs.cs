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
    public sealed class LoadDictionaryDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDictionaryDependencyAssetEventArgs).GetHashCode();

        public LoadDictionaryDependencyAssetEventArgs()
        {
            DictionaryAssetName = null;
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

        public string DictionaryAssetName
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

        public static LoadDictionaryDependencyAssetEventArgs Create(ReadDataDependencyAssetEventArgs e)
        {
            LoadDictionaryDependencyAssetEventArgs loadDictionaryDependencyAssetEventArgs = ReferencePool.Acquire<LoadDictionaryDependencyAssetEventArgs>();
            loadDictionaryDependencyAssetEventArgs.DictionaryAssetName = e.DataAssetName;
            loadDictionaryDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            loadDictionaryDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            loadDictionaryDependencyAssetEventArgs.TotalCount = e.TotalCount;
            loadDictionaryDependencyAssetEventArgs.UserData = e.UserData;
            return loadDictionaryDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            DictionaryAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}

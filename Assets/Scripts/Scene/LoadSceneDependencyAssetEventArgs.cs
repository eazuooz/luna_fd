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
    public sealed class LoadSceneDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadSceneDependencyAssetEventArgs).GetHashCode();

        public LoadSceneDependencyAssetEventArgs()
        {
            SceneAssetName = null;
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

        public string SceneAssetName
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

        public static LoadSceneDependencyAssetEventArgs Create(GameFramework.Scene.LoadSceneDependencyAssetEventArgs e)
        {
            LoadSceneDependencyAssetEventArgs loadSceneDependencyAssetEventArgs = ReferencePool.Acquire<LoadSceneDependencyAssetEventArgs>();
            loadSceneDependencyAssetEventArgs.SceneAssetName = e.SceneAssetName;
            loadSceneDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            loadSceneDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            loadSceneDependencyAssetEventArgs.TotalCount = e.TotalCount;
            loadSceneDependencyAssetEventArgs.UserData = e.UserData;
            return loadSceneDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}

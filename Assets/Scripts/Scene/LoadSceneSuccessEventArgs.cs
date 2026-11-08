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
    public sealed class LoadSceneSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadSceneSuccessEventArgs).GetHashCode();

        public LoadSceneSuccessEventArgs()
        {
            SceneAssetName = null;
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

        public string SceneAssetName
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

        public static LoadSceneSuccessEventArgs Create(GameFramework.Scene.LoadSceneSuccessEventArgs e)
        {
            LoadSceneSuccessEventArgs loadSceneSuccessEventArgs = ReferencePool.Acquire<LoadSceneSuccessEventArgs>();
            loadSceneSuccessEventArgs.SceneAssetName = e.SceneAssetName;
            loadSceneSuccessEventArgs.Duration = e.Duration;
            loadSceneSuccessEventArgs.UserData = e.UserData;
            return loadSceneSuccessEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            Duration = 0f;
            UserData = null;
        }
    }
}

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
    public sealed class LoadSceneFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadSceneFailureEventArgs).GetHashCode();

        public LoadSceneFailureEventArgs()
        {
            SceneAssetName = null;
            ErrorMessage = null;
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

        public string ErrorMessage
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadSceneFailureEventArgs Create(GameFramework.Scene.LoadSceneFailureEventArgs e)
        {
            LoadSceneFailureEventArgs loadSceneFailureEventArgs = ReferencePool.Acquire<LoadSceneFailureEventArgs>();
            loadSceneFailureEventArgs.SceneAssetName = e.SceneAssetName;
            loadSceneFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadSceneFailureEventArgs.UserData = e.UserData;
            return loadSceneFailureEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}

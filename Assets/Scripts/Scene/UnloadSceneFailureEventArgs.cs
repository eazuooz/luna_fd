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
    public sealed class UnloadSceneFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UnloadSceneFailureEventArgs).GetHashCode();

        public UnloadSceneFailureEventArgs()
        {
            SceneAssetName = null;
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

        public object UserData
        {
            get;
            private set;
        }

        public static UnloadSceneFailureEventArgs Create(GameFramework.Scene.UnloadSceneFailureEventArgs e)
        {
            UnloadSceneFailureEventArgs unloadSceneFailureEventArgs = ReferencePool.Acquire<UnloadSceneFailureEventArgs>();
            unloadSceneFailureEventArgs.SceneAssetName = e.SceneAssetName;
            unloadSceneFailureEventArgs.UserData = e.UserData;
            return unloadSceneFailureEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            UserData = null;
        }
    }
}

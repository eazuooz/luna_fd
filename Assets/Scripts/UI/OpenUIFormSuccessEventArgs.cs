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
    public sealed class OpenUIFormSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(OpenUIFormSuccessEventArgs).GetHashCode();

        public OpenUIFormSuccessEventArgs()
        {
            UIForm = null;
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

        public UIForm UIForm
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

        public static OpenUIFormSuccessEventArgs Create(GameFramework.UI.OpenUIFormSuccessEventArgs e)
        {
            OpenUIFormSuccessEventArgs openUIFormSuccessEventArgs = ReferencePool.Acquire<OpenUIFormSuccessEventArgs>();
            openUIFormSuccessEventArgs.UIForm = (UIForm)e.UIForm;
            openUIFormSuccessEventArgs.Duration = e.Duration;
            openUIFormSuccessEventArgs.UserData = e.UserData;
            return openUIFormSuccessEventArgs;
        }

        public override void Clear()
        {
            UIForm = null;
            Duration = 0f;
            UserData = null;
        }
    }
}

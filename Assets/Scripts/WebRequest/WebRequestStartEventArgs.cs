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
    public sealed class WebRequestStartEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(WebRequestStartEventArgs).GetHashCode();

        public WebRequestStartEventArgs()
        {
            SerialId = 0;
            WebRequestUri = null;
            UserData = null;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public int SerialId
        {
            get;
            private set;
        }

        public string WebRequestUri
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static WebRequestStartEventArgs Create(GameFramework.WebRequest.WebRequestStartEventArgs e)
        {
            WWWFormInfo wwwFormInfo = (WWWFormInfo)e.UserData;
            WebRequestStartEventArgs webRequestStartEventArgs = ReferencePool.Acquire<WebRequestStartEventArgs>();
            webRequestStartEventArgs.SerialId = e.SerialId;
            webRequestStartEventArgs.WebRequestUri = e.WebRequestUri;
            webRequestStartEventArgs.UserData = wwwFormInfo.UserData;
            return webRequestStartEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            WebRequestUri = null;
            UserData = null;
        }
    }
}

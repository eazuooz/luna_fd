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
    public sealed class WebRequestFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(WebRequestFailureEventArgs).GetHashCode();

        public WebRequestFailureEventArgs()
        {
            SerialId = 0;
            WebRequestUri = null;
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

        public static WebRequestFailureEventArgs Create(GameFramework.WebRequest.WebRequestFailureEventArgs e)
        {
            WWWFormInfo wwwFormInfo = (WWWFormInfo)e.UserData;
            WebRequestFailureEventArgs webRequestFailureEventArgs = ReferencePool.Acquire<WebRequestFailureEventArgs>();
            webRequestFailureEventArgs.SerialId = e.SerialId;
            webRequestFailureEventArgs.WebRequestUri = e.WebRequestUri;
            webRequestFailureEventArgs.ErrorMessage = e.ErrorMessage;
            webRequestFailureEventArgs.UserData = wwwFormInfo.UserData;
            ReferencePool.Release(wwwFormInfo);
            return webRequestFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            WebRequestUri = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}

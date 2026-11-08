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
    public sealed class DownloadFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DownloadFailureEventArgs).GetHashCode();

        public DownloadFailureEventArgs()
        {
            SerialId = 0;
            DownloadPath = null;
            DownloadUri = null;
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

        public string DownloadPath
        {
            get;
            private set;
        }

        public string DownloadUri
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

        public static DownloadFailureEventArgs Create(GameFramework.Download.DownloadFailureEventArgs e)
        {
            DownloadFailureEventArgs downloadFailureEventArgs = ReferencePool.Acquire<DownloadFailureEventArgs>();
            downloadFailureEventArgs.SerialId = e.SerialId;
            downloadFailureEventArgs.DownloadPath = e.DownloadPath;
            downloadFailureEventArgs.DownloadUri = e.DownloadUri;
            downloadFailureEventArgs.ErrorMessage = e.ErrorMessage;
            downloadFailureEventArgs.UserData = e.UserData;
            return downloadFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            DownloadPath = null;
            DownloadUri = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}

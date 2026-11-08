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
    public sealed class DownloadSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DownloadSuccessEventArgs).GetHashCode();

        public DownloadSuccessEventArgs()
        {
            SerialId = 0;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0L;
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

        public long CurrentLength
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static DownloadSuccessEventArgs Create(GameFramework.Download.DownloadSuccessEventArgs e)
        {
            DownloadSuccessEventArgs downloadSuccessEventArgs = ReferencePool.Acquire<DownloadSuccessEventArgs>();
            downloadSuccessEventArgs.SerialId = e.SerialId;
            downloadSuccessEventArgs.DownloadPath = e.DownloadPath;
            downloadSuccessEventArgs.DownloadUri = e.DownloadUri;
            downloadSuccessEventArgs.CurrentLength = e.CurrentLength;
            downloadSuccessEventArgs.UserData = e.UserData;
            return downloadSuccessEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0L;
            UserData = null;
        }
    }
}

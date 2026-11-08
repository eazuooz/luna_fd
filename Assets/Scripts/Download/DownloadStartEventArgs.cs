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
    public sealed class DownloadStartEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DownloadStartEventArgs).GetHashCode();

        public DownloadStartEventArgs()
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

        public static DownloadStartEventArgs Create(GameFramework.Download.DownloadStartEventArgs e)
        {
            DownloadStartEventArgs downloadStartEventArgs = ReferencePool.Acquire<DownloadStartEventArgs>();
            downloadStartEventArgs.SerialId = e.SerialId;
            downloadStartEventArgs.DownloadPath = e.DownloadPath;
            downloadStartEventArgs.DownloadUri = e.DownloadUri;
            downloadStartEventArgs.CurrentLength = e.CurrentLength;
            downloadStartEventArgs.UserData = e.UserData;
            return downloadStartEventArgs;
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

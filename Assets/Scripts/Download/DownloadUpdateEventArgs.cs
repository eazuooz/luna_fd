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
    public sealed class DownloadUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DownloadUpdateEventArgs).GetHashCode();

        public DownloadUpdateEventArgs()
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

        public static DownloadUpdateEventArgs Create(GameFramework.Download.DownloadUpdateEventArgs e)
        {
            DownloadUpdateEventArgs downloadUpdateEventArgs = ReferencePool.Acquire<DownloadUpdateEventArgs>();
            downloadUpdateEventArgs.SerialId = e.SerialId;
            downloadUpdateEventArgs.DownloadPath = e.DownloadPath;
            downloadUpdateEventArgs.DownloadUri = e.DownloadUri;
            downloadUpdateEventArgs.CurrentLength = e.CurrentLength;
            downloadUpdateEventArgs.UserData = e.UserData;
            return downloadUpdateEventArgs;
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

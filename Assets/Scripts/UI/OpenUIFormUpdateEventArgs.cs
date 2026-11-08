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
    public sealed class OpenUIFormUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(OpenUIFormUpdateEventArgs).GetHashCode();

        public OpenUIFormUpdateEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            Progress = 0f;
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

        public string UIFormAssetName
        {
            get;
            private set;
        }

        public string UIGroupName
        {
            get;
            private set;
        }

        public bool PauseCoveredUIForm
        {
            get;
            private set;
        }

        public float Progress
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static OpenUIFormUpdateEventArgs Create(GameFramework.UI.OpenUIFormUpdateEventArgs e)
        {
            OpenUIFormUpdateEventArgs openUIFormUpdateEventArgs = ReferencePool.Acquire<OpenUIFormUpdateEventArgs>();
            openUIFormUpdateEventArgs.SerialId = e.SerialId;
            openUIFormUpdateEventArgs.UIFormAssetName = e.UIFormAssetName;
            openUIFormUpdateEventArgs.UIGroupName = e.UIGroupName;
            openUIFormUpdateEventArgs.PauseCoveredUIForm = e.PauseCoveredUIForm;
            openUIFormUpdateEventArgs.Progress = e.Progress;
            openUIFormUpdateEventArgs.UserData = e.UserData;
            return openUIFormUpdateEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            Progress = 0f;
            UserData = null;
        }
    }
}

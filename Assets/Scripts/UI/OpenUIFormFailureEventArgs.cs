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
    public sealed class OpenUIFormFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(OpenUIFormFailureEventArgs).GetHashCode();

        public OpenUIFormFailureEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
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

        public static OpenUIFormFailureEventArgs Create(GameFramework.UI.OpenUIFormFailureEventArgs e)
        {
            OpenUIFormFailureEventArgs openUIFormFailureEventArgs = ReferencePool.Acquire<OpenUIFormFailureEventArgs>();
            openUIFormFailureEventArgs.SerialId = e.SerialId;
            openUIFormFailureEventArgs.UIFormAssetName = e.UIFormAssetName;
            openUIFormFailureEventArgs.UIGroupName = e.UIGroupName;
            openUIFormFailureEventArgs.PauseCoveredUIForm = e.PauseCoveredUIForm;
            openUIFormFailureEventArgs.ErrorMessage = e.ErrorMessage;
            openUIFormFailureEventArgs.UserData = e.UserData;
            return openUIFormFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            ErrorMessage = null;
            UserData = null;
        }
    }
}

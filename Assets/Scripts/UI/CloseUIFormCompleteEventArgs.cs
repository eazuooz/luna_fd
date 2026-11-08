//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;
using GameFramework.UI;

namespace UnityGameFramework.Runtime
{
    public sealed class CloseUIFormCompleteEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(CloseUIFormCompleteEventArgs).GetHashCode();

        public CloseUIFormCompleteEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroup = null;
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

        public IUIGroup UIGroup
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static CloseUIFormCompleteEventArgs Create(GameFramework.UI.CloseUIFormCompleteEventArgs e)
        {
            CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = ReferencePool.Acquire<CloseUIFormCompleteEventArgs>();
            closeUIFormCompleteEventArgs.SerialId = e.SerialId;
            closeUIFormCompleteEventArgs.UIFormAssetName = e.UIFormAssetName;
            closeUIFormCompleteEventArgs.UIGroup = e.UIGroup;
            closeUIFormCompleteEventArgs.UserData = e.UserData;
            return closeUIFormCompleteEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroup = null;
            UserData = null;
        }
    }
}

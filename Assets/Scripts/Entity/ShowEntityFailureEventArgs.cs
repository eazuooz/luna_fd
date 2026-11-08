//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;
using System;

namespace UnityGameFramework.Runtime
{
    public sealed class ShowEntityFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ShowEntityFailureEventArgs).GetHashCode();

        public ShowEntityFailureEventArgs()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
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

        public int EntityId
        {
            get;
            private set;
        }

        public Type EntityLogicType
        {
            get;
            private set;
        }

        public string EntityAssetName
        {
            get;
            private set;
        }

        public string EntityGroupName
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

        public static ShowEntityFailureEventArgs Create(GameFramework.Entity.ShowEntityFailureEventArgs e)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)e.UserData;
            ShowEntityFailureEventArgs showEntityFailureEventArgs = ReferencePool.Acquire<ShowEntityFailureEventArgs>();
            showEntityFailureEventArgs.EntityId = e.EntityId;
            showEntityFailureEventArgs.EntityLogicType = showEntityInfo.EntityLogicType;
            showEntityFailureEventArgs.EntityAssetName = e.EntityAssetName;
            showEntityFailureEventArgs.EntityGroupName = e.EntityGroupName;
            showEntityFailureEventArgs.ErrorMessage = e.ErrorMessage;
            showEntityFailureEventArgs.UserData = showEntityInfo.UserData;
            ReferencePool.Release(showEntityInfo);
            return showEntityFailureEventArgs;
        }

        public override void Clear()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}

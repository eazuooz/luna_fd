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
    public sealed class ShowEntityUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ShowEntityUpdateEventArgs).GetHashCode();

        public ShowEntityUpdateEventArgs()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
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

        public static ShowEntityUpdateEventArgs Create(GameFramework.Entity.ShowEntityUpdateEventArgs e)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)e.UserData;
            ShowEntityUpdateEventArgs showEntityUpdateEventArgs = ReferencePool.Acquire<ShowEntityUpdateEventArgs>();
            showEntityUpdateEventArgs.EntityId = e.EntityId;
            showEntityUpdateEventArgs.EntityLogicType = showEntityInfo.EntityLogicType;
            showEntityUpdateEventArgs.EntityAssetName = e.EntityAssetName;
            showEntityUpdateEventArgs.EntityGroupName = e.EntityGroupName;
            showEntityUpdateEventArgs.Progress = e.Progress;
            showEntityUpdateEventArgs.UserData = showEntityInfo.UserData;
            return showEntityUpdateEventArgs;
        }

        public override void Clear()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}

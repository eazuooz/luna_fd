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
    public sealed class ShowEntitySuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ShowEntitySuccessEventArgs).GetHashCode();

        public ShowEntitySuccessEventArgs()
        {
            EntityLogicType = null;
            Entity = null;
            Duration = 0f;
            UserData = null;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public Type EntityLogicType
        {
            get;
            private set;
        }

        public Entity Entity
        {
            get;
            private set;
        }

        public float Duration
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static ShowEntitySuccessEventArgs Create(GameFramework.Entity.ShowEntitySuccessEventArgs e)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)e.UserData;
            ShowEntitySuccessEventArgs showEntitySuccessEventArgs = ReferencePool.Acquire<ShowEntitySuccessEventArgs>();
            showEntitySuccessEventArgs.EntityLogicType = showEntityInfo.EntityLogicType;
            showEntitySuccessEventArgs.Entity = (Entity)e.Entity;
            showEntitySuccessEventArgs.Duration = e.Duration;
            showEntitySuccessEventArgs.UserData = showEntityInfo.UserData;
            ReferencePool.Release(showEntityInfo);
            return showEntitySuccessEventArgs;
        }

        public override void Clear()
        {
            EntityLogicType = null;
            Entity = null;
            Duration = 0f;
            UserData = null;
        }
    }
}

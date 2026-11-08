//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Entity;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class HideEntityCompleteEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(HideEntityCompleteEventArgs).GetHashCode();

        public HideEntityCompleteEventArgs()
        {
            EntityId = 0;
            EntityAssetName = null;
            EntityGroup = null;
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

        public string EntityAssetName
        {
            get;
            private set;
        }

        public IEntityGroup EntityGroup
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static HideEntityCompleteEventArgs Create(GameFramework.Entity.HideEntityCompleteEventArgs e)
        {
            HideEntityCompleteEventArgs hideEntityCompleteEventArgs = ReferencePool.Acquire<HideEntityCompleteEventArgs>();
            hideEntityCompleteEventArgs.EntityId = e.EntityId;
            hideEntityCompleteEventArgs.EntityAssetName = e.EntityAssetName;
            hideEntityCompleteEventArgs.EntityGroup = e.EntityGroup;
            hideEntityCompleteEventArgs.UserData = e.UserData;
            return hideEntityCompleteEventArgs;
        }

        public override void Clear()
        {
            EntityId = 0;
            EntityAssetName = null;
            EntityGroup = null;
            UserData = null;
        }
    }
}

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
    public sealed class ShowEntityDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ShowEntityDependencyAssetEventArgs).GetHashCode();

        public ShowEntityDependencyAssetEventArgs()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
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

        public string DependencyAssetName
        {
            get;
            private set;
        }

        public int LoadedCount
        {
            get;
            private set;
        }

        public int TotalCount
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static ShowEntityDependencyAssetEventArgs Create(GameFramework.Entity.ShowEntityDependencyAssetEventArgs e)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)e.UserData;
            ShowEntityDependencyAssetEventArgs showEntityDependencyAssetEventArgs = ReferencePool.Acquire<ShowEntityDependencyAssetEventArgs>();
            showEntityDependencyAssetEventArgs.EntityId = e.EntityId;
            showEntityDependencyAssetEventArgs.EntityLogicType = showEntityInfo.EntityLogicType;
            showEntityDependencyAssetEventArgs.EntityAssetName = e.EntityAssetName;
            showEntityDependencyAssetEventArgs.EntityGroupName = e.EntityGroupName;
            showEntityDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            showEntityDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            showEntityDependencyAssetEventArgs.TotalCount = e.TotalCount;
            showEntityDependencyAssetEventArgs.UserData = showEntityInfo.UserData;
            return showEntityDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}

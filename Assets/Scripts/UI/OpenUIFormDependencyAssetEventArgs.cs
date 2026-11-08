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
    public sealed class OpenUIFormDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(OpenUIFormDependencyAssetEventArgs).GetHashCode();

        public OpenUIFormDependencyAssetEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
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

        public static OpenUIFormDependencyAssetEventArgs Create(GameFramework.UI.OpenUIFormDependencyAssetEventArgs e)
        {
            OpenUIFormDependencyAssetEventArgs openUIFormDependencyAssetEventArgs = ReferencePool.Acquire<OpenUIFormDependencyAssetEventArgs>();
            openUIFormDependencyAssetEventArgs.SerialId = e.SerialId;
            openUIFormDependencyAssetEventArgs.UIFormAssetName = e.UIFormAssetName;
            openUIFormDependencyAssetEventArgs.UIGroupName = e.UIGroupName;
            openUIFormDependencyAssetEventArgs.PauseCoveredUIForm = e.PauseCoveredUIForm;
            openUIFormDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            openUIFormDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            openUIFormDependencyAssetEventArgs.TotalCount = e.TotalCount;
            openUIFormDependencyAssetEventArgs.UserData = e.UserData;
            return openUIFormDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}

//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;
using GameFramework.Sound;

namespace UnityGameFramework.Runtime
{
    public sealed class PlaySoundDependencyAssetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlaySoundDependencyAssetEventArgs).GetHashCode();

        public PlaySoundDependencyAssetEventArgs()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            BindingEntity = null;
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

        public string SoundAssetName
        {
            get;
            private set;
        }

        public string SoundGroupName
        {
            get;
            private set;
        }

        public PlaySoundParams PlaySoundParams
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

        public Entity BindingEntity
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static PlaySoundDependencyAssetEventArgs Create(GameFramework.Sound.PlaySoundDependencyAssetEventArgs e)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)e.UserData;
            PlaySoundDependencyAssetEventArgs playSoundDependencyAssetEventArgs = ReferencePool.Acquire<PlaySoundDependencyAssetEventArgs>();
            playSoundDependencyAssetEventArgs.SerialId = e.SerialId;
            playSoundDependencyAssetEventArgs.SoundAssetName = e.SoundAssetName;
            playSoundDependencyAssetEventArgs.SoundGroupName = e.SoundGroupName;
            playSoundDependencyAssetEventArgs.PlaySoundParams = e.PlaySoundParams;
            playSoundDependencyAssetEventArgs.DependencyAssetName = e.DependencyAssetName;
            playSoundDependencyAssetEventArgs.LoadedCount = e.LoadedCount;
            playSoundDependencyAssetEventArgs.TotalCount = e.TotalCount;
            playSoundDependencyAssetEventArgs.BindingEntity = playSoundInfo.BindingEntity;
            playSoundDependencyAssetEventArgs.UserData = playSoundInfo.UserData;
            return playSoundDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            BindingEntity = null;
            UserData = null;
        }
    }
}

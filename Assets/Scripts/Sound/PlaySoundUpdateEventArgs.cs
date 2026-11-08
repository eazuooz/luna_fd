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
    public sealed class PlaySoundUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlaySoundUpdateEventArgs).GetHashCode();

        public PlaySoundUpdateEventArgs()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            Progress = 0f;
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

        public float Progress
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

        public static PlaySoundUpdateEventArgs Create(GameFramework.Sound.PlaySoundUpdateEventArgs e)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)e.UserData;
            PlaySoundUpdateEventArgs playSoundUpdateEventArgs = ReferencePool.Acquire<PlaySoundUpdateEventArgs>();
            playSoundUpdateEventArgs.SerialId = e.SerialId;
            playSoundUpdateEventArgs.SoundAssetName = e.SoundAssetName;
            playSoundUpdateEventArgs.SoundGroupName = e.SoundGroupName;
            playSoundUpdateEventArgs.PlaySoundParams = e.PlaySoundParams;
            playSoundUpdateEventArgs.Progress = e.Progress;
            playSoundUpdateEventArgs.BindingEntity = playSoundInfo.BindingEntity;
            playSoundUpdateEventArgs.UserData = playSoundInfo.UserData;
            return playSoundUpdateEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            Progress = 0f;
            BindingEntity = null;
            UserData = null;
        }
    }
}

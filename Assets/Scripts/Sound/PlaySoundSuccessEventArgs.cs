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
    public sealed class PlaySoundSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlaySoundSuccessEventArgs).GetHashCode();

        public PlaySoundSuccessEventArgs()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundAgent = null;
            Duration = 0f;
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

        public ISoundAgent SoundAgent
        {
            get;
            private set;
        }

        public float Duration
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

        public static PlaySoundSuccessEventArgs Create(GameFramework.Sound.PlaySoundSuccessEventArgs e)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)e.UserData;
            PlaySoundSuccessEventArgs playSoundSuccessEventArgs = ReferencePool.Acquire<PlaySoundSuccessEventArgs>();
            playSoundSuccessEventArgs.SerialId = e.SerialId;
            playSoundSuccessEventArgs.SoundAssetName = e.SoundAssetName;
            playSoundSuccessEventArgs.SoundAgent = e.SoundAgent;
            playSoundSuccessEventArgs.Duration = e.Duration;
            playSoundSuccessEventArgs.BindingEntity = playSoundInfo.BindingEntity;
            playSoundSuccessEventArgs.UserData = playSoundInfo.UserData;
            ReferencePool.Release(playSoundInfo);
            return playSoundSuccessEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundAgent = null;
            Duration = 0f;
            BindingEntity = null;
            UserData = null;
        }
    }
}

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
    public sealed class PlaySoundFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlaySoundFailureEventArgs).GetHashCode();

        public PlaySoundFailureEventArgs()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            BindingEntity = null;
            ErrorCode = 0;
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

        public Entity BindingEntity
        {
            get;
            private set;
        }

        public PlaySoundErrorCode ErrorCode
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

        public static PlaySoundFailureEventArgs Create(GameFramework.Sound.PlaySoundFailureEventArgs e)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)e.UserData;
            PlaySoundFailureEventArgs playSoundFailureEventArgs = ReferencePool.Acquire<PlaySoundFailureEventArgs>();
            playSoundFailureEventArgs.SerialId = e.SerialId;
            playSoundFailureEventArgs.SoundAssetName = e.SoundAssetName;
            playSoundFailureEventArgs.SoundGroupName = e.SoundGroupName;
            playSoundFailureEventArgs.PlaySoundParams = e.PlaySoundParams;
            playSoundFailureEventArgs.BindingEntity = playSoundInfo.BindingEntity;
            playSoundFailureEventArgs.ErrorCode = e.ErrorCode;
            playSoundFailureEventArgs.ErrorMessage = e.ErrorMessage;
            playSoundFailureEventArgs.UserData = playSoundInfo.UserData;
            ReferencePool.Release(playSoundInfo);
            return playSoundFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            BindingEntity = null;
            ErrorCode = 0;
            ErrorMessage = null;
            UserData = null;
        }
    }
}

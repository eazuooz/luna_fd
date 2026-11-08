//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    internal sealed class PlaySoundInfo : IReference
    {
        private Entity mBindingEntity;
        private Vector3 mWorldPosition;
        private object mUserData;

        public PlaySoundInfo()
        {
            mBindingEntity = null;
            mWorldPosition = Vector3.zero;
            mUserData = null;
        }

        public Entity BindingEntity
        {
            get
            {
                return mBindingEntity;
            }
        }

        public Vector3 WorldPosition
        {
            get
            {
                return mWorldPosition;
            }
        }

        public object UserData
        {
            get
            {
                return mUserData;
            }
        }

        public static PlaySoundInfo Create(Entity bindingEntity, Vector3 worldPosition, object userData)
        {
            PlaySoundInfo playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
            playSoundInfo.mBindingEntity = bindingEntity;
            playSoundInfo.mWorldPosition = worldPosition;
            playSoundInfo.mUserData = userData;
            return playSoundInfo;
        }

        public void Clear()
        {
            mBindingEntity = null;
            mWorldPosition = Vector3.zero;
            mUserData = null;
        }
    }
}

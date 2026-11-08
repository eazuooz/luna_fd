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
    internal sealed class AttachEntityInfo : IReference
    {
        private Transform mParentTransform;
        private object mUserData;

        public AttachEntityInfo()
        {
            mParentTransform = null;
            mUserData = null;
        }

        public Transform ParentTransform
        {
            get
            {
                return mParentTransform;
            }
        }

        public object UserData
        {
            get
            {
                return mUserData;
            }
        }

        public static AttachEntityInfo Create(Transform parentTransform, object userData)
        {
            AttachEntityInfo attachEntityInfo = ReferencePool.Acquire<AttachEntityInfo>();
            attachEntityInfo.mParentTransform = parentTransform;
            attachEntityInfo.mUserData = userData;
            return attachEntityInfo;
        }

        public void Clear()
        {
            mParentTransform = null;
            mUserData = null;
        }
    }
}

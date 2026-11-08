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
    internal sealed class WWWFormInfo : IReference
    {
        private WWWForm mWWWForm;
        private object mUserData;

        public WWWFormInfo()
        {
            mWWWForm = null;
            mUserData = null;
        }

        public WWWForm WWWForm
        {
            get
            {
                return mWWWForm;
            }
        }

        public object UserData
        {
            get
            {
                return mUserData;
            }
        }

        public static WWWFormInfo Create(WWWForm wwwForm, object userData)
        {
            WWWFormInfo wwwFormInfo = ReferencePool.Acquire<WWWFormInfo>();
            wwwFormInfo.mWWWForm = wwwForm;
            wwwFormInfo.mUserData = userData;
            return wwwFormInfo;
        }

        public void Clear()
        {
            mWWWForm = null;
            mUserData = null;
        }
    }
}

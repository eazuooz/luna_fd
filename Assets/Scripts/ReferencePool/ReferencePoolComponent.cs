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
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/ReferencePool")]
    public sealed class ReferencePoolComponent : GameFrameworkComponent
    {
        [SerializeField]
        private ReferenceStrictCheckType mEnableStrictCheck = ReferenceStrictCheckType.AlwaysEnable;

        public bool EnableStrictCheck
        {
            get
            {
                return ReferencePool.EnableStrictCheck;
            }
            set
            {
                ReferencePool.EnableStrictCheck = value;
                if (value)
                {
                    Log.Info("Strict checking is enabled for the Reference Pool. It will drastically affect the performance.");
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            switch (mEnableStrictCheck)
            {
                case ReferenceStrictCheckType.AlwaysEnable:
                    EnableStrictCheck = true;
                    break;

                case ReferenceStrictCheckType.OnlyEnableWhenDevelopment:
                    EnableStrictCheck = Debug.isDebugBuild;
                    break;

                case ReferenceStrictCheckType.OnlyEnableInEditor:
                    EnableStrictCheck = Application.isEditor;
                    break;

                default:
                    EnableStrictCheck = false;
                    break;
            }
        }
    }
}

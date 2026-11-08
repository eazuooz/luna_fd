//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class EntityGroup
        {
            [SerializeField]
            private string mName = null;

            [SerializeField]
            private float mInstanceAutoReleaseInterval = 60f;

            [SerializeField]
            private int mInstanceCapacity = 16;

            [SerializeField]
            private float mInstanceExpireTime = 60f;

            [SerializeField]
            private int mInstancePriority = 0;

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public float InstanceAutoReleaseInterval
            {
                get
                {
                    return mInstanceAutoReleaseInterval;
                }
            }

            public int InstanceCapacity
            {
                get
                {
                    return mInstanceCapacity;
                }
            }

            public float InstanceExpireTime
            {
                get
                {
                    return mInstanceExpireTime;
                }
            }

            public int InstancePriority
            {
                get
                {
                    return mInstancePriority;
                }
            }
        }
    }
}

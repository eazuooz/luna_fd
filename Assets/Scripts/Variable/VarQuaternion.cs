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
    public sealed class VarQuaternion : Variable<Quaternion>
    {
        public VarQuaternion()
        {
        }

        public static implicit operator VarQuaternion(Quaternion value)
        {
            VarQuaternion varValue = ReferencePool.Acquire<VarQuaternion>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Quaternion(VarQuaternion value)
        {
            return value.Value;
        }
    }
}

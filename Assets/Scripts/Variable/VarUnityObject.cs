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
    public sealed class VarUnityObject : Variable<Object>
    {
        public VarUnityObject()
        {
        }

        public static implicit operator VarUnityObject(Object value)
        {
            VarUnityObject varValue = ReferencePool.Acquire<VarUnityObject>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Object(VarUnityObject value)
        {
            return value.Value;
        }
    }
}

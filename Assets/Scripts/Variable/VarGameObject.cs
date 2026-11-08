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
    public sealed class VarGameObject : Variable<GameObject>
    {
        public VarGameObject()
        {
        }

        public static implicit operator VarGameObject(GameObject value)
        {
            VarGameObject varValue = ReferencePool.Acquire<VarGameObject>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator GameObject(VarGameObject value)
        {
            return value.Value;
        }
    }
}

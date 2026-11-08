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
    public sealed class VarVector2 : Variable<Vector2>
    {
        public VarVector2()
        {
        }

        public static implicit operator VarVector2(Vector2 value)
        {
            VarVector2 varValue = ReferencePool.Acquire<VarVector2>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Vector2(VarVector2 value)
        {
            return value.Value;
        }
    }
}

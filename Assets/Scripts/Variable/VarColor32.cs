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
    public sealed class VarColor32 : Variable<Color32>
    {
        public VarColor32()
        {
        }

        public static implicit operator VarColor32(Color32 value)
        {
            VarColor32 varValue = ReferencePool.Acquire<VarColor32>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Color32(VarColor32 value)
        {
            return value.Value;
        }
    }
}

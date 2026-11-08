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
    public sealed class VarColor : Variable<Color>
    {
        public VarColor()
        {
        }

        public static implicit operator VarColor(Color value)
        {
            VarColor varValue = ReferencePool.Acquire<VarColor>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Color(VarColor value)
        {
            return value.Value;
        }
    }
}

//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using System;

namespace UnityGameFramework.Runtime
{
    public sealed class VarDateTime : Variable<DateTime>
    {
        public VarDateTime()
        {
        }

        public static implicit operator VarDateTime(DateTime value)
        {
            VarDateTime varValue = ReferencePool.Acquire<VarDateTime>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator DateTime(VarDateTime value)
        {
            return value.Value;
        }
    }
}

//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;

namespace UnityGameFramework.Runtime
{
    public sealed class VarDecimal : Variable<decimal>
    {
        public VarDecimal()
        {
        }

        public static implicit operator VarDecimal(decimal value)
        {
            VarDecimal varValue = ReferencePool.Acquire<VarDecimal>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator decimal(VarDecimal value)
        {
            return value.Value;
        }
    }
}

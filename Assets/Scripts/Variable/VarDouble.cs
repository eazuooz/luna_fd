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
    public sealed class VarDouble : Variable<double>
    {
        public VarDouble()
        {
        }

        public static implicit operator VarDouble(double value)
        {
            VarDouble varValue = ReferencePool.Acquire<VarDouble>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator double(VarDouble value)
        {
            return value.Value;
        }
    }
}

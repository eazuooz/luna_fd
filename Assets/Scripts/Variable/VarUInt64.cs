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
    public sealed class VarUInt64 : Variable<ulong>
    {
        public VarUInt64()
        {
        }

        public static implicit operator VarUInt64(ulong value)
        {
            VarUInt64 varValue = ReferencePool.Acquire<VarUInt64>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator ulong(VarUInt64 value)
        {
            return value.Value;
        }
    }
}

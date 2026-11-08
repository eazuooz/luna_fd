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
    public sealed class VarUInt32 : Variable<uint>
    {
        public VarUInt32()
        {
        }

        public static implicit operator VarUInt32(uint value)
        {
            VarUInt32 varValue = ReferencePool.Acquire<VarUInt32>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator uint(VarUInt32 value)
        {
            return value.Value;
        }
    }
}

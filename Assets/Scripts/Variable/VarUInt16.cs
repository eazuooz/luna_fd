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
    public sealed class VarUInt16 : Variable<ushort>
    {
        public VarUInt16()
        {
        }

        public static implicit operator VarUInt16(ushort value)
        {
            VarUInt16 varValue = ReferencePool.Acquire<VarUInt16>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator ushort(VarUInt16 value)
        {
            return value.Value;
        }
    }
}

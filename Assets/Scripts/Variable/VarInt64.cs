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
    public sealed class VarInt64 : Variable<long>
    {
        public VarInt64()
        {
        }

        public static implicit operator VarInt64(long value)
        {
            VarInt64 varValue = ReferencePool.Acquire<VarInt64>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator long(VarInt64 value)
        {
            return value.Value;
        }
    }
}

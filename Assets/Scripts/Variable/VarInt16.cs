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
    public sealed class VarInt16 : Variable<short>
    {
        public VarInt16()
        {
        }

        public static implicit operator VarInt16(short value)
        {
            VarInt16 varValue = ReferencePool.Acquire<VarInt16>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator short(VarInt16 value)
        {
            return value.Value;
        }
    }
}

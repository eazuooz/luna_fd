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
    public sealed class VarInt32 : Variable<int>
    {
        public VarInt32()
        {
        }

        public static implicit operator VarInt32(int value)
        {
            VarInt32 varValue = ReferencePool.Acquire<VarInt32>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator int(VarInt32 value)
        {
            return value.Value;
        }
    }
}

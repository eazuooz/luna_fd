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
    public sealed class VarByte : Variable<byte>
    {
        public VarByte()
        {
        }

        public static implicit operator VarByte(byte value)
        {
            VarByte varValue = ReferencePool.Acquire<VarByte>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator byte(VarByte value)
        {
            return value.Value;
        }
    }
}

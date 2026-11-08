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
    public sealed class VarSByte : Variable<sbyte>
    {
        public VarSByte()
        {
        }

        public static implicit operator VarSByte(sbyte value)
        {
            VarSByte varValue = ReferencePool.Acquire<VarSByte>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator sbyte(VarSByte value)
        {
            return value.Value;
        }
    }
}

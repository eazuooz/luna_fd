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
    public sealed class VarCharArray : Variable<char[]>
    {
        public VarCharArray()
        {
        }

        public static implicit operator VarCharArray(char[] value)
        {
            VarCharArray varValue = ReferencePool.Acquire<VarCharArray>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator char[](VarCharArray value)
        {
            return value.Value;
        }
    }
}

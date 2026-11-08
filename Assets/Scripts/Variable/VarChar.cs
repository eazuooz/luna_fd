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
    public sealed class VarChar : Variable<char>
    {
        public VarChar()
        {
        }

        public static implicit operator VarChar(char value)
        {
            VarChar varValue = ReferencePool.Acquire<VarChar>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator char(VarChar value)
        {
            return value.Value;
        }
    }
}

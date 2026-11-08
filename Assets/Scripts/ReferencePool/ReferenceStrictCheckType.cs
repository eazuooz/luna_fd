//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


namespace UnityGameFramework.Runtime
{
    public enum ReferenceStrictCheckType : byte
    {
        AlwaysEnable = 0,

        OnlyEnableWhenDevelopment,

        OnlyEnableInEditor,

        AlwaysDisable,
    }
}

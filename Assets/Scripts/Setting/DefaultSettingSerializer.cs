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
    public sealed class DefaultSettingSerializer : GameFrameworkSerializer<DefaultSetting>
    {
        private static readonly byte[] Header = new byte[] { (byte)'G', (byte)'F', (byte)'S' };

        public DefaultSettingSerializer()
        {
        }

        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}

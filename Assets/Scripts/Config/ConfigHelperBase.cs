//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Config;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class ConfigHelperBase : MonoBehaviour, IDataProviderHelper<IConfigManager>, IConfigHelper
    {
        public abstract bool ReadData(IConfigManager configManager, string configAssetName, object configAsset, object userData);

        public abstract bool ReadData(IConfigManager configManager, string configAssetName, byte[] configBytes, int startIndex, int length, object userData);

        public abstract bool ParseData(IConfigManager configManager, string configString, object userData);

        public abstract bool ParseData(IConfigManager configManager, byte[] configBytes, int startIndex, int length, object userData);

        public abstract void ReleaseDataAsset(IConfigManager configManager, object configAsset);
    }
}

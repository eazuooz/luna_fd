//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Localization;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class LocalizationHelperBase : MonoBehaviour, IDataProviderHelper<ILocalizationManager>, ILocalizationHelper
    {
        public abstract Language SystemLanguage
        {
            get;
        }

        public abstract bool ReadData(ILocalizationManager localizationManager, string dictionaryAssetName, object dictionaryAsset, object userData);

        public abstract bool ReadData(ILocalizationManager localizationManager, string dictionaryAssetName, byte[] dictionaryBytes, int startIndex, int length, object userData);

        public abstract bool ParseData(ILocalizationManager localizationManager, string dictionaryString, object userData);

        public abstract bool ParseData(ILocalizationManager localizationManager, byte[] dictionaryBytes, int startIndex, int length, object userData);

        public abstract void ReleaseDataAsset(ILocalizationManager localizationManager, object dictionaryAsset);
    }
}

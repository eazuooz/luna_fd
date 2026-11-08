//------------------------------------------------------------
// Game Framework (YamYamCoding Edition) - MIT License
// Copyright © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    // 원본의 Resource Tools(Resource Editor / Builder / Analyzer / Pack Builder / Sync Tools)는
    // 전부 AssetBundle 파이프라인용이다. 이 에디션은 리소스를 Addressables 로 읽으므로
    // 그 도구들이 만드는 산출물을 런타임이 읽지 못한다. 그래서 옮기지 않고,
    // 같은 자리에 Addressables 를 다루는 메뉴를 둔다.
    public static class AddressablesTools
    {
        private const string GroupsWindowMenuPath = "Window/Asset Management/Addressables/Groups";

        [MenuItem("Game Framework/Addressables/Open Groups Window", false, 40)]
        public static void OpenGroupsWindow()
        {
            if (!EditorApplication.ExecuteMenuItem(GroupsWindowMenuPath))
            {
                Debug.LogWarning("Can not open Addressables Groups window. Check that the Addressables package is installed.");
            }
        }

        // 프로젝트를 새로 받은 사람이 가장 먼저 해야 하는 설정이다. 이 값은 Library 아래에
        // 저장돼 버전 관리로 공유되지 않는다.
        [MenuItem("Game Framework/Addressables/Play Mode - Use Asset Database", false, 41)]
        public static void UseAssetDatabasePlayMode()
        {
            AddressableAssetSettings settings = GetSettings();
            if (settings == null)
            {
                Debug.LogError("Addressables settings are not created.");
                return;
            }

            int fastModeIndex = FindFastPlayModeIndex(settings);
            if (fastModeIndex < 0)
            {
                Debug.LogError("Can not find 'Use Asset Database' play mode script in Addressables settings.");
                return;
            }

            settings.ActivePlayModeDataBuilderIndex = fastModeIndex;
            Debug.Log("Addressables Play Mode Script is set to 'Use Asset Database'.");
        }

        [MenuItem("Game Framework/Addressables/Play Mode - Use Asset Database", true)]
        private static bool UseAssetDatabasePlayModeValidate()
        {
            AddressableAssetSettings settings = GetSettings();

            return settings != null && !IsFastPlayMode(settings);
        }

        [MenuItem("Game Framework/Addressables/Build Content", false, 60)]
        public static void BuildContent()
        {
            if (GetSettings() == null)
            {
                Debug.LogError("Addressables settings are not created.");
                return;
            }

            AddressablesPlayerBuildResult result = null;
            AddressableAssetSettings.BuildPlayerContent(out result);

            if (result != null && !string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError("Build Addressables content failure: " + result.Error);
                return;
            }

            Debug.Log("Build Addressables content complete.");
        }

        [MenuItem("Game Framework/Addressables/Clean Build", false, 61)]
        public static void CleanBuild()
        {
            if (GetSettings() == null)
            {
                Debug.LogError("Addressables settings are not created.");
                return;
            }

            AddressableAssetSettings.CleanPlayerContent();
            Debug.Log("Clean Addressables build complete.");
        }

        // 원본 Resource Analyzer 자리. 프레임워크는 주소 문자열로 에셋을 찾으므로,
        // 가장 흔한 실수 두 가지(같은 주소가 둘 / 주소를 줄이지 않고 경로 그대로 둠)를 찾아 준다.
        [MenuItem("Game Framework/Addressables/Validate Addresses", false, 80)]
        public static void ValidateAddresses()
        {
            AddressableAssetSettings settings = GetSettings();
            if (settings == null)
            {
                Debug.LogError("Addressables settings are not created.");
                return;
            }

            Dictionary<string, string> assetPathByAddress = new Dictionary<string, string>();
            StringBuilder duplicated = new StringBuilder();
            StringBuilder pathLike = new StringBuilder();
            int entryCount = 0;
            int duplicatedCount = 0;
            int pathLikeCount = 0;

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry == null || entry.IsFolder)
                    {
                        continue;
                    }

                    entryCount++;

                    string existingAssetPath = null;
                    if (assetPathByAddress.TryGetValue(entry.address, out existingAssetPath))
                    {
                        duplicatedCount++;
                        duplicated.AppendFormat("\n  '{0}': {1}  <->  {2}", entry.address, existingAssetPath, entry.AssetPath);
                    }
                    else
                    {
                        assetPathByAddress.Add(entry.address, entry.AssetPath);
                    }

                    if (entry.address == entry.AssetPath)
                    {
                        pathLikeCount++;
                        pathLike.AppendFormat("\n  [{0}] {1}", group.Name, entry.AssetPath);
                    }
                }
            }

            if (duplicatedCount > 0)
            {
                Debug.LogError("Addressables: " + duplicatedCount + " duplicated address(es). Loading by these addresses is ambiguous." + duplicated);
            }

            if (pathLikeCount > 0)
            {
                Debug.LogWarning("Addressables: " + pathLikeCount + " entry(ies) still use the asset path as address. If game code loads them by a short key, rename the address." + pathLike);
            }

            Debug.Log("Addressables: validated " + entryCount + " entry(ies) in " + settings.groups.Count + " group(s). Duplicated: " + duplicatedCount + ", path as address: " + pathLikeCount + ".");
        }

        internal static AddressableAssetSettings GetSettings()
        {
            return AddressableAssetSettingsDefaultObject.Settings;
        }

        internal static ScriptableObject GetActivePlayModeDataBuilder(AddressableAssetSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            int index = settings.ActivePlayModeDataBuilderIndex;
            if (index < 0 || index >= settings.DataBuilders.Count)
            {
                return null;
            }

            return settings.DataBuilders[index];
        }

        internal static bool IsFastPlayMode(AddressableAssetSettings settings)
        {
            return GetActivePlayModeDataBuilder(settings) is BuildScriptFastMode;
        }

        private static int FindFastPlayModeIndex(AddressableAssetSettings settings)
        {
            for (int i = 0; i < settings.DataBuilders.Count; i++)
            {
                if (settings.DataBuilders[i] is BuildScriptFastMode)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

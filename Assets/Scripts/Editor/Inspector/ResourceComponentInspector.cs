//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    // 원본 ResourceComponentInspector 는 AssetBundle 파이프라인(리소스 모드, 읽기/쓰기 경로,
    // 업데이트 URI, 로드 에이전트 등)을 다뤘다. 이 에디션의 ResourceComponent 는 Addressables
    // 어댑터라 그 항목들이 없다. 남아 있는 설정 두 개와 Addressables 상태만 보여 준다.
    [CustomEditor(typeof(ResourceComponent))]
    internal sealed class ResourceComponentInspector : GameFrameworkInspector
    {
        private const float MaxUnloadUnusedAssetsIntervalLimit = 3600f;

        private SerializedProperty mMinUnloadUnusedAssetsInterval = null;
        private SerializedProperty mMaxUnloadUnusedAssetsInterval = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceComponent t = (ResourceComponent)target;

            EditorGUILayout.HelpBox("Assets are loaded through Addressables. The asset name passed to ShowEntity / OpenUIForm / PlaySound is the Addressables address.", MessageType.Info);

            DrawAddressablesState();

            float minUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Min Unload Unused Assets Interval", mMinUnloadUnusedAssetsInterval.floatValue, 0f, MaxUnloadUnusedAssetsIntervalLimit);
            if (minUnloadUnusedAssetsInterval != mMinUnloadUnusedAssetsInterval.floatValue)
            {
                mMinUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
            }

            float maxUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Max Unload Unused Assets Interval", mMaxUnloadUnusedAssetsInterval.floatValue, 0f, MaxUnloadUnusedAssetsIntervalLimit);
            if (maxUnloadUnusedAssetsInterval != mMaxUnloadUnusedAssetsInterval.floatValue)
            {
                mMaxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
            }

            if (mMaxUnloadUnusedAssetsInterval.floatValue < mMinUnloadUnusedAssetsInterval.floatValue)
            {
                EditorGUILayout.HelpBox("Max interval is smaller than min interval. Unused assets will be unloaded every max interval.", MessageType.Warning);
            }

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Loaded Asset Count", t.ResourceManager != null ? t.ResourceManager.AssetCount.ToString() : "<None>");
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        private void OnEnable()
        {
            mMinUnloadUnusedAssetsInterval = serializedObject.FindProperty("mMinUnloadUnusedAssetsInterval");
            mMaxUnloadUnusedAssetsInterval = serializedObject.FindProperty("mMaxUnloadUnusedAssetsInterval");
        }

        private static void DrawAddressablesState()
        {
            AddressableAssetSettings settings = AddressablesTools.GetSettings();
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Addressables settings are not created. Open 'Window > Asset Management > Addressables > Groups' and create them.", MessageType.Error);
            }
            else
            {
                ScriptableObject playModeDataBuilder = AddressablesTools.GetActivePlayModeDataBuilder(settings);
                EditorGUILayout.LabelField("Play Mode Script", playModeDataBuilder != null ? playModeDataBuilder.name : "<None>");

                if (!AddressablesTools.IsFastPlayMode(settings))
                {
                    EditorGUILayout.HelpBox("Play Mode Script is not 'Use Asset Database'. Assets load from built content, so build Addressables content first or nothing will appear.", MessageType.Warning);
                }
            }

            if (GUILayout.Button("Open Addressables Groups"))
            {
                AddressablesTools.OpenGroupsWindow();
            }
        }
    }
}

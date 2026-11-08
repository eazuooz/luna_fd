//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(LocalizationComponent))]
    internal sealed class LocalizationComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mEnableLoadDictionaryUpdateEvent = null;
        private SerializedProperty mEnableLoadDictionaryDependencyAssetEvent = null;
        private SerializedProperty mCachedBytesSize = null;

        private HelperInfo<LocalizationHelperBase> mLocalizationHelperInfo = new HelperInfo<LocalizationHelperBase>("Localization");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            LocalizationComponent t = (LocalizationComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mEnableLoadDictionaryUpdateEvent);
                PropertyField(mEnableLoadDictionaryDependencyAssetEvent);
                mLocalizationHelperInfo.Draw();
                PropertyField(mCachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Language", t.Language.ToString());
                EditorGUILayout.LabelField("System Language", t.SystemLanguage.ToString());
                EditorGUILayout.LabelField("Dictionary Count", t.DictionaryCount.ToString());
                EditorGUILayout.LabelField("Cached Bytes Size", t.CachedBytesSize.ToString());
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            mEnableLoadDictionaryUpdateEvent = serializedObject.FindProperty("mEnableLoadDictionaryUpdateEvent");
            mEnableLoadDictionaryDependencyAssetEvent = serializedObject.FindProperty("mEnableLoadDictionaryDependencyAssetEvent");
            mCachedBytesSize = serializedObject.FindProperty("mCachedBytesSize");

            mLocalizationHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mLocalizationHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

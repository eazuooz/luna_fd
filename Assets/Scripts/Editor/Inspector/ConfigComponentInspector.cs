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
    [CustomEditor(typeof(ConfigComponent))]
    internal sealed class ConfigComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mEnableLoadConfigUpdateEvent = null;
        private SerializedProperty mEnableLoadConfigDependencyAssetEvent = null;
        private SerializedProperty mCachedBytesSize = null;

        private HelperInfo<ConfigHelperBase> mConfigHelperInfo = new HelperInfo<ConfigHelperBase>("Config");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ConfigComponent t = (ConfigComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mEnableLoadConfigUpdateEvent);
                PropertyField(mEnableLoadConfigDependencyAssetEvent);
                mConfigHelperInfo.Draw();
                PropertyField(mCachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Config Count", t.Count.ToString());
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
            mEnableLoadConfigUpdateEvent = serializedObject.FindProperty("mEnableLoadConfigUpdateEvent");
            mEnableLoadConfigDependencyAssetEvent = serializedObject.FindProperty("mEnableLoadConfigDependencyAssetEvent");
            mCachedBytesSize = serializedObject.FindProperty("mCachedBytesSize");

            mConfigHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mConfigHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

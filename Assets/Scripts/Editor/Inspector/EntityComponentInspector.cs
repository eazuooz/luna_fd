//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using GameFramework.Entity;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(EntityComponent))]
    internal sealed class EntityComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mEnableShowEntityUpdateEvent = null;
        private SerializedProperty mEnableShowEntityDependencyAssetEvent = null;
        private SerializedProperty mInstanceRoot = null;
        private SerializedProperty mEntityGroups = null;

        private HelperInfo<EntityHelperBase> mEntityHelperInfo = new HelperInfo<EntityHelperBase>("Entity");
        private HelperInfo<EntityGroupHelperBase> mEntityGroupHelperInfo = new HelperInfo<EntityGroupHelperBase>("EntityGroup");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EntityComponent t = (EntityComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mEnableShowEntityUpdateEvent);
                PropertyField(mEnableShowEntityDependencyAssetEvent);
                PropertyField(mInstanceRoot);
                mEntityHelperInfo.Draw();
                mEntityGroupHelperInfo.Draw();
                PropertyField(mEntityGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Entity Group Count", t.EntityGroupCount.ToString());
                EditorGUILayout.LabelField("Entity Count (Total)", t.EntityCount.ToString());
                IEntityGroup[] entityGroups = t.GetAllEntityGroups();
                foreach (IEntityGroup entityGroup in entityGroups)
                {
                    EditorGUILayout.LabelField(Utility.Text.Format("Entity Count ({0})", entityGroup.Name), entityGroup.EntityCount.ToString());
                }
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
            mEnableShowEntityUpdateEvent = serializedObject.FindProperty("mEnableShowEntityUpdateEvent");
            mEnableShowEntityDependencyAssetEvent = serializedObject.FindProperty("mEnableShowEntityDependencyAssetEvent");
            mInstanceRoot = serializedObject.FindProperty("mInstanceRoot");
            mEntityGroups = serializedObject.FindProperty("mEntityGroups");

            mEntityHelperInfo.Init(serializedObject);
            mEntityGroupHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mEntityHelperInfo.Refresh();
            mEntityGroupHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

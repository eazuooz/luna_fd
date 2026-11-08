//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using GameFramework.DataTable;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(DataTableComponent))]
    internal sealed class DataTableComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mEnableLoadDataTableUpdateEvent = null;
        private SerializedProperty mEnableLoadDataTableDependencyAssetEvent = null;
        private SerializedProperty mCachedBytesSize = null;

        private HelperInfo<DataTableHelperBase> mDataTableHelperInfo = new HelperInfo<DataTableHelperBase>("DataTable");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DataTableComponent t = (DataTableComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mEnableLoadDataTableUpdateEvent);
                PropertyField(mEnableLoadDataTableDependencyAssetEvent);
                mDataTableHelperInfo.Draw();
                PropertyField(mCachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Data Table Count", t.Count.ToString());
                EditorGUILayout.LabelField("Cached Bytes Size", t.CachedBytesSize.ToString());

                DataTableBase[] dataTables = t.GetAllDataTables();
                foreach (DataTableBase dataTable in dataTables)
                {
                    DrawDataTable(dataTable);
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
            mEnableLoadDataTableUpdateEvent = serializedObject.FindProperty("mEnableLoadDataTableUpdateEvent");
            mEnableLoadDataTableDependencyAssetEvent = serializedObject.FindProperty("mEnableLoadDataTableDependencyAssetEvent");
            mCachedBytesSize = serializedObject.FindProperty("mCachedBytesSize");

            mDataTableHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void DrawDataTable(DataTableBase dataTable)
        {
            EditorGUILayout.LabelField(dataTable.FullName, Utility.Text.Format("{0} Rows", dataTable.Count));
        }

        private void RefreshTypeNames()
        {
            mDataTableHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

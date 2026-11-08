//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.DataNode;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(DataNodeComponent))]
    internal sealed class DataNodeComponentInspector : GameFrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            DataNodeComponent t = (DataNodeComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                DrawDataNode(t.Root);
            }

            Repaint();
        }

        private void OnEnable()
        {
        }

        private void DrawDataNode(IDataNode dataNode)
        {
            EditorGUILayout.LabelField(dataNode.FullName, dataNode.ToDataString());
            IDataNode[] child = dataNode.GetAllChild();
            foreach (IDataNode c in child)
            {
                DrawDataNode(c);
            }
        }
    }
}

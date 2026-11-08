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
    [CustomEditor(typeof(EventComponent))]
    internal sealed class EventComponentInspector : GameFrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            EventComponent t = (EventComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Event Handler Count", t.EventHandlerCount.ToString());
                EditorGUILayout.LabelField("Event Count", t.EventCount.ToString());
            }

            Repaint();
        }

        private void OnEnable()
        {
        }
    }
}

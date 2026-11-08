//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(DebuggerComponent))]
    internal sealed class DebuggerComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mSkin = null;
        private SerializedProperty mActiveWindow = null;
        private SerializedProperty mShowFullWindow = null;
        private SerializedProperty mConsoleWindow = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DebuggerComponent t = (DebuggerComponent)target;

            PropertyField(mSkin);

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                bool activeWindow = EditorGUILayout.Toggle("Active Window", t.ActiveWindow);
                if (activeWindow != t.ActiveWindow)
                {
                    t.ActiveWindow = activeWindow;
                }
            }
            else
            {
                PropertyField(mActiveWindow);
            }

            PropertyField(mShowFullWindow);

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Reset Layout"))
                {
                    t.ResetLayout();
                }
            }

            PropertyField(mConsoleWindow, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            mSkin = serializedObject.FindProperty("mSkin");
            mActiveWindow = serializedObject.FindProperty("mActiveWindow");
            mShowFullWindow = serializedObject.FindProperty("mShowFullWindow");
            mConsoleWindow = serializedObject.FindProperty("mConsoleWindow");
        }
    }
}

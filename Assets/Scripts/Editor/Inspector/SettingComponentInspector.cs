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
    [CustomEditor(typeof(SettingComponent))]
    internal sealed class SettingComponentInspector : GameFrameworkInspector
    {
        private HelperInfo<SettingHelperBase> mSettingHelperInfo = new HelperInfo<SettingHelperBase>("Setting");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SettingComponent t = (SettingComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                mSettingHelperInfo.Draw();
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Setting Count", t.Count >= 0 ? t.Count.ToString() : "<Unknown>");
                if (t.Count > 0)
                {
                    string[] settingNames = t.GetAllSettingNames();
                    foreach (string settingName in settingNames)
                    {
                        EditorGUILayout.LabelField(settingName, t.GetString(settingName));
                    }
                }
            }

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Save Settings"))
                {
                    t.Save();
                }
                if (GUILayout.Button("Remove All Settings"))
                {
                    t.RemoveAllSettings();
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
            mSettingHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mSettingHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

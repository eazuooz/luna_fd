//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    public abstract class GameFrameworkInspector : UnityEditor.Editor
    {
        private bool mIsCompiling = false;

        public override void OnInspectorGUI()
        {
            if (mIsCompiling && !EditorApplication.isCompiling)
            {
                mIsCompiling = false;
                OnCompileComplete();
            }
            else if (!mIsCompiling && EditorApplication.isCompiling)
            {
                mIsCompiling = true;
                OnCompileStart();
            }
        }

        protected virtual void OnCompileStart()
        {
        }

        protected virtual void OnCompileComplete()
        {
        }

        // Unity 는 필드 이름으로 라벨을 만들 때 "m_" 접두사는 떼어 주지만 "m" 접두사는 떼지 않는다.
        // 이 에디션의 필드는 mEditorLanguage 꼴이라 그대로 그리면 "M Editor Language" 가 된다.
        // 프레임워크 인스펙터는 EditorGUILayout.PropertyField 대신 이 함수로 그린다.
        internal static void PropertyField(SerializedProperty property)
        {
            PropertyField(property, false);
        }

        internal static void PropertyField(SerializedProperty property, bool includeChildren)
        {
            GUIContent label = new GUIContent(GetDisplayName(property), property.tooltip);

            // 자식이 있는 필드는 자식 라벨도 고쳐야 하므로 직접 펼쳐 그린다.
            bool isComposite = property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren;
            if (!includeChildren || !isComposite)
            {
                EditorGUILayout.PropertyField(property, label, includeChildren);
                return;
            }

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            if (property.isArray)
            {
                DrawArrayElements(property);
            }
            else
            {
                DrawChildren(property);
            }

            EditorGUI.indentLevel--;
        }

        internal static string GetDisplayName(SerializedProperty property)
        {
            const string UnstrippedPrefix = "M ";

            string name = property.name;
            string displayName = property.displayName;
            bool hasMemberPrefix = name.Length > 1 && name[0] == 'm' && char.IsUpper(name[1]);
            if (!hasMemberPrefix)
            {
                return displayName;
            }

            if (displayName.StartsWith(UnstrippedPrefix))
            {
                return displayName.Substring(UnstrippedPrefix.Length);
            }

            // 약어로 시작하는 필드(mUIGroups)는 Unity 가 접두어를 약어에 붙여 "MUI Groups" 로 만든다.
            // 접두어를 떼고 다시 다듬는다.
            return ObjectNames.NicifyVariableName(name.Substring(1));
        }

        private static void DrawChildren(SerializedProperty property)
        {
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;
            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                PropertyField(child, true);
            }
        }

        private static void DrawArrayElements(SerializedProperty property)
        {
            const float RemoveButtonWidth = 22f;

            int size = EditorGUILayout.DelayedIntField("Size", property.arraySize);
            if (size >= 0 && size != property.arraySize)
            {
                property.arraySize = size;
            }

            for (int i = 0; i < property.arraySize; i++)
            {
                bool removed = false;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        PropertyField(property.GetArrayElementAtIndex(i), true);
                    }
                    EditorGUILayout.EndVertical();

                    removed = GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth));
                }
                EditorGUILayout.EndHorizontal();

                if (removed)
                {
                    property.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), "Add"))
            {
                property.arraySize++;
            }
        }

        protected bool IsPrefabInHierarchy(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

#if UNITY_2018_3_OR_NEWER
            return PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Regular;
#else
            return PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab;
#endif
        }
    }
}

//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    internal sealed class HelperInfo<T> where T : MonoBehaviour
    {
        private const string CustomOptionName = "<Custom>";

        private readonly string mName;

        private SerializedProperty mHelperTypeName;
        private SerializedProperty mCustomHelper;
        private string[] mHelperTypeNames;
        private int mHelperTypeNameIndex;

        public HelperInfo(string name)
        {
            mName = name;

            mHelperTypeName = null;
            mCustomHelper = null;
            mHelperTypeNames = null;
            mHelperTypeNameIndex = 0;
        }

        public void Init(SerializedObject serializedObject)
        {
            mHelperTypeName = serializedObject.FindProperty(Utility.Text.Format("m{0}HelperTypeName", mName));
            mCustomHelper = serializedObject.FindProperty(Utility.Text.Format("mCustom{0}Helper", mName));
        }

        public void Draw()
        {
            string displayName = FieldNameForDisplay(mName);
            int selectedIndex = EditorGUILayout.Popup(Utility.Text.Format("{0} Helper", displayName), mHelperTypeNameIndex, mHelperTypeNames);
            if (selectedIndex != mHelperTypeNameIndex)
            {
                mHelperTypeNameIndex = selectedIndex;
                mHelperTypeName.stringValue = selectedIndex <= 0 ? null : mHelperTypeNames[selectedIndex];
            }

            if (mHelperTypeNameIndex <= 0)
            {
                GameFrameworkInspector.PropertyField(mCustomHelper);
                if (mCustomHelper.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(Utility.Text.Format("You must set Custom {0} Helper.", displayName), MessageType.Error);
                }
            }
        }

        public void Refresh()
        {
            List<string> helperTypeNameList = new List<string>
            {
                CustomOptionName
            };

            helperTypeNameList.AddRange(Type.GetRuntimeTypeNames(typeof(T)));
            mHelperTypeNames = helperTypeNameList.ToArray();

            mHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(mHelperTypeName.stringValue))
            {
                mHelperTypeNameIndex = helperTypeNameList.IndexOf(mHelperTypeName.stringValue);
                if (mHelperTypeNameIndex <= 0)
                {
                    mHelperTypeNameIndex = 0;
                    mHelperTypeName.stringValue = null;
                }
            }
        }

        private string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }
    }
}

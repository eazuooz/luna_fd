//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(BaseComponent))]
    internal sealed class BaseComponentInspector : GameFrameworkInspector
    {
        private const string NoneOptionName = "<None>";
        private static readonly float[] GameSpeed = new float[] { 0f, 0.01f, 0.1f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f };
        private static readonly string[] GameSpeedForDisplay = new string[] { "0x", "0.01x", "0.1x", "0.25x", "0.5x", "1x", "1.5x", "2x", "4x", "8x" };

        private SerializedProperty mEditorResourceMode = null;
        private SerializedProperty mEditorLanguage = null;
        private SerializedProperty mTextHelperTypeName = null;
        private SerializedProperty mVersionHelperTypeName = null;
        private SerializedProperty mLogHelperTypeName = null;
        private SerializedProperty mCompressionHelperTypeName = null;
        private SerializedProperty mJsonHelperTypeName = null;
        private SerializedProperty mFrameRate = null;
        private SerializedProperty mGameSpeed = null;
        private SerializedProperty mRunInBackground = null;
        private SerializedProperty mNeverSleep = null;

        private string[] mTextHelperTypeNames = null;
        private int mTextHelperTypeNameIndex = 0;
        private string[] mVersionHelperTypeNames = null;
        private int mVersionHelperTypeNameIndex = 0;
        private string[] mLogHelperTypeNames = null;
        private int mLogHelperTypeNameIndex = 0;
        private string[] mCompressionHelperTypeNames = null;
        private int mCompressionHelperTypeNameIndex = 0;
        private string[] mJsonHelperTypeNames = null;
        private int mJsonHelperTypeNameIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            BaseComponent t = (BaseComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                mEditorResourceMode.boolValue = EditorGUILayout.BeginToggleGroup("Editor Resource Mode", mEditorResourceMode.boolValue);
                {
                    EditorGUILayout.HelpBox("Editor resource mode option is only for editor mode. Game Framework will use editor resource files, which you should validate first.", MessageType.Warning);
                    PropertyField(mEditorLanguage);
                    EditorGUILayout.HelpBox("Editor language option is only use for localization test in editor mode.", MessageType.Info);
                }
                EditorGUILayout.EndToggleGroup();

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Global Helpers", EditorStyles.boldLabel);

                    int textHelperSelectedIndex = EditorGUILayout.Popup("Text Helper", mTextHelperTypeNameIndex, mTextHelperTypeNames);
                    if (textHelperSelectedIndex != mTextHelperTypeNameIndex)
                    {
                        mTextHelperTypeNameIndex = textHelperSelectedIndex;
                        mTextHelperTypeName.stringValue = textHelperSelectedIndex <= 0 ? null : mTextHelperTypeNames[textHelperSelectedIndex];
                    }

                    int versionHelperSelectedIndex = EditorGUILayout.Popup("Version Helper", mVersionHelperTypeNameIndex, mVersionHelperTypeNames);
                    if (versionHelperSelectedIndex != mVersionHelperTypeNameIndex)
                    {
                        mVersionHelperTypeNameIndex = versionHelperSelectedIndex;
                        mVersionHelperTypeName.stringValue = versionHelperSelectedIndex <= 0 ? null : mVersionHelperTypeNames[versionHelperSelectedIndex];
                    }

                    int logHelperSelectedIndex = EditorGUILayout.Popup("Log Helper", mLogHelperTypeNameIndex, mLogHelperTypeNames);
                    if (logHelperSelectedIndex != mLogHelperTypeNameIndex)
                    {
                        mLogHelperTypeNameIndex = logHelperSelectedIndex;
                        mLogHelperTypeName.stringValue = logHelperSelectedIndex <= 0 ? null : mLogHelperTypeNames[logHelperSelectedIndex];
                    }

                    int compressionHelperSelectedIndex = EditorGUILayout.Popup("Compression Helper", mCompressionHelperTypeNameIndex, mCompressionHelperTypeNames);
                    if (compressionHelperSelectedIndex != mCompressionHelperTypeNameIndex)
                    {
                        mCompressionHelperTypeNameIndex = compressionHelperSelectedIndex;
                        mCompressionHelperTypeName.stringValue = compressionHelperSelectedIndex <= 0 ? null : mCompressionHelperTypeNames[compressionHelperSelectedIndex];
                    }

                    int jsonHelperSelectedIndex = EditorGUILayout.Popup("JSON Helper", mJsonHelperTypeNameIndex, mJsonHelperTypeNames);
                    if (jsonHelperSelectedIndex != mJsonHelperTypeNameIndex)
                    {
                        mJsonHelperTypeNameIndex = jsonHelperSelectedIndex;
                        mJsonHelperTypeName.stringValue = jsonHelperSelectedIndex <= 0 ? null : mJsonHelperTypeNames[jsonHelperSelectedIndex];
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            int frameRate = EditorGUILayout.IntSlider("Frame Rate", mFrameRate.intValue, 1, 120);
            if (frameRate != mFrameRate.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FrameRate = frameRate;
                }
                else
                {
                    mFrameRate.intValue = frameRate;
                }
            }

            EditorGUILayout.BeginVertical("box");
            {
                float gameSpeed = EditorGUILayout.Slider("Game Speed", mGameSpeed.floatValue, 0f, 8f);
                int selectedGameSpeed = GUILayout.SelectionGrid(GetSelectedGameSpeed(gameSpeed), GameSpeedForDisplay, 5);
                if (selectedGameSpeed >= 0)
                {
                    gameSpeed = GetGameSpeed(selectedGameSpeed);
                }

                if (gameSpeed != mGameSpeed.floatValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.GameSpeed = gameSpeed;
                    }
                    else
                    {
                        mGameSpeed.floatValue = gameSpeed;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            bool runInBackground = EditorGUILayout.Toggle("Run in Background", mRunInBackground.boolValue);
            if (runInBackground != mRunInBackground.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.RunInBackground = runInBackground;
                }
                else
                {
                    mRunInBackground.boolValue = runInBackground;
                }
            }

            bool neverSleep = EditorGUILayout.Toggle("Never Sleep", mNeverSleep.boolValue);
            if (neverSleep != mNeverSleep.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.NeverSleep = neverSleep;
                }
                else
                {
                    mNeverSleep.boolValue = neverSleep;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            mEditorResourceMode = serializedObject.FindProperty("mEditorResourceMode");
            mEditorLanguage = serializedObject.FindProperty("mEditorLanguage");
            mTextHelperTypeName = serializedObject.FindProperty("mTextHelperTypeName");
            mVersionHelperTypeName = serializedObject.FindProperty("mVersionHelperTypeName");
            mLogHelperTypeName = serializedObject.FindProperty("mLogHelperTypeName");
            mCompressionHelperTypeName = serializedObject.FindProperty("mCompressionHelperTypeName");
            mJsonHelperTypeName = serializedObject.FindProperty("mJsonHelperTypeName");
            mFrameRate = serializedObject.FindProperty("mFrameRate");
            mGameSpeed = serializedObject.FindProperty("mGameSpeed");
            mRunInBackground = serializedObject.FindProperty("mRunInBackground");
            mNeverSleep = serializedObject.FindProperty("mNeverSleep");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            List<string> textHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            textHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Utility.Text.ITextHelper)));
            mTextHelperTypeNames = textHelperTypeNames.ToArray();
            mTextHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(mTextHelperTypeName.stringValue))
            {
                mTextHelperTypeNameIndex = textHelperTypeNames.IndexOf(mTextHelperTypeName.stringValue);
                if (mTextHelperTypeNameIndex <= 0)
                {
                    mTextHelperTypeNameIndex = 0;
                    mTextHelperTypeName.stringValue = null;
                }
            }

            List<string> versionHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            versionHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Version.IVersionHelper)));
            mVersionHelperTypeNames = versionHelperTypeNames.ToArray();
            mVersionHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(mVersionHelperTypeName.stringValue))
            {
                mVersionHelperTypeNameIndex = versionHelperTypeNames.IndexOf(mVersionHelperTypeName.stringValue);
                if (mVersionHelperTypeNameIndex <= 0)
                {
                    mVersionHelperTypeNameIndex = 0;
                    mVersionHelperTypeName.stringValue = null;
                }
            }

            List<string> logHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            logHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(GameFrameworkLog.ILogHelper)));
            mLogHelperTypeNames = logHelperTypeNames.ToArray();
            mLogHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(mLogHelperTypeName.stringValue))
            {
                mLogHelperTypeNameIndex = logHelperTypeNames.IndexOf(mLogHelperTypeName.stringValue);
                if (mLogHelperTypeNameIndex <= 0)
                {
                    mLogHelperTypeNameIndex = 0;
                    mLogHelperTypeName.stringValue = null;
                }
            }

            List<string> compressionHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            compressionHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Utility.Compression.ICompressionHelper)));
            mCompressionHelperTypeNames = compressionHelperTypeNames.ToArray();
            mCompressionHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(mCompressionHelperTypeName.stringValue))
            {
                mCompressionHelperTypeNameIndex = compressionHelperTypeNames.IndexOf(mCompressionHelperTypeName.stringValue);
                if (mCompressionHelperTypeNameIndex <= 0)
                {
                    mCompressionHelperTypeNameIndex = 0;
                    mCompressionHelperTypeName.stringValue = null;
                }
            }

            List<string> jsonHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            jsonHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Utility.Json.IJsonHelper)));
            mJsonHelperTypeNames = jsonHelperTypeNames.ToArray();
            mJsonHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(mJsonHelperTypeName.stringValue))
            {
                mJsonHelperTypeNameIndex = jsonHelperTypeNames.IndexOf(mJsonHelperTypeName.stringValue);
                if (mJsonHelperTypeNameIndex <= 0)
                {
                    mJsonHelperTypeNameIndex = 0;
                    mJsonHelperTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private float GetGameSpeed(int selectedGameSpeed)
        {
            if (selectedGameSpeed < 0)
            {
                return GameSpeed[0];
            }

            if (selectedGameSpeed >= GameSpeed.Length)
            {
                return GameSpeed[GameSpeed.Length - 1];
            }

            return GameSpeed[selectedGameSpeed];
        }

        private int GetSelectedGameSpeed(float gameSpeed)
        {
            for (int i = 0; i < GameSpeed.Length; i++)
            {
                if (gameSpeed == GameSpeed[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

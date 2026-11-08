//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Procedure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(ProcedureComponent))]
    internal sealed class ProcedureComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mAvailableProcedureTypeNames = null;
        private SerializedProperty mEntranceProcedureTypeName = null;

        private string[] mProcedureTypeNames = null;
        private List<string> mCurrentAvailableProcedureTypeNames = null;
        private int mEntranceProcedureIndex = -1;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ProcedureComponent t = (ProcedureComponent)target;

            if (string.IsNullOrEmpty(mEntranceProcedureTypeName.stringValue))
            {
                EditorGUILayout.HelpBox("Entrance procedure is invalid.", MessageType.Error);
            }
            else if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Current Procedure", GetCurrentProcedureName(t));
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Label("Available Procedures", EditorStyles.boldLabel);
                if (mProcedureTypeNames.Length > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        foreach (string procedureTypeName in mProcedureTypeNames)
                        {
                            bool selected = mCurrentAvailableProcedureTypeNames.Contains(procedureTypeName);
                            if (selected != EditorGUILayout.ToggleLeft(procedureTypeName, selected))
                            {
                                if (!selected)
                                {
                                    mCurrentAvailableProcedureTypeNames.Add(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                                else if (procedureTypeName != mEntranceProcedureTypeName.stringValue)
                                {
                                    mCurrentAvailableProcedureTypeNames.Remove(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no available procedure.", MessageType.Warning);
                }

                if (mCurrentAvailableProcedureTypeNames.Count > 0)
                {
                    EditorGUILayout.Separator();

                    int selectedIndex = EditorGUILayout.Popup("Entrance Procedure", mEntranceProcedureIndex, mCurrentAvailableProcedureTypeNames.ToArray());
                    if (selectedIndex != mEntranceProcedureIndex)
                    {
                        mEntranceProcedureIndex = selectedIndex;
                        mEntranceProcedureTypeName.stringValue = mCurrentAvailableProcedureTypeNames[selectedIndex];
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Select available procedures first.", MessageType.Info);
                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        // 진입 절차는 ProcedureComponent.Start 가 끝나야 시작된다. 재생 직후 그 전에 인스펙터가 그려지면
        // 코어의 CurrentProcedure 가 "You must initialize procedure first." 예외를 던진다.
        // 아직 시작 전이면 절차가 없는 것으로 표시한다.
        private static string GetCurrentProcedureName(ProcedureComponent procedureComponent)
        {
            try
            {
                ProcedureBase currentProcedure = procedureComponent.CurrentProcedure;

                return currentProcedure == null ? "None" : currentProcedure.GetType().ToString();
            }
            catch (GameFramework.GameFrameworkException)
            {
                return "None";
            }
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            mAvailableProcedureTypeNames = serializedObject.FindProperty("mAvailableProcedureTypeNames");
            mEntranceProcedureTypeName = serializedObject.FindProperty("mEntranceProcedureTypeName");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mProcedureTypeNames = Type.GetRuntimeTypeNames(typeof(ProcedureBase));
            ReadAvailableProcedureTypeNames();
            int oldCount = mCurrentAvailableProcedureTypeNames.Count;
            mCurrentAvailableProcedureTypeNames = mCurrentAvailableProcedureTypeNames.Where(x => mProcedureTypeNames.Contains(x)).ToList();
            if (mCurrentAvailableProcedureTypeNames.Count != oldCount)
            {
                WriteAvailableProcedureTypeNames();
            }
            else if (!string.IsNullOrEmpty(mEntranceProcedureTypeName.stringValue))
            {
                mEntranceProcedureIndex = mCurrentAvailableProcedureTypeNames.IndexOf(mEntranceProcedureTypeName.stringValue);
                if (mEntranceProcedureIndex < 0)
                {
                    mEntranceProcedureTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ReadAvailableProcedureTypeNames()
        {
            mCurrentAvailableProcedureTypeNames = new List<string>();
            int count = mAvailableProcedureTypeNames.arraySize;
            for (int i = 0; i < count; i++)
            {
                mCurrentAvailableProcedureTypeNames.Add(mAvailableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue);
            }
        }

        private void WriteAvailableProcedureTypeNames()
        {
            mAvailableProcedureTypeNames.ClearArray();
            if (mCurrentAvailableProcedureTypeNames == null)
            {
                return;
            }

            mCurrentAvailableProcedureTypeNames.Sort();
            int count = mCurrentAvailableProcedureTypeNames.Count;
            for (int i = 0; i < count; i++)
            {
                mAvailableProcedureTypeNames.InsertArrayElementAtIndex(i);
                mAvailableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue = mCurrentAvailableProcedureTypeNames[i];
            }

            if (!string.IsNullOrEmpty(mEntranceProcedureTypeName.stringValue))
            {
                mEntranceProcedureIndex = mCurrentAvailableProcedureTypeNames.IndexOf(mEntranceProcedureTypeName.stringValue);
                if (mEntranceProcedureIndex < 0)
                {
                    mEntranceProcedureTypeName.stringValue = null;
                }
            }
        }
    }
}

//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(WebRequestComponent))]
    internal sealed class WebRequestComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mInstanceRoot = null;
        private SerializedProperty mWebRequestAgentHelperCount = null;
        private SerializedProperty mTimeout = null;

        private HelperInfo<WebRequestAgentHelperBase> mWebRequestAgentHelperInfo = new HelperInfo<WebRequestAgentHelperBase>("WebRequestAgent");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            WebRequestComponent t = (WebRequestComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mInstanceRoot);

                mWebRequestAgentHelperInfo.Draw();

                mWebRequestAgentHelperCount.intValue = EditorGUILayout.IntSlider("Web Request Agent Helper Count", mWebRequestAgentHelperCount.intValue, 1, 16);
            }
            EditorGUI.EndDisabledGroup();

            float timeout = EditorGUILayout.Slider("Timeout", mTimeout.floatValue, 0f, 120f);
            if (timeout != mTimeout.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.Timeout = timeout;
                }
                else
                {
                    mTimeout.floatValue = timeout;
                }
            }

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Total Agent Count", t.TotalAgentCount.ToString());
                EditorGUILayout.LabelField("Free Agent Count", t.FreeAgentCount.ToString());
                EditorGUILayout.LabelField("Working Agent Count", t.WorkingAgentCount.ToString());
                EditorGUILayout.LabelField("Waiting Agent Count", t.WaitingTaskCount.ToString());
                EditorGUILayout.BeginVertical("box");
                {
                    TaskInfo[] webRequestInfos = t.GetAllWebRequestInfos();
                    if (webRequestInfos.Length > 0)
                    {
                        foreach (TaskInfo webRequestInfo in webRequestInfos)
                        {
                            DrawWebRequestInfo(webRequestInfo);
                        }

                        if (GUILayout.Button("Export CSV Data"))
                        {
                            string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, "WebRequest Task Data.csv", string.Empty);
                            if (!string.IsNullOrEmpty(exportFileName))
                            {
                                try
                                {
                                    int index = 0;
                                    string[] data = new string[webRequestInfos.Length + 1];
                                    data[index++] = "WebRequest Uri,Serial Id,Tag,Priority,Status";
                                    foreach (TaskInfo webRequestInfo in webRequestInfos)
                                    {
                                        data[index++] = Utility.Text.Format("{0},{1},{2},{3},{4}", webRequestInfo.Description, webRequestInfo.SerialId, webRequestInfo.Tag ?? string.Empty, webRequestInfo.Priority, webRequestInfo.Status);
                                    }

                                    File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                    Debug.Log(Utility.Text.Format("Export web request task CSV data to '{0}' success.", exportFileName));
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogError(Utility.Text.Format("Export web request task CSV data to '{0}' failure, exception is '{1}'.", exportFileName, exception));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("WebRequset Task is Empty ...");
                    }
                }
                EditorGUILayout.EndVertical();
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
            mInstanceRoot = serializedObject.FindProperty("mInstanceRoot");
            mWebRequestAgentHelperCount = serializedObject.FindProperty("mWebRequestAgentHelperCount");
            mTimeout = serializedObject.FindProperty("mTimeout");

            mWebRequestAgentHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void DrawWebRequestInfo(TaskInfo webRequestInfo)
        {
            EditorGUILayout.LabelField(webRequestInfo.Description, Utility.Text.Format("[SerialId]{0} [Tag]{1} [Priority]{2} [Status]{3}", webRequestInfo.SerialId, webRequestInfo.Tag ?? "<None>", webRequestInfo.Priority, webRequestInfo.Status));
        }

        private void RefreshTypeNames()
        {
            mWebRequestAgentHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

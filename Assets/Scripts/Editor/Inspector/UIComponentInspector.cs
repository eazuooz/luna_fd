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
    [CustomEditor(typeof(UIComponent))]
    internal sealed class UIComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mEnableOpenUIFormSuccessEvent = null;
        private SerializedProperty mEnableOpenUIFormFailureEvent = null;
        private SerializedProperty mEnableOpenUIFormUpdateEvent = null;
        private SerializedProperty mEnableOpenUIFormDependencyAssetEvent = null;
        private SerializedProperty mEnableCloseUIFormCompleteEvent = null;
        private SerializedProperty mInstanceAutoReleaseInterval = null;
        private SerializedProperty mInstanceCapacity = null;
        private SerializedProperty mInstanceExpireTime = null;
        private SerializedProperty mInstancePriority = null;
        private SerializedProperty mInstanceRoot = null;
        private SerializedProperty mUIGroups = null;

        private HelperInfo<UIFormHelperBase> mUIFormHelperInfo = new HelperInfo<UIFormHelperBase>("UIForm");
        private HelperInfo<UIGroupHelperBase> mUIGroupHelperInfo = new HelperInfo<UIGroupHelperBase>("UIGroup");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            UIComponent t = (UIComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mEnableOpenUIFormSuccessEvent);
                PropertyField(mEnableOpenUIFormFailureEvent);
                PropertyField(mEnableOpenUIFormUpdateEvent);
                PropertyField(mEnableOpenUIFormDependencyAssetEvent);
                PropertyField(mEnableCloseUIFormCompleteEvent);
            }
            EditorGUI.EndDisabledGroup();

            float instanceAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Instance Auto Release Interval", mInstanceAutoReleaseInterval.floatValue);
            if (instanceAutoReleaseInterval != mInstanceAutoReleaseInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
                }
                else
                {
                    mInstanceAutoReleaseInterval.floatValue = instanceAutoReleaseInterval;
                }
            }

            int instanceCapacity = EditorGUILayout.DelayedIntField("Instance Capacity", mInstanceCapacity.intValue);
            if (instanceCapacity != mInstanceCapacity.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceCapacity = instanceCapacity;
                }
                else
                {
                    mInstanceCapacity.intValue = instanceCapacity;
                }
            }

            float instanceExpireTime = EditorGUILayout.DelayedFloatField("Instance Expire Time", mInstanceExpireTime.floatValue);
            if (instanceExpireTime != mInstanceExpireTime.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceExpireTime = instanceExpireTime;
                }
                else
                {
                    mInstanceExpireTime.floatValue = instanceExpireTime;
                }
            }

            int instancePriority = EditorGUILayout.DelayedIntField("Instance Priority", mInstancePriority.intValue);
            if (instancePriority != mInstancePriority.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstancePriority = instancePriority;
                }
                else
                {
                    mInstancePriority.intValue = instancePriority;
                }
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mInstanceRoot);
                mUIFormHelperInfo.Draw();
                mUIGroupHelperInfo.Draw();
                PropertyField(mUIGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("UI Group Count", t.UIGroupCount.ToString());
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
            mEnableOpenUIFormSuccessEvent = serializedObject.FindProperty("mEnableOpenUIFormSuccessEvent");
            mEnableOpenUIFormFailureEvent = serializedObject.FindProperty("mEnableOpenUIFormFailureEvent");
            mEnableOpenUIFormUpdateEvent = serializedObject.FindProperty("mEnableOpenUIFormUpdateEvent");
            mEnableOpenUIFormDependencyAssetEvent = serializedObject.FindProperty("mEnableOpenUIFormDependencyAssetEvent");
            mEnableCloseUIFormCompleteEvent = serializedObject.FindProperty("mEnableCloseUIFormCompleteEvent");
            mInstanceAutoReleaseInterval = serializedObject.FindProperty("mInstanceAutoReleaseInterval");
            mInstanceCapacity = serializedObject.FindProperty("mInstanceCapacity");
            mInstanceExpireTime = serializedObject.FindProperty("mInstanceExpireTime");
            mInstancePriority = serializedObject.FindProperty("mInstancePriority");
            mInstanceRoot = serializedObject.FindProperty("mInstanceRoot");
            mUIGroups = serializedObject.FindProperty("mUIGroups");

            mUIFormHelperInfo.Init(serializedObject);
            mUIGroupHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mUIFormHelperInfo.Refresh();
            mUIGroupHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

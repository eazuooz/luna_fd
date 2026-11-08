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
    [CustomEditor(typeof(SoundComponent))]
    internal sealed class SoundComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty mEnablePlaySoundUpdateEvent = null;
        private SerializedProperty mEnablePlaySoundDependencyAssetEvent = null;
        private SerializedProperty mInstanceRoot = null;
        private SerializedProperty mAudioMixer = null;
        private SerializedProperty mSoundGroups = null;

        private HelperInfo<SoundHelperBase> mSoundHelperInfo = new HelperInfo<SoundHelperBase>("Sound");
        private HelperInfo<SoundGroupHelperBase> mSoundGroupHelperInfo = new HelperInfo<SoundGroupHelperBase>("SoundGroup");
        private HelperInfo<SoundAgentHelperBase> mSoundAgentHelperInfo = new HelperInfo<SoundAgentHelperBase>("SoundAgent");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SoundComponent t = (SoundComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                PropertyField(mEnablePlaySoundUpdateEvent);
                PropertyField(mEnablePlaySoundDependencyAssetEvent);
                PropertyField(mInstanceRoot);
                PropertyField(mAudioMixer);
                mSoundHelperInfo.Draw();
                mSoundGroupHelperInfo.Draw();
                mSoundAgentHelperInfo.Draw();
                PropertyField(mSoundGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Sound Group Count", t.SoundGroupCount.ToString());
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
            mEnablePlaySoundUpdateEvent = serializedObject.FindProperty("mEnablePlaySoundUpdateEvent");
            mEnablePlaySoundDependencyAssetEvent = serializedObject.FindProperty("mEnablePlaySoundDependencyAssetEvent");
            mInstanceRoot = serializedObject.FindProperty("mInstanceRoot");
            mAudioMixer = serializedObject.FindProperty("mAudioMixer");
            mSoundGroups = serializedObject.FindProperty("mSoundGroups");

            mSoundHelperInfo.Init(serializedObject);
            mSoundGroupHelperInfo.Init(serializedObject);
            mSoundAgentHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            mSoundHelperInfo.Refresh();
            mSoundGroupHelperInfo.Refresh();
            mSoundAgentHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

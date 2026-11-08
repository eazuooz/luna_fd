//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Resource;
#if UNITY_5_3
using GameFramework.Scene;
#endif
using GameFramework.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Sound")]
    public sealed partial class SoundComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private ISoundManager mSoundManager = null;
        private EventComponent mEventComponent = null;
        private AudioListener mAudioListener = null;

        [SerializeField]
        private bool mEnablePlaySoundUpdateEvent = false;

        [SerializeField]
        private bool mEnablePlaySoundDependencyAssetEvent = false;

        [SerializeField]
        private Transform mInstanceRoot = null;

        [SerializeField]
        private AudioMixer mAudioMixer = null;

        [SerializeField]
        private string mSoundHelperTypeName = "UnityGameFramework.Runtime.DefaultSoundHelper";

        [SerializeField]
        private SoundHelperBase mCustomSoundHelper = null;

        [SerializeField]
        private string mSoundGroupHelperTypeName = "UnityGameFramework.Runtime.DefaultSoundGroupHelper";

        [SerializeField]
        private SoundGroupHelperBase mCustomSoundGroupHelper = null;

        [SerializeField]
        private string mSoundAgentHelperTypeName = "UnityGameFramework.Runtime.DefaultSoundAgentHelper";

        [SerializeField]
        private SoundAgentHelperBase mCustomSoundAgentHelper = null;

        [SerializeField]
        private SoundGroup[] mSoundGroups = null;

        public int SoundGroupCount
        {
            get
            {
                return mSoundManager.SoundGroupCount;
            }
        }

        public AudioMixer AudioMixer
        {
            get
            {
                return mAudioMixer;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mSoundManager = GameFrameworkEntry.GetModule<ISoundManager>();
            if (mSoundManager == null)
            {
                Log.Fatal("Sound manager is invalid.");
                return;
            }

            mSoundManager.PlaySoundSuccess += OnPlaySoundSuccess;
            mSoundManager.PlaySoundFailure += OnPlaySoundFailure;

            if (mEnablePlaySoundUpdateEvent)
            {
                mSoundManager.PlaySoundUpdate += OnPlaySoundUpdate;
            }

            if (mEnablePlaySoundDependencyAssetEvent)
            {
                mSoundManager.PlaySoundDependencyAsset += OnPlaySoundDependencyAsset;
            }

            mAudioListener = gameObject.GetOrAddComponent<AudioListener>();

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
#else
            ISceneManager sceneManager = GameFrameworkEntry.GetModule<ISceneManager>();
            if (sceneManager == null)
            {
                Log.Fatal("Scene manager is invalid.");
                return;
            }

            sceneManager.LoadSceneSuccess += OnLoadSceneSuccess;
            sceneManager.LoadSceneFailure += OnLoadSceneFailure;
            sceneManager.UnloadSceneSuccess += OnUnloadSceneSuccess;
            sceneManager.UnloadSceneFailure += OnUnloadSceneFailure;
#endif
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            mEventComponent = GameEntry.GetComponent<EventComponent>();
            if (mEventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            mSoundManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);

            SoundHelperBase soundHelper = Helper.CreateHelper(mSoundHelperTypeName, mCustomSoundHelper);
            if (soundHelper == null)
            {
                Log.Error("Can not create sound helper.");
                return;
            }

            soundHelper.name = "Sound Helper";
            Transform transform = soundHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mSoundManager.SetSoundHelper(soundHelper);

            if (mInstanceRoot == null)
            {
                mInstanceRoot = new GameObject("Sound Instances").transform;
                mInstanceRoot.SetParent(gameObject.transform);
                mInstanceRoot.localScale = Vector3.one;
            }

            for (int i = 0; i < mSoundGroups.Length; i++)
            {
                if (!AddSoundGroup(mSoundGroups[i].Name, mSoundGroups[i].AvoidBeingReplacedBySamePriority, mSoundGroups[i].Mute, mSoundGroups[i].Volume, mSoundGroups[i].AgentHelperCount))
                {
                    Log.Warning("Add sound group '{0}' failure.", mSoundGroups[i].Name);
                    continue;
                }
            }
        }

        private void OnDestroy()
        {
#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
#endif
        }

        public bool HasSoundGroup(string soundGroupName)
        {
            return mSoundManager.HasSoundGroup(soundGroupName);
        }

        public ISoundGroup GetSoundGroup(string soundGroupName)
        {
            return mSoundManager.GetSoundGroup(soundGroupName);
        }

        public ISoundGroup[] GetAllSoundGroups()
        {
            return mSoundManager.GetAllSoundGroups();
        }

        public void GetAllSoundGroups(List<ISoundGroup> results)
        {
            mSoundManager.GetAllSoundGroups(results);
        }

        public bool AddSoundGroup(string soundGroupName, int soundAgentHelperCount)
        {
            return AddSoundGroup(soundGroupName, false, false, 1f, soundAgentHelperCount);
        }

        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, int soundAgentHelperCount)
        {
            if (mSoundManager.HasSoundGroup(soundGroupName))
            {
                return false;
            }

            SoundGroupHelperBase soundGroupHelper = Helper.CreateHelper(mSoundGroupHelperTypeName, mCustomSoundGroupHelper, SoundGroupCount);
            if (soundGroupHelper == null)
            {
                Log.Error("Can not create sound group helper.");
                return false;
            }

            soundGroupHelper.name = Utility.Text.Format("Sound Group - {0}", soundGroupName);
            Transform transform = soundGroupHelper.transform;
            transform.SetParent(mInstanceRoot);
            transform.localScale = Vector3.one;

            if (mAudioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = mAudioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}", soundGroupName));
                if (audioMixerGroups.Length > 0)
                {
                    soundGroupHelper.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    soundGroupHelper.AudioMixerGroup = mAudioMixer.FindMatchingGroups("Master")[0];
                }
            }

            if (!mSoundManager.AddSoundGroup(soundGroupName, soundGroupAvoidBeingReplacedBySamePriority, soundGroupMute, soundGroupVolume, soundGroupHelper))
            {
                return false;
            }

            for (int i = 0; i < soundAgentHelperCount; i++)
            {
                if (!AddSoundAgentHelper(soundGroupName, soundGroupHelper, i))
                {
                    return false;
                }
            }

            return true;
        }

        public int[] GetAllLoadingSoundSerialIds()
        {
            return mSoundManager.GetAllLoadingSoundSerialIds();
        }

        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            mSoundManager.GetAllLoadingSoundSerialIds(results);
        }

        public bool IsLoadingSound(int serialId)
        {
            return mSoundManager.IsLoadingSound(serialId);
        }

        public int PlaySound(string soundAssetName, string soundGroupName)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, null, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, null, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, playSoundParams, null, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, Entity bindingEntity)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, bindingEntity, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, Vector3 worldPosition)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, worldPosition, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, null, userData);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, userData);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, Entity bindingEntity)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, bindingEntity, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, Entity bindingEntity, object userData)
        {
            return mSoundManager.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, PlaySoundInfo.Create(bindingEntity, Vector3.zero, userData));
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, Vector3 worldPosition)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, worldPosition, null);
        }

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, Vector3 worldPosition, object userData)
        {
            return mSoundManager.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, PlaySoundInfo.Create(null, worldPosition, userData));
        }

        public bool StopSound(int serialId)
        {
            return mSoundManager.StopSound(serialId);
        }

        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            return mSoundManager.StopSound(serialId, fadeOutSeconds);
        }

        public void StopAllLoadedSounds()
        {
            mSoundManager.StopAllLoadedSounds();
        }

        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            mSoundManager.StopAllLoadedSounds(fadeOutSeconds);
        }

        public void StopAllLoadingSounds()
        {
            mSoundManager.StopAllLoadingSounds();
        }

        public void PauseSound(int serialId)
        {
            mSoundManager.PauseSound(serialId);
        }

        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            mSoundManager.PauseSound(serialId, fadeOutSeconds);
        }

        public void ResumeSound(int serialId)
        {
            mSoundManager.ResumeSound(serialId);
        }

        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            mSoundManager.ResumeSound(serialId, fadeInSeconds);
        }

        private bool AddSoundAgentHelper(string soundGroupName, SoundGroupHelperBase soundGroupHelper, int index)
        {
            SoundAgentHelperBase soundAgentHelper = Helper.CreateHelper(mSoundAgentHelperTypeName, mCustomSoundAgentHelper, index);
            if (soundAgentHelper == null)
            {
                Log.Error("Can not create sound agent helper.");
                return false;
            }

            soundAgentHelper.name = Utility.Text.Format("Sound Agent Helper - {0} - {1}", soundGroupName, index);
            Transform transform = soundAgentHelper.transform;
            transform.SetParent(soundGroupHelper.transform);
            transform.localScale = Vector3.one;

            if (mAudioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = mAudioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", soundGroupName, index));
                if (audioMixerGroups.Length > 0)
                {
                    soundAgentHelper.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    soundAgentHelper.AudioMixerGroup = soundGroupHelper.AudioMixerGroup;
                }
            }

            mSoundManager.AddSoundAgentHelper(soundGroupName, soundAgentHelper);

            return true;
        }

        private void OnPlaySoundSuccess(object sender, GameFramework.Sound.PlaySoundSuccessEventArgs e)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)e.UserData;
            if (playSoundInfo != null)
            {
                SoundAgentHelperBase soundAgentHelper = (SoundAgentHelperBase)e.SoundAgent.Helper;
                if (playSoundInfo.BindingEntity != null)
                {
                    soundAgentHelper.SetBindingEntity(playSoundInfo.BindingEntity);
                }
                else
                {
                    soundAgentHelper.SetWorldPosition(playSoundInfo.WorldPosition);
                }
            }

            mEventComponent.Fire(this, PlaySoundSuccessEventArgs.Create(e));
        }

        private void OnPlaySoundFailure(object sender, GameFramework.Sound.PlaySoundFailureEventArgs e)
        {
            string logMessage = Utility.Text.Format("Play sound failure, asset name '{0}', sound group name '{1}', error code '{2}', error message '{3}'.", e.SoundAssetName, e.SoundGroupName, e.ErrorCode, e.ErrorMessage);
            if (e.ErrorCode == PlaySoundErrorCode.IgnoredDueToLowPriority)
            {
                Log.Info(logMessage);
            }
            else
            {
                Log.Warning(logMessage);
            }

            mEventComponent.Fire(this, PlaySoundFailureEventArgs.Create(e));
        }

        private void OnPlaySoundUpdate(object sender, GameFramework.Sound.PlaySoundUpdateEventArgs e)
        {
            mEventComponent.Fire(this, PlaySoundUpdateEventArgs.Create(e));
        }

        private void OnPlaySoundDependencyAsset(object sender, GameFramework.Sound.PlaySoundDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, PlaySoundDependencyAssetEventArgs.Create(e));
        }

        private void OnLoadSceneSuccess(object sender, GameFramework.Scene.LoadSceneSuccessEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnLoadSceneFailure(object sender, GameFramework.Scene.LoadSceneFailureEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnUnloadSceneSuccess(object sender, GameFramework.Scene.UnloadSceneSuccessEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnUnloadSceneFailure(object sender, GameFramework.Scene.UnloadSceneFailureEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            RefreshAudioListener();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            RefreshAudioListener();
        }

        private void RefreshAudioListener()
        {
            mAudioListener.enabled = FindObjectsByType<AudioListener>().Length <= 1;
        }
    }
}

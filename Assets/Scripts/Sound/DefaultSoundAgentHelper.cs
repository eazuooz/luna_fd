//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Sound;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityGameFramework.Runtime
{
    public class DefaultSoundAgentHelper : SoundAgentHelperBase
    {
        private Transform mCachedTransform = null;
        private AudioSource mAudioSource = null;
        private EntityLogic mBindingEntityLogic = null;
        private float mVolumeWhenPause = 0f;
        private bool mApplicationPauseFlag = false;
        private EventHandler<ResetSoundAgentEventArgs> mResetSoundAgentEventHandler = null;

        public override bool IsPlaying
        {
            get
            {
                return mAudioSource.isPlaying;
            }
        }

        public override float Length
        {
            get
            {
                return mAudioSource.clip != null ? mAudioSource.clip.length : 0f;
            }
        }

        public override float Time
        {
            get
            {
                return mAudioSource.time;
            }
            set
            {
                // 클립이 없으면 Unity 가 대입을 무시하고 경고만 남긴다. 코어의 SoundAgent.Reset 이 클립 없이도 Time 을 되돌린다.
                if (mAudioSource.clip == null)
                {
                    return;
                }

                mAudioSource.time = value;
            }
        }

        public override bool Mute
        {
            get
            {
                return mAudioSource.mute;
            }
            set
            {
                mAudioSource.mute = value;
            }
        }

        public override bool Loop
        {
            get
            {
                return mAudioSource.loop;
            }
            set
            {
                mAudioSource.loop = value;
            }
        }

        public override int Priority
        {
            get
            {
                return 128 - mAudioSource.priority;
            }
            set
            {
                mAudioSource.priority = 128 - value;
            }
        }

        public override float Volume
        {
            get
            {
                return mAudioSource.volume;
            }
            set
            {
                mAudioSource.volume = value;
            }
        }

        public override float Pitch
        {
            get
            {
                return mAudioSource.pitch;
            }
            set
            {
                mAudioSource.pitch = value;
            }
        }

        public override float PanStereo
        {
            get
            {
                return mAudioSource.panStereo;
            }
            set
            {
                mAudioSource.panStereo = value;
            }
        }

        public override float SpatialBlend
        {
            get
            {
                return mAudioSource.spatialBlend;
            }
            set
            {
                mAudioSource.spatialBlend = value;
            }
        }

        public override float MaxDistance
        {
            get
            {
                return mAudioSource.maxDistance;
            }

            set
            {
                mAudioSource.maxDistance = value;
            }
        }

        public override float DopplerLevel
        {
            get
            {
                return mAudioSource.dopplerLevel;
            }
            set
            {
                mAudioSource.dopplerLevel = value;
            }
        }

        public override AudioMixerGroup AudioMixerGroup
        {
            get
            {
                return mAudioSource.outputAudioMixerGroup;
            }
            set
            {
                mAudioSource.outputAudioMixerGroup = value;
            }
        }

        public override event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent
        {
            add
            {
                mResetSoundAgentEventHandler += value;
            }
            remove
            {
                mResetSoundAgentEventHandler -= value;
            }
        }

        public override void Play(float fadeInSeconds)
        {
            StopAllCoroutines();

            mAudioSource.Play();
            if (fadeInSeconds > 0f)
            {
                float volume = mAudioSource.volume;
                mAudioSource.volume = 0f;
                StartCoroutine(FadeToVolume(mAudioSource, volume, fadeInSeconds));
            }
        }

        public override void Stop(float fadeOutSeconds)
        {
            StopAllCoroutines();

            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(StopCo(fadeOutSeconds));
            }
            else
            {
                mAudioSource.Stop();
            }
        }

        public override void Pause(float fadeOutSeconds)
        {
            StopAllCoroutines();

            mVolumeWhenPause = mAudioSource.volume;
            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(PauseCo(fadeOutSeconds));
            }
            else
            {
                mAudioSource.Pause();
            }
        }

        public override void Resume(float fadeInSeconds)
        {
            StopAllCoroutines();

            mAudioSource.UnPause();
            if (fadeInSeconds > 0f)
            {
                StartCoroutine(FadeToVolume(mAudioSource, mVolumeWhenPause, fadeInSeconds));
            }
            else
            {
                mAudioSource.volume = mVolumeWhenPause;
            }
        }

        public override void Reset()
        {
            mCachedTransform.localPosition = Vector3.zero;
            mAudioSource.clip = null;
            mBindingEntityLogic = null;
            mVolumeWhenPause = 0f;
        }

        public override bool SetSoundAsset(object soundAsset)
        {
            AudioClip audioClip = soundAsset as AudioClip;
            if (audioClip == null)
            {
                return false;
            }

            mAudioSource.clip = audioClip;
            return true;
        }

        public override void SetBindingEntity(Entity bindingEntity)
        {
            mBindingEntityLogic = bindingEntity.Logic;
            if (mBindingEntityLogic != null)
            {
                UpdateAgentPosition();
                return;
            }

            if (mResetSoundAgentEventHandler != null)
            {
                ResetSoundAgentEventArgs resetSoundAgentEventArgs = ResetSoundAgentEventArgs.Create();
                mResetSoundAgentEventHandler(this, resetSoundAgentEventArgs);
                ReferencePool.Release(resetSoundAgentEventArgs);
            }
        }

        public override void SetWorldPosition(Vector3 worldPosition)
        {
            mCachedTransform.position = worldPosition;
        }

        private void Awake()
        {
            mCachedTransform = transform;
            mAudioSource = gameObject.GetOrAddComponent<AudioSource>();
            mAudioSource.playOnAwake = false;
            mAudioSource.rolloffMode = AudioRolloffMode.Custom;
        }

        private void Update()
        {
            if (!mApplicationPauseFlag && !IsPlaying && mAudioSource.clip != null && mResetSoundAgentEventHandler != null)
            {
                ResetSoundAgentEventArgs resetSoundAgentEventArgs = ResetSoundAgentEventArgs.Create();
                mResetSoundAgentEventHandler(this, resetSoundAgentEventArgs);
                ReferencePool.Release(resetSoundAgentEventArgs);
                return;
            }

            if (mBindingEntityLogic != null)
            {
                UpdateAgentPosition();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            mApplicationPauseFlag = pause;
        }

        private void UpdateAgentPosition()
        {
            if (mBindingEntityLogic.Available)
            {
                mCachedTransform.position = mBindingEntityLogic.CachedTransform.position;
                return;
            }

            if (mResetSoundAgentEventHandler != null)
            {
                ResetSoundAgentEventArgs resetSoundAgentEventArgs = ResetSoundAgentEventArgs.Create();
                mResetSoundAgentEventHandler(this, resetSoundAgentEventArgs);
                ReferencePool.Release(resetSoundAgentEventArgs);
            }
        }

        private IEnumerator StopCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(mAudioSource, 0f, fadeOutSeconds);
            mAudioSource.Stop();
        }

        private IEnumerator PauseCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(mAudioSource, 0f, fadeOutSeconds);
            mAudioSource.Pause();
        }

        private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
        {
            float time = 0f;
            float originalVolume = audioSource.volume;
            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = volume;
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    /// <summary>Scene-authored presentation and one-time setup; the native Procedure component owns flow.</summary>
    [DefaultExecutionOrder(-1100)]
    public sealed class LunaBootstrap : GameFrameworkComponent
    {
        public Canvas rootCanvas;
        public Camera gameplayCamera;
        public SpriteRenderer fadeRenderer;
        public GameObject[] playerPrefabs;
        private Coroutine fade;
        private bool ready;
        private static LunaBootstrap Presentation => GameEntry.GetComponent<LunaBootstrap>();
        public static bool IsReady => Presentation != null && Presentation.ready;
        public static Canvas RootCanvas => Presentation != null ? Presentation.rootCanvas : null;
        public static Camera GameplayCamera => Presentation != null ? Presentation.gameplayCamera : null;

        protected override void Awake()
        {
            var existing = Presentation;
            if (existing != null && existing != this) { Destroy(gameObject); return; }
            base.Awake();
            DontDestroyOnLoad(gameObject);
            var prefab = Resources.Load<GameObject>("Luna/GameFramework");
            if (prefab == null) throw new InvalidOperationException("Missing supplied GameFramework prefab.");
            var framework = Instantiate(prefab, transform);
            framework.name = "GameFramework";
            framework.GetComponentInChildren<BaseComponent>().FrameRate = 60;
            var listener = framework.GetComponentInChildren<SoundComponent>().GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }

        // Called by LunaLaunchProcedure after the native components have completed Start.
        internal void InitializeGame()
        {
            if (ready) return;
            if (rootCanvas == null || gameplayCamera == null)
                throw new InvalidOperationException("Luna presentation references are missing.");
            LunaSpawn.Initialize();
            LunaSession.Initialize(playerPrefabs);
            LunaRuntimeData.Initialize();
            LunaStageTables.Initialize();
            LunaUI.Initialize();
            LunaScenes.Initialize();
            var sound = GameEntry.GetComponent<SoundComponent>();
            if (!sound.HasSoundGroup("LunaMusic")) sound.AddSoundGroup("LunaMusic", false, false, 0.5f, 1);
            ready = true;
        }

        internal static void Fade(float target, float seconds, Action completed)
        {
            var presentation = Presentation;
            if (presentation == null) { completed?.Invoke(); return; }
            if (presentation.fade != null) presentation.StopCoroutine(presentation.fade);
            presentation.fade = presentation.StartCoroutine(presentation.FadeRoutine(target, seconds, completed));
        }
        private IEnumerator FadeRoutine(float target, float seconds, Action completed)
        {
            if (fadeRenderer != null)
            {
                float start = fadeRenderer.color.a;
                for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
                {
                    var color = fadeRenderer.color;
                    color.a = Mathf.Lerp(start, target, elapsed / seconds);
                    fadeRenderer.color = color;
                    yield return null;
                }
                var final = fadeRenderer.color; final.a = target; fadeRenderer.color = final;
            }
            fade = null;
            completed?.Invoke();
        }
        private void OnDestroy() { ready = false; }
    }
}

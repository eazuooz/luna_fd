using System;
using System.Collections;
using GameFramework;
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
        private bool shutdownStarted, shutdownCompleted, modulesStopped, quitRequested, allowQuit;
        private bool shutdownFaulted;
        private int quitExitCode;
        private static LunaBootstrap presentation;
        private static LunaBootstrap Presentation => presentation != null ? presentation : GameEntry.GetComponent<LunaBootstrap>();
        public static bool IsReady => Presentation != null && Presentation.ready;
        public static bool IsShuttingDown => Presentation != null && Presentation.shutdownStarted;
        public static bool IsShutdownComplete => Presentation != null && Presentation.shutdownCompleted;
        public static Canvas RootCanvas => Presentation != null ? Presentation.rootCanvas : null;
        public static Camera GameplayCamera => Presentation != null ? Presentation.gameplayCamera : null;

        protected override void Awake()
        {
            var existing = Presentation;
            if (existing != null && existing != this) { Destroy(gameObject); return; }
            base.Awake();
            presentation = this;
            Application.wantsToQuit += OnWantsToQuit;
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
            if (ready || shutdownStarted) return;
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
        /// <summary>Stops native consumers before destroying their owned runtime assets.</summary>
        public static IEnumerator Shutdown()
        {
            var owner = Presentation;
            if (owner == null) yield break;
            if (owner.shutdownStarted)
            {
                while (!owner.shutdownCompleted) yield return null;
                yield break;
            }
            owner.shutdownStarted = true;
            owner.Quiesce();
            // Sound helpers reset stopped agents in Update, returning their AudioClip handles.
            yield return null;
            var resources = GameEntry.GetComponent<ResourceComponent>()?.ResourceManager as UnityGameFramework.Runtime.ResourceManager;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (resources != null && resources.PendingLoadCount > 0 && Time.realtimeSinceStartup < deadline)
                yield return null;
            owner.StopNativeModules(resources);
            // Addressables scene unloads are asynchronous even after all consumers stop.
            // Keep helpers alive until accepted operations have returned their final leases.
            deadline = Time.realtimeSinceStartup + 10f;
            while (resources != null && (resources.PendingLoadCount > 0 || resources.PendingUnloadCount > 0) && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (resources != null && (resources.LoadedHandleCount != 0 || resources.PendingLoadCount != 0 || resources.PendingUnloadCount != 0))
                Debug.LogError("Luna shutdown did not drain native resource leases within 10 seconds.");
            LunaResourcesProvider.Unregister();
            GameEntry.Shutdown(ShutdownType.None);
            owner.ReleaseOwnedAssets(false);
            // Destroy is deferred. Keep the player alive until the native objects are actually gone.
            yield return null;
            yield return Resources.UnloadUnusedAssets();
            owner.shutdownCompleted = true;
        }

        public static void Quit(int exitCode = 0)
        {
            var owner = Presentation;
            if (owner == null) { Application.Quit(exitCode); return; }
            owner.quitExitCode = exitCode;
            if (owner.quitRequested) return;
            owner.quitRequested = true;
            owner.StartCoroutine(owner.QuitRoutine());
        }
        private IEnumerator QuitRoutine()
        {
            // Drive this bounded shutdown explicitly: an exception in a nested Unity
            // coroutine must not leave wantsToQuit vetoing every subsequent close.
            IEnumerator shutdown = Shutdown();
            Exception failure = null;
            float deadline = Time.realtimeSinceStartup + 20f;
            try
            {
                while (failure == null)
                {
                    if (Time.realtimeSinceStartup >= deadline)
                    {
                        failure = new TimeoutException("Luna coordinated shutdown exceeded 20 seconds.");
                        break;
                    }
                    bool advanced;
                    object step = null;
                    try
                    {
                        advanced = shutdown.MoveNext();
                        if (advanced) step = shutdown.Current;
                    }
                    catch (Exception error) { failure = error; break; }
                    if (!advanced) break;
                    if (step is AsyncOperation operation)
                    {
                        while (!operation.isDone && Time.realtimeSinceStartup < deadline) yield return null;
                    }
                    else yield return step;
                }
            }
            finally
            {
                try { (shutdown as IDisposable)?.Dispose(); }
                catch (Exception error) { failure = failure ?? error; }
            }
            if (failure == null && !shutdownCompleted)
                failure = new InvalidOperationException("Luna shutdown returned without reaching its terminal state.");
            if (failure != null)
            {
                // Do not claim clean shutdown or destroy data with potentially live
                // consumers. Preserve the failure and let the process terminate.
                shutdownFaulted = true;
                if (quitExitCode == 0) quitExitCode = 1;
                Debug.LogError("Luna shutdown failed; exiting with code " + quitExitCode + ": " + failure);
            }
            allowQuit = true;
#if UNITY_EDITOR
            if (Application.isEditor) { UnityEditor.EditorApplication.isPlaying = false; yield break; }
#endif
            Application.Quit(quitExitCode);
        }
        private bool OnWantsToQuit()
        {
            // Editor stop cannot be postponed; its synchronous fallback is below.
            if (Application.isEditor || allowQuit) return true;
            Quit(quitExitCode);
            return false;
        }
        private void Quiesce()
        {
            bool wasReady = ready;
            ready = false;
            if (fade != null) { StopCoroutine(fade); fade = null; }
            var nativeBase = GameEntry.GetComponent<BaseComponent>();
            if (nativeBase != null) nativeBase.enabled = false;
            var procedures = GameEntry.GetComponent<ProcedureComponent>();
            if (procedures != null)
            {
                procedures.enabled = false;
                procedures.StopAllCoroutines();
            }
            // Disabling native ticks prevents a pending Procedure transition from starting
            // another round. Accepted loads are cancelled/drained without dispatching gameplay.
            // CurrentProcedure/HasProcedure throw until Procedure.Start initializes
            // its FSM. Before game setup completes there cannot be an active round.
            if (wasReady)
            {
                LunaScenes.CurrentGame?.Shutdown();
                LunaScenes.CurrentLobbyGame?.Shutdown();
            }
            LunaUI.CloseSceneForms();
            LunaSpawn.BeginShutdown();
            var sound = GameEntry.GetComponent<SoundComponent>();
            if (sound != null) { sound.StopAllLoadingSounds(); sound.StopAllLoadedSounds(0f); }
            LunaRuntimeData.SaveNow();
            (GameEntry.GetComponent<ResourceComponent>()?.ResourceManager as UnityGameFramework.Runtime.ResourceManager)?.BeginShutdown();
        }
        private void StopNativeModules(UnityGameFramework.Runtime.ResourceManager resources)
        {
            if (modulesStopped) return;
            // Unlike GameEntry.Shutdown(None), this keeps the component registry and all
            // release helpers alive until ObjectPool, Sound, Entity and UI finish releasing.
            // Return unclaimed requests before Core clears ReferencePool. Claimed data
            // stays with its entity and is returned by the native OnHide below.
            LunaSpawn.FinishShutdown();
            GameFrameworkEntry.Shutdown();
            modulesStopped = true;
            resources?.Shutdown();
        }
        private void ReleaseOwnedAssets(bool immediate)
        {
            if (!modulesStopped) throw new InvalidOperationException("Runtime assets still have active native consumers.");
            LunaUI.Shutdown();
            LunaUIViews.ReleaseRuntimeSprites(immediate);
            LunaRuntimeData.Shutdown(immediate);
            LunaSession.Shutdown();
        }
        private void OnApplicationQuit()
        {
            if (shutdownCompleted || shutdownFaulted) return;
            // Some platforms/editor stop cannot defer quitting. Only owner-created
            // DontSave objects are destroyed immediately, after native consumers stop.
            if (!Application.isEditor && shutdownStarted && !allowQuit) return;
            ShutdownImmediate();
        }
        private void ShutdownImmediate()
        {
            if (shutdownCompleted || presentation != this) return;
            shutdownStarted = true;
            Quiesce();
            StopNativeModules(GameEntry.GetComponent<ResourceComponent>()?.ResourceManager as UnityGameFramework.Runtime.ResourceManager);
            LunaResourcesProvider.Unregister();
            ReleaseOwnedAssets(true);
            GameEntry.Shutdown(ShutdownType.None);
            shutdownCompleted = true;
        }
        private void OnDestroy()
        {
            Application.wantsToQuit -= OnWantsToQuit;
            if (presentation != this) return;
            try { if (!shutdownFaulted) ShutdownImmediate(); }
            finally { ready = false; presentation = null; }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    /// <summary>Luna's serialized facade over Game Framework and game-specific services.</summary>
    [DefaultExecutionOrder(-1100)]
    public class Game : Singleton<Game>
    {
        public static event Action BeforeInitialize;
        public ScreenManager screenManager;
        public ResourcesManager resourcesManager;
        public CryptoManager cryptoManager;
        public UIManager uiManager;
        public DataManager dataManager;
        public LunaSceneManager lunaSceneManager;
        public LightManager lightManager;
        public TutorialManager tutorialManager;
        public LanguageManager languageManager;
        public InputManager inputManager;
        public SoundManager soundManager;

        private readonly List<IManager> managers = new List<IManager>();
        private bool initialized;
        private bool ownsFramework;
        public bool IsInitialized => initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents() { BeforeInitialize = null; }

        private void Awake()
        {
            foreach (var other in FindObjectsByType<Game>())
                if (other != this && other.ownsFramework) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
            var prefab = Resources.Load<GameObject>("Luna/GameFramework");
            if (prefab == null) throw new InvalidOperationException("Luna Game Framework prefab is missing.");
            var framework = Instantiate(prefab, transform);
            framework.name = "GameFramework";
            framework.GetComponentInChildren<ProcedureComponent>().enabled = false;
            framework.GetComponentInChildren<BaseComponent>().FrameRate = 60;
            var listener = framework.GetComponentInChildren<SoundComponent>().GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
            ownsFramework = true;
        }

        protected virtual IEnumerator Start()
        {
            if (!ownsFramework) yield break;
            yield return null; // Framework helper Start methods must run before opening UI/settings.
            LunaEntities.Initialize(this);
            BeforeInitialize?.Invoke();
            managers.AddRange(new IManager[] { screenManager, resourcesManager, cryptoManager,
                dataManager, lunaSceneManager, lightManager, tutorialManager, languageManager,
                inputManager, soundManager, uiManager });
            foreach (var manager in managers) manager?.ManagerInitialize();
            initialized = true;
            var procedures = GameFrameworkEntry.GetModule<IProcedureManager>();
            procedures.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(),
                new LunaLoadingProcedure(), new LunaLobbyProcedure(), new LunaBattleProcedure());
            procedures.StartProcedure<LunaLoadingProcedure>();
            StartCoroutine(Loop());
            Debug.Log("Luna initialized with Game Framework: Procedure, Entity, UI, Resource, ObjectPool, Sound and Setting.");
        }

        protected virtual void Update()
        {
            if (!initialized) return;
            foreach (var manager in managers) manager?.ManagerUpdate();
        }

        protected virtual void OnDestroy()
        {
            if (!ownsFramework) return;
            initialized = false;
            for (int i = managers.Count - 1; i >= 0; --i) managers[i]?.ManagerClear();
        }

        protected virtual void GameLoop()
        {
            foreach (var manager in managers) manager?.ManagerLoop();
        }

        private IEnumerator Loop()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (initialized) { yield return wait; GameLoop(); }
        }
    }
}

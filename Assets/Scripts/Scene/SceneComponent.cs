//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Resource;
using GameFramework.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Scene")]
    public sealed class SceneComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private ISceneManager mSceneManager = null;
        private EventComponent mEventComponent = null;
        private readonly SortedDictionary<string, int> mSceneOrder = new SortedDictionary<string, int>(StringComparer.Ordinal);
        private Camera mMainCamera = null;
        private Scene mGameFrameworkScene = default(Scene);

        [SerializeField]
        private bool mEnableLoadSceneUpdateEvent = true;

        [SerializeField]
        private bool mEnableLoadSceneDependencyAssetEvent = true;

        public Camera MainCamera
        {
            get
            {
                return mMainCamera;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mSceneManager = GameFrameworkEntry.GetModule<ISceneManager>();
            if (mSceneManager == null)
            {
                Log.Fatal("Scene manager is invalid.");
                return;
            }

            mSceneManager.LoadSceneSuccess += OnLoadSceneSuccess;
            mSceneManager.LoadSceneFailure += OnLoadSceneFailure;

            if (mEnableLoadSceneUpdateEvent)
            {
                mSceneManager.LoadSceneUpdate += OnLoadSceneUpdate;
            }

            if (mEnableLoadSceneDependencyAssetEvent)
            {
                mSceneManager.LoadSceneDependencyAsset += OnLoadSceneDependencyAsset;
            }

            mSceneManager.UnloadSceneSuccess += OnUnloadSceneSuccess;
            mSceneManager.UnloadSceneFailure += OnUnloadSceneFailure;

            mGameFrameworkScene = SceneManager.GetSceneAt(GameEntry.GameFrameworkSceneId);
            if (!mGameFrameworkScene.IsValid())
            {
                Log.Fatal("Game Framework scene is invalid.");
                return;
            }
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

            mSceneManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);
        }

        public static string GetSceneName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return null;
            }

            int sceneNamePosition = sceneAssetName.LastIndexOf('/');
            if (sceneNamePosition + 1 >= sceneAssetName.Length)
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return null;
            }

            string sceneName = sceneAssetName.Substring(sceneNamePosition + 1);
            sceneNamePosition = sceneName.LastIndexOf(".unity");
            if (sceneNamePosition > 0)
            {
                sceneName = sceneName.Substring(0, sceneNamePosition);
            }

            return sceneName;
        }

        public bool SceneIsLoaded(string sceneAssetName)
        {
            return mSceneManager.SceneIsLoaded(sceneAssetName);
        }

        public string[] GetLoadedSceneAssetNames()
        {
            return mSceneManager.GetLoadedSceneAssetNames();
        }

        public void GetLoadedSceneAssetNames(List<string> results)
        {
            mSceneManager.GetLoadedSceneAssetNames(results);
        }

        public bool SceneIsLoading(string sceneAssetName)
        {
            return mSceneManager.SceneIsLoading(sceneAssetName);
        }

        public string[] GetLoadingSceneAssetNames()
        {
            return mSceneManager.GetLoadingSceneAssetNames();
        }

        public void GetLoadingSceneAssetNames(List<string> results)
        {
            mSceneManager.GetLoadingSceneAssetNames(results);
        }

        public bool SceneIsUnloading(string sceneAssetName)
        {
            return mSceneManager.SceneIsUnloading(sceneAssetName);
        }

        public string[] GetUnloadingSceneAssetNames()
        {
            return mSceneManager.GetUnloadingSceneAssetNames();
        }

        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            mSceneManager.GetUnloadingSceneAssetNames(results);
        }

        public bool HasScene(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return false;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return false;
            }

            return mSceneManager.HasScene(sceneAssetName);
        }

        public void LoadScene(string sceneAssetName)
        {
            LoadScene(sceneAssetName, DefaultPriority, null);
        }

        public void LoadScene(string sceneAssetName, int priority)
        {
            LoadScene(sceneAssetName, priority, null);
        }

        public void LoadScene(string sceneAssetName, object userData)
        {
            LoadScene(sceneAssetName, DefaultPriority, userData);
        }

        public void LoadScene(string sceneAssetName, int priority, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return;
            }

            mSceneManager.LoadScene(sceneAssetName, priority, userData);
        }

        public void UnloadScene(string sceneAssetName)
        {
            UnloadScene(sceneAssetName, null);
        }

        public void UnloadScene(string sceneAssetName, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return;
            }

            mSceneManager.UnloadScene(sceneAssetName, userData);
            mSceneOrder.Remove(sceneAssetName);
        }

        public void SetSceneOrder(string sceneAssetName, int sceneOrder)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return;
            }

            if (SceneIsLoading(sceneAssetName))
            {
                mSceneOrder[sceneAssetName] = sceneOrder;
                return;
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                mSceneOrder[sceneAssetName] = sceneOrder;
                RefreshSceneOrder();
                return;
            }

            Log.Error("Scene '{0}' is not loaded or loading.", sceneAssetName);
        }

        public void RefreshMainCamera()
        {
            mMainCamera = Camera.main;
        }

        private void RefreshSceneOrder()
        {
            if (mSceneOrder.Count > 0)
            {
                string maxSceneName = null;
                int maxSceneOrder = 0;
                foreach (KeyValuePair<string, int> sceneOrder in mSceneOrder)
                {
                    if (SceneIsLoading(sceneOrder.Key))
                    {
                        continue;
                    }

                    if (maxSceneName == null)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                        continue;
                    }

                    if (sceneOrder.Value > maxSceneOrder)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                    }
                }

                if (maxSceneName == null)
                {
                    SetActiveScene(mGameFrameworkScene);
                    return;
                }

                Scene scene = SceneManager.GetSceneByName(GetSceneName(maxSceneName));
                if (!scene.IsValid())
                {
                    Log.Error("Active scene '{0}' is invalid.", maxSceneName);
                    return;
                }

                SetActiveScene(scene);
            }
            else
            {
                SetActiveScene(mGameFrameworkScene);
            }
        }

        private void SetActiveScene(Scene activeScene)
        {
            Scene lastActiveScene = SceneManager.GetActiveScene();
            if (lastActiveScene != activeScene)
            {
                SceneManager.SetActiveScene(activeScene);
                mEventComponent.Fire(this, ActiveSceneChangedEventArgs.Create(lastActiveScene, activeScene));
            }

            RefreshMainCamera();
        }

        private void OnLoadSceneSuccess(object sender, GameFramework.Scene.LoadSceneSuccessEventArgs e)
        {
            if (!mSceneOrder.ContainsKey(e.SceneAssetName))
            {
                mSceneOrder.Add(e.SceneAssetName, 0);
            }

            mEventComponent.Fire(this, LoadSceneSuccessEventArgs.Create(e));
            RefreshSceneOrder();
        }

        private void OnLoadSceneFailure(object sender, GameFramework.Scene.LoadSceneFailureEventArgs e)
        {
            Log.Warning("Load scene failure, scene asset name '{0}', error message '{1}'.", e.SceneAssetName, e.ErrorMessage);
            mEventComponent.Fire(this, LoadSceneFailureEventArgs.Create(e));
        }

        private void OnLoadSceneUpdate(object sender, GameFramework.Scene.LoadSceneUpdateEventArgs e)
        {
            mEventComponent.Fire(this, LoadSceneUpdateEventArgs.Create(e));
        }

        private void OnLoadSceneDependencyAsset(object sender, GameFramework.Scene.LoadSceneDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, LoadSceneDependencyAssetEventArgs.Create(e));
        }

        private void OnUnloadSceneSuccess(object sender, GameFramework.Scene.UnloadSceneSuccessEventArgs e)
        {
            mEventComponent.Fire(this, UnloadSceneSuccessEventArgs.Create(e));
            mSceneOrder.Remove(e.SceneAssetName);
            RefreshSceneOrder();
        }

        private void OnUnloadSceneFailure(object sender, GameFramework.Scene.UnloadSceneFailureEventArgs e)
        {
            Log.Warning("Unload scene failure, scene asset name '{0}'.", e.SceneAssetName);
            mEventComponent.Fire(this, UnloadSceneFailureEventArgs.Create(e));
        }
    }
}

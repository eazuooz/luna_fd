//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
//------------------------------------------------------------

using GameFramework.Resource;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public sealed class ResourceComponent : GameFrameworkComponent
    {
        private IResourceManager mResourceManager = null;

        [SerializeField]
        private float mMinUnloadUnusedAssetsInterval = 60f;

        [SerializeField]
        private float mMaxUnloadUnusedAssetsInterval = 300f;

        private float mLastUnloadUnusedAssetsOperationElapseSeconds = 0f;
        private bool mForceUnloadUnusedAssets = false;
        private bool mPreorderUnloadUnusedAssets = false;
        private bool mPerformGCCollect = false;
        private AsyncOperation mAsyncOperation = null;

        /// <summary>
        /// 다른 컴포넌트들이 IResourceManager를 주입받을 때 사용합니다.
        /// </summary>
        public IResourceManager ResourceManager => mResourceManager;

        // Debugger 호환용 (Addressables 모드에서는 의미 없음)
        public string ApplicableGameVersion => string.Empty;
        public int InternalResourceVersion => 0;

        protected override void Awake()
        {
            base.Awake();
            mResourceManager = new ResourceManager();
        }

        private void Start()
        {
            mResourceManager.InitResources(OnInitResourcesComplete);
        }

        private void Update()
        {
            mLastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (mAsyncOperation == null &&
                (mForceUnloadUnusedAssets ||
                 mLastUnloadUnusedAssetsOperationElapseSeconds >= mMaxUnloadUnusedAssetsInterval ||
                 mPreorderUnloadUnusedAssets && mLastUnloadUnusedAssetsOperationElapseSeconds >= mMinUnloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                mForceUnloadUnusedAssets = false;
                mPreorderUnloadUnusedAssets = false;
                mLastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                mAsyncOperation = Resources.UnloadUnusedAssets();
            }

            if (mAsyncOperation != null && mAsyncOperation.isDone)
            {
                mAsyncOperation = null;
                if (mPerformGCCollect)
                {
                    Log.Info("GC.Collect...");
                    mPerformGCCollect = false;
                    GC.Collect();
                }
            }
        }

        private void OnInitResourcesComplete()
        {
            Log.Info("Addressables resource manager initialized.");
        }

        // ─── 공개 API (기존과 동일한 인터페이스 유지) ───

        public HasAssetResult HasAsset(string assetName) =>
            mResourceManager.HasAsset(assetName);

        public void InitResources(InitResourcesCompleteCallback initResourcesCompleteCallback) =>
            mResourceManager.InitResources(initResourcesCompleteCallback);

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks) =>
            mResourceManager.LoadAsset(assetName, loadAssetCallbacks);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks) =>
            mResourceManager.LoadAsset(assetName, assetType, loadAssetCallbacks);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks) =>
            mResourceManager.LoadAsset(assetName, priority, loadAssetCallbacks);

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            mResourceManager.LoadAsset(assetName, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks) =>
            mResourceManager.LoadAsset(assetName, assetType, priority, loadAssetCallbacks);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            mResourceManager.LoadAsset(assetName, assetType, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            mResourceManager.LoadAsset(assetName, priority, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            mResourceManager.LoadAsset(assetName, assetType, priority, loadAssetCallbacks, userData);

        public void UnloadAsset(object asset) =>
            mResourceManager.UnloadAsset(asset);

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks) =>
            mResourceManager.LoadScene(sceneAssetName, loadSceneCallbacks);

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks) =>
            mResourceManager.LoadScene(sceneAssetName, priority, loadSceneCallbacks);

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData) =>
            mResourceManager.LoadScene(sceneAssetName, loadSceneCallbacks, userData);

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData) =>
            mResourceManager.LoadScene(sceneAssetName, priority, loadSceneCallbacks, userData);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks) =>
            mResourceManager.UnloadScene(sceneAssetName, unloadSceneCallbacks);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData) =>
            mResourceManager.UnloadScene(sceneAssetName, unloadSceneCallbacks, userData);

        public void UnloadUnusedAssets(bool performGCCollect)
        {
            mPreorderUnloadUnusedAssets = true;
            if (performGCCollect)
                mPerformGCCollect = true;
        }

        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            mForceUnloadUnusedAssets = true;
            if (performGCCollect)
                mPerformGCCollect = true;
        }
    }
}

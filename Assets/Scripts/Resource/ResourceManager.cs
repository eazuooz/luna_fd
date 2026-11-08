//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
//------------------------------------------------------------

using GameFramework;
using GameFramework.Download;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// Addressables 기반 IResourceManager 구현체.
    /// GF의 에셋번들 기반 ResourceManager를 대체합니다.
    /// </summary>
    public sealed class ResourceManager : IResourceManager
    {
        // One lease per acquisition: equal Addressables handles still own separate references.
        // Index by the returned object, not just its address (aliases can return the same object).
        private sealed class AssetLease
        {
            internal AsyncOperationHandle<object> Handle;
            internal object Asset;
            internal bool Released;
        }

        private sealed class SceneLease
        {
            internal string Name;
            internal AsyncOperationHandle<SceneInstance> Handle;
            internal bool Loading = true;
            internal bool Unloading;
        }

        private readonly HashSet<AssetLease> mPendingAssets = new HashSet<AssetLease>();
        private readonly Dictionary<object, List<AssetLease>> mAssetHandles = new Dictionary<object, List<AssetLease>>();
        private readonly List<SceneLease> mSceneHandles = new List<SceneLease>();
        private bool mIsShuttingDown;
        private bool mIsShutdown;

        public bool IsShuttingDown => mIsShuttingDown;
        public bool IsShutdown => mIsShutdown;
        public int LoadedHandleCount
        {
            get
            {
                int count = 0;
                foreach (List<AssetLease> leases in mAssetHandles.Values) count += leases.Count;
                foreach (SceneLease lease in mSceneHandles) if (!lease.Loading && !lease.Unloading) count++;
                return count;
            }
        }
        public int PendingLoadCount
        {
            get
            {
                int count = mPendingAssets.Count;
                foreach (SceneLease lease in mSceneHandles) if (lease.Loading) count++;
                return count;
            }
        }
        public int PendingUnloadCount
        {
            get
            {
                int count = 0;
                foreach (SceneLease lease in mSceneHandles) if (lease.Unloading) count++;
                return count;
            }
        }

        /// <summary>Reject new work while accepted loads still notify their native owners.</summary>
        public void BeginShutdown() => mIsShuttingDown = true;

        /// <summary>
        /// Call after native modules and their pools have released their objects. Late loads
        /// cannot notify consumers; scene loads are allowed to finish only to unload them.
        /// </summary>
        public void Shutdown()
        {
            if (mIsShutdown) return;
            mIsShuttingDown = mIsShutdown = true;
            foreach (AssetLease lease in new List<AssetLease>(mPendingAssets)) ReleaseAsset(lease);
            var loaded = new List<AssetLease>();
            foreach (List<AssetLease> leases in mAssetHandles.Values) loaded.AddRange(leases);
            foreach (AssetLease lease in loaded) ReleaseAsset(lease);
            foreach (SceneLease lease in mSceneHandles.ToArray())
                if (!lease.Loading && !lease.Unloading) StartUnloadScene(lease, null, null);
        }

        // ─── Addressables에서 사용 안 하는 프로퍼티들 (기본값 반환) ───

        public string ReadOnlyPath => string.Empty;
        public string ReadWritePath => string.Empty;
        public ResourceMode ResourceMode => ResourceMode.Package;
        public string CurrentVariant => null;
        public PackageVersionListSerializer PackageVersionListSerializer => null;
        public UpdatableVersionListSerializer UpdatableVersionListSerializer => null;
        public ReadOnlyVersionListSerializer ReadOnlyVersionListSerializer => null;
        public ReadWriteVersionListSerializer ReadWriteVersionListSerializer => null;
        public ResourcePackVersionListSerializer ResourcePackVersionListSerializer => null;
        public string ApplicableGameVersion => string.Empty;
        public int InternalResourceVersion => 0;
        public int AssetCount => mAssetHandles.Count;
        public int ResourceCount => 0;
        public int ResourceGroupCount => 0;
        public string UpdatePrefixUri { get => string.Empty; set { } }
        public int GenerateReadWriteVersionListLength { get => 0; set { } }
        public string ApplyingResourcePackPath => string.Empty;
        public int ApplyWaitingCount => 0;
        public int UpdateRetryCount { get => 0; set { } }
        public IResourceGroup UpdatingResourceGroup => null;
        public int UpdateWaitingCount => 0;
        public int UpdateWaitingWhilePlayingCount => 0;
        public int UpdateCandidateCount => 0;
        public int LoadTotalAgentCount => 0;
        public int LoadFreeAgentCount => 0;
        public int LoadWorkingAgentCount => 0;
        public int LoadWaitingTaskCount => 0;
        public float AssetAutoReleaseInterval { get => 0f; set { } }
        public int AssetCapacity { get => 0; set { } }
        public float AssetExpireTime { get => 0f; set { } }
        public int AssetPriority { get => 0; set { } }
        public float ResourceAutoReleaseInterval { get => 0f; set { } }
        public int ResourceCapacity { get => 0; set { } }
        public float ResourceExpireTime { get => 0f; set { } }
        public int ResourcePriority { get => 0; set { } }

        // ─── Addressables에서 사용 안 하는 이벤트들 (no-op) ───

        public event EventHandler<ResourceVerifyStartEventArgs> ResourceVerifyStart { add { } remove { } }
        public event EventHandler<ResourceVerifySuccessEventArgs> ResourceVerifySuccess { add { } remove { } }
        public event EventHandler<ResourceVerifyFailureEventArgs> ResourceVerifyFailure { add { } remove { } }
        public event EventHandler<ResourceApplyStartEventArgs> ResourceApplyStart { add { } remove { } }
        public event EventHandler<ResourceApplySuccessEventArgs> ResourceApplySuccess { add { } remove { } }
        public event EventHandler<ResourceApplyFailureEventArgs> ResourceApplyFailure { add { } remove { } }
        public event EventHandler<ResourceUpdateStartEventArgs> ResourceUpdateStart { add { } remove { } }
        public event EventHandler<ResourceUpdateChangedEventArgs> ResourceUpdateChanged { add { } remove { } }
        public event EventHandler<ResourceUpdateSuccessEventArgs> ResourceUpdateSuccess { add { } remove { } }
        public event EventHandler<ResourceUpdateFailureEventArgs> ResourceUpdateFailure { add { } remove { } }
        public event EventHandler<ResourceUpdateAllCompleteEventArgs> ResourceUpdateAllComplete { add { } remove { } }

        // ─── 셋업 메서드들 (Addressables는 자체 초기화, no-op) ───

        public void SetReadOnlyPath(string readOnlyPath) { }
        public void SetReadWritePath(string readWritePath) { }
        public void SetResourceMode(ResourceMode resourceMode) { }
        public void SetCurrentVariant(string currentVariant) { }
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager) { }
        public void SetFileSystemManager(IFileSystemManager fileSystemManager) { }
        public void SetDownloadManager(IDownloadManager downloadManager) { }
        public void SetDecryptResourceCallback(DecryptResourceCallback decryptResourceCallback) { }
        public void SetResourceHelper(IResourceHelper resourceHelper) { }
        public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper) { }

        // ─── 초기화 ───

        /// <summary>
        /// Addressables는 자동으로 초기화되므로 콜백을 즉시 호출합니다.
        /// </summary>
        public void InitResources(InitResourcesCompleteCallback initResourcesCompleteCallback)
        {
            initResourcesCompleteCallback?.Invoke();
        }

        // ─── 버전/업데이트 (Addressables가 자체 처리, no-op) ───

        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion) => CheckVersionListResult.Updated;
        public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode, UpdateVersionListCallbacks updateVersionListCallbacks) { }
        public void VerifyResources(int verifyResourceLengthPerFrame, VerifyResourcesCompleteCallback verifyResourcesCompleteCallback) => verifyResourcesCompleteCallback?.Invoke(true);
        public void CheckResources(bool ignoreOtherVariant, CheckResourcesCompleteCallback checkResourcesCompleteCallback) => checkResourcesCompleteCallback?.Invoke(0, 0, 0, 0L, 0L);
        public void ApplyResources(string resourcePackPath, ApplyResourcesCompleteCallback applyResourcesCompleteCallback) { }
        public void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback) => updateResourcesCompleteCallback?.Invoke(null, true);
        public void UpdateResources(string resourceGroupName, UpdateResourcesCompleteCallback updateResourcesCompleteCallback) => updateResourcesCompleteCallback?.Invoke(null, true);
        public void StopUpdateResources() { }
        public bool VerifyResourcePack(string resourcePackPath) => false;

        // ─── TaskInfo ───

        public TaskInfo[] GetAllLoadAssetInfos() => new TaskInfo[0];
        public void GetAllLoadAssetInfos(List<TaskInfo> results) { }

        // ─── 에셋 존재 여부 ───

        public HasAssetResult HasAsset(string assetName) => HasAssetResult.AssetOnDisk;

        // ─── LoadAsset ───

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, null, 0, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, assetType, 0, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, null, priority, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            LoadAsset(assetName, null, 0, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, assetType, priority, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            LoadAsset(assetName, assetType, 0, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            LoadAsset(assetName, null, priority, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (mIsShuttingDown)
            {
                loadAssetCallbacks?.LoadAssetFailureCallback?.Invoke(assetName, LoadResourceStatus.NotExist, "Resource manager is shutting down.", userData);
                return;
            }
            if (string.IsNullOrEmpty(assetName))
            {
                loadAssetCallbacks?.LoadAssetFailureCallback?.Invoke(assetName, LoadResourceStatus.NotExist, "Asset name is invalid.", userData);
                return;
            }

            float startTime = Time.time;
            var lease = new AssetLease { Handle = Addressables.LoadAssetAsync<object>(assetName) };
            mPendingAssets.Add(lease);
            lease.Handle.Completed += op =>
            {
                // Releasing an in-flight Addressables operation is supported. Its internal
                // completion may still arrive, but this request no longer owns a reference.
                mPendingAssets.Remove(lease);
                if (lease.Released) return;
                if (mIsShutdown)
                {
                    ReleaseAsset(lease);
                    return;
                }
                if (op.Status == AsyncOperationStatus.Succeeded && op.Result != null)
                {
                    lease.Asset = op.Result;
                    if (!mAssetHandles.TryGetValue(lease.Asset, out List<AssetLease> handles))
                    {
                        handles = new List<AssetLease>();
                        mAssetHandles.Add(lease.Asset, handles);
                    }
                    handles.Add(lease);
                    loadAssetCallbacks?.LoadAssetSuccessCallback?.Invoke(assetName, op.Result, Time.time - startTime, userData);
                }
                else
                {
                    // 실패한 오퍼레이션도 반납해야 한다. 없으면 잘못된 주소를 반복 요청할 때마다 샌다.
                    // Releasing invalidates the handle, so read its exception first.
                    string errorMsg = op.OperationException != null ? op.OperationException.Message : $"Addressables failed to load '{assetName}'.";
                    ReleaseAsset(lease);
                    loadAssetCallbacks?.LoadAssetFailureCallback?.Invoke(assetName, LoadResourceStatus.NotExist, errorMsg, userData);
                }
            };
        }

        public void UnloadAsset(object asset)
        {
            if (asset != null && mAssetHandles.TryGetValue(asset, out List<AssetLease> handles) && handles.Count > 0)
                ReleaseAsset(handles[handles.Count - 1]);
        }

        private void ReleaseAsset(AssetLease lease)
        {
            if (lease.Released) return;
            lease.Released = true;
            // A released in-flight operation still finishes internally. Keep it in the
            // pending set until its completion callback so shutdown can genuinely drain it.
            if (lease.Asset != null && mAssetHandles.TryGetValue(lease.Asset, out List<AssetLease> handles))
            {
                handles.Remove(lease);
                if (handles.Count == 0) mAssetHandles.Remove(lease.Asset);
            }
            lease.Asset = null;
            if (lease.Handle.IsValid()) Addressables.Release(lease.Handle);
        }

        // ─── LoadScene ───

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks) =>
            LoadScene(sceneAssetName, 0, loadSceneCallbacks, null);

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks) =>
            LoadScene(sceneAssetName, priority, loadSceneCallbacks, null);

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData) =>
            LoadScene(sceneAssetName, 0, loadSceneCallbacks, userData);

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            if (mIsShuttingDown)
            {
                loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.NotExist, "Resource manager is shutting down.", userData);
                return;
            }
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.NotExist, "Scene asset name is invalid.", userData);
                return;
            }

            float startTime = Time.time;
            var lease = new SceneLease { Name = sceneAssetName, Handle = Addressables.LoadSceneAsync(sceneAssetName, LoadSceneMode.Additive) };
            mSceneHandles.Add(lease);
            lease.Handle.Completed += op =>
            {
                lease.Loading = false;
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    if (mIsShutdown) StartUnloadScene(lease, null, null);
                    else loadSceneCallbacks?.LoadSceneSuccessCallback?.Invoke(sceneAssetName, Time.time - startTime, userData);
                }
                else
                {
                    string errorMsg = op.OperationException != null ? op.OperationException.Message : $"Addressables failed to load scene '{sceneAssetName}'.";
                    mSceneHandles.Remove(lease);
                    Addressables.Release(op);
                    if (!mIsShutdown) loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.NotExist, errorMsg, userData);
                }
            };
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks) =>
            UnloadScene(sceneAssetName, unloadSceneCallbacks, null);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            SceneLease lease = mSceneHandles.Find(candidate => candidate.Name == sceneAssetName && !candidate.Loading && !candidate.Unloading);
            if (lease == null)
            {
                if (!mIsShutdown) unloadSceneCallbacks?.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
                return;
            }
            StartUnloadScene(lease, unloadSceneCallbacks, userData);
        }

        private void StartUnloadScene(SceneLease lease, UnloadSceneCallbacks callbacks, object userData)
        {
            if (lease.Unloading) return;
            if (!lease.Handle.IsValid())
            {
                // Addressables may already have released a scene unloaded externally.
                mSceneHandles.Remove(lease);
                if (!mIsShutdown) callbacks?.UnloadSceneSuccessCallback?.Invoke(lease.Name, userData);
                return;
            }
            lease.Unloading = true;
            // SceneProvider owns the load-handle release during unload. Only release the
            // returned unload operation here; releasing the load handle too would double it.
            AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(lease.Handle, autoReleaseHandle: false);
            unloadHandle.Completed += op =>
            {
                mSceneHandles.Remove(lease);
                try
                {
                    if (!mIsShutdown)
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded) callbacks?.UnloadSceneSuccessCallback?.Invoke(lease.Name, userData);
                        else callbacks?.UnloadSceneFailureCallback?.Invoke(lease.Name, userData);
                    }
                }
                finally
                {
                    if (op.IsValid()) Addressables.Release(op);
                }
            };
        }

        // ─── 바이너리 (Addressables에서는 LoadAsset으로 대체, 미지원) ───

        public string GetBinaryPath(string binaryAssetName) => null;

        public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem, out string relativePath, out string fileName)
        {
            storageInReadOnly = false;
            storageInFileSystem = false;
            relativePath = null;
            fileName = null;
            return false;
        }

        public int GetBinaryLength(string binaryAssetName) => -1;

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks) =>
            loadBinaryCallbacks?.LoadBinaryFailureCallback?.Invoke(binaryAssetName, LoadResourceStatus.NotExist, "Binary loading is not supported in Addressables mode. Use LoadAsset instead.", null);

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData) =>
            loadBinaryCallbacks?.LoadBinaryFailureCallback?.Invoke(binaryAssetName, LoadResourceStatus.NotExist, "Binary loading is not supported in Addressables mode. Use LoadAsset instead.", userData);

        public byte[] LoadBinaryFromFileSystem(string binaryAssetName) => null;
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer) => 0;
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex) => 0;
        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length) => 0;
        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length) => null;
        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length) => null;
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer) => 0;
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int length) => 0;
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length) => 0;
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer) => 0;
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int length) => 0;
        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex, int length) => 0;

        // ─── 리소스 그룹 (Addressables Labels/Groups로 대체, 미지원) ───

        public bool HasResourceGroup(string resourceGroupName) => false;
        public IResourceGroup GetResourceGroup() => null;
        public IResourceGroup GetResourceGroup(string resourceGroupName) => null;
        public IResourceGroup[] GetAllResourceGroups() => new IResourceGroup[0];
        public void GetAllResourceGroups(List<IResourceGroup> results) { }
        public IResourceGroupCollection GetResourceGroupCollection(params string[] resourceGroupNames) => null;
        public IResourceGroupCollection GetResourceGroupCollection(List<string> resourceGroupNames) => null;
    }
}

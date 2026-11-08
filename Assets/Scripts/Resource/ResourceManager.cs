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
        // Resident assets let game-specific Entity helpers preserve serialized prefab references.
        // They are explicitly registered/unregistered by the owning game; Addressables still owns
        // all other keys and their reference counts.
        private readonly Dictionary<string, object> mResidentAssets = new Dictionary<string, object>();
        public void RegisterResidentAsset(string key, object asset)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Resident asset key is empty.", nameof(key));
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            mResidentAssets[key] = asset;
        }
        public void UnregisterResidentAsset(string key) { mResidentAssets.Remove(key); }

        // 로드된 에셋 핸들 추적 (UnloadAsset 시 Release하기 위해)
        //
        // 주소(assetName) 하나당 핸들 목록을 들고 취득 1회 : 해제 1회로 맞춘다.
        // Addressables 는 같은 주소를 N번 로드하면 내부 참조 카운트를 N으로 올리고
        // 매번 동등한 핸들을 돌려주므로, 로드 결과 오브젝트를 키로 삼으면
        // 2회차 이후 핸들이 추적되지 않아 그만큼 영구 누수된다.
        // (같은 주소 중복 로드는 캐릭터 선택 → 플레이어 스폰에서 실제로 발생한다.)
        private readonly Dictionary<string, List<AsyncOperationHandle>> mAssetHandles = new Dictionary<string, List<AsyncOperationHandle>>();
        // 로드 결과 오브젝트 → 주소 역참조. UnloadAsset 이 에셋만 받기 때문에 필요하다.
        private readonly Dictionary<object, string> mAssetNames = new Dictionary<object, string>();
        // 로드된 씬 핸들 추적
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> mSceneHandles = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();

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
            if (string.IsNullOrEmpty(assetName))
            {
                loadAssetCallbacks?.LoadAssetFailureCallback?.Invoke(assetName, LoadResourceStatus.NotExist, "Asset name is invalid.", userData);
                return;
            }

            if (mResidentAssets.TryGetValue(assetName, out var resident))
            {
                loadAssetCallbacks?.LoadAssetSuccessCallback?.Invoke(assetName, resident, 0f, userData);
                return;
            }

            float startTime = Time.time;
            AsyncOperationHandle<object> handle = Addressables.LoadAssetAsync<object>(assetName);

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded && op.Result != null)
                {
                    if (!mAssetHandles.TryGetValue(assetName, out List<AsyncOperationHandle> handles))
                    {
                        handles = new List<AsyncOperationHandle>();
                        mAssetHandles.Add(assetName, handles);
                    }

                    handles.Add(op);
                    mAssetNames[op.Result] = assetName;

                    loadAssetCallbacks?.LoadAssetSuccessCallback?.Invoke(assetName, op.Result, Time.time - startTime, userData);
                }
                else
                {
                    // 실패한 오퍼레이션도 반납해야 한다. 없으면 잘못된 주소를 반복 요청할 때마다 샌다.
                    // Releasing invalidates the handle, so read its exception first.
                    string errorMsg = op.OperationException != null ? op.OperationException.Message : $"Addressables failed to load '{assetName}'.";
                    Addressables.Release(op);
                    loadAssetCallbacks?.LoadAssetFailureCallback?.Invoke(assetName, LoadResourceStatus.NotExist, errorMsg, userData);
                }
            };
        }

        public void UnloadAsset(object asset)
        {
            if (asset == null)
                return;

            if (!mAssetNames.TryGetValue(asset, out string assetName))
                return;

            if (!mAssetHandles.TryGetValue(assetName, out List<AsyncOperationHandle> handles) || handles.Count == 0)
            {
                mAssetHandles.Remove(assetName);
                mAssetNames.Remove(asset);
                return;
            }

            // 로드 1회당 정확히 1개씩 반납한다. 남은 참조가 있으면 에셋은 살아 있다.
            int last = handles.Count - 1;
            Addressables.Release(handles[last]);
            handles.RemoveAt(last);

            if (handles.Count == 0)
            {
                mAssetHandles.Remove(assetName);
                mAssetNames.Remove(asset);
            }
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
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.NotExist, "Scene asset name is invalid.", userData);
                return;
            }

            float startTime = Time.time;
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneAssetName, LoadSceneMode.Additive);

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    mSceneHandles[sceneAssetName] = op;
                    loadSceneCallbacks?.LoadSceneSuccessCallback?.Invoke(sceneAssetName, Time.time - startTime, userData);
                }
                else
                {
                    string errorMsg = op.OperationException != null ? op.OperationException.Message : $"Addressables failed to load scene '{sceneAssetName}'.";
                    Addressables.Release(op);
                    loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.NotExist, errorMsg, userData);
                }
            };
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks) =>
            UnloadScene(sceneAssetName, unloadSceneCallbacks, null);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (!mSceneHandles.TryGetValue(sceneAssetName, out AsyncOperationHandle<SceneInstance> handle))
            {
                unloadSceneCallbacks?.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
                return;
            }

            AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(handle);
            unloadHandle.Completed += op =>
            {
                mSceneHandles.Remove(sceneAssetName);
                if (op.Status == AsyncOperationStatus.Succeeded)
                    unloadSceneCallbacks?.UnloadSceneSuccessCallback?.Invoke(sceneAssetName, userData);
                else
                    unloadSceneCallbacks?.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
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

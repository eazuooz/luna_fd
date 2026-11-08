using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    public class ResourcesManager : MonoBehaviour, IManager
    {
        private readonly Dictionary<GameObject, UnityEngine.Object> assets = new Dictionary<GameObject, UnityEngine.Object>();
        private ResourceComponent Resource => GameEntry.GetComponent<ResourceComponent>();
        public void Load(AssetReference reference, Transform root, Action<GameObject> success, Action failure)
        {
            if (reference == null || !reference.RuntimeKeyIsValid()) { failure?.Invoke(); return; }
            Resource.LoadAsset(reference.RuntimeKey.ToString(), typeof(GameObject), new LoadAssetCallbacks(
                (name, asset, duration, data) =>
                {
                    var prefab = asset as GameObject;
                    if (prefab == null) { Resource.UnloadAsset(asset); failure?.Invoke(); return; }
                    var clone = Instantiate(prefab, root, false);
                    assets[clone] = prefab;
                    success?.Invoke(clone);
                },
                (name, status, message, data) => { Debug.LogError("Luna asset load failed: " + name + ": " + message); failure?.Invoke(); }));
        }
        public IEnumerator LoadAsset(AssetReference reference, Transform root, Action<GameObject> success, Action failure)
        {
            bool done = false;
            Load(reference, root, go => { done = true; success?.Invoke(go); }, () => { done = true; failure?.Invoke(); });
            while (!done) yield return null;
        }
        public void UnLoad(AssetReference reference, GameObject instance, bool isCaching)
        {
            if (instance == null) return;
            if (assets.TryGetValue(instance, out var asset)) { assets.Remove(instance); Resource.UnloadAsset(asset); }
            Destroy(instance);
        }
        public void Load(AssetBundle bundle, GameObject reference, Transform root, Action<GameObject> success, Action failure)
        {
            StartCoroutine(LoadAsset(bundle, reference, root, success, failure));
        }
        public IEnumerator LoadAsset(AssetBundle bundle, GameObject reference, Transform root, Action<GameObject> success, Action failure)
        {
            if (bundle == null || reference == null) { failure?.Invoke(); yield break; }
            var request = bundle.LoadAssetAsync<GameObject>(reference.name);
            yield return request;
            var prefab = request.asset as GameObject;
            if (prefab != null) success?.Invoke(Instantiate(prefab, root, false)); else failure?.Invoke();
        }
        public void UnLoad(AssetBundle bundle, GameObject instance, bool isCaching) { if (instance != null) Destroy(instance); }
        public void ManagerInitialize() { }
        public void ManagerUpdate() { }
        public void ManagerLoop() { }
        public void ManagerClear()
        {
            if (Resource != null) foreach (var asset in assets.Values) Resource.UnloadAsset(asset);
            assets.Clear();
        }
    }
}

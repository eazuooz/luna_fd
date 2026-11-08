using UnityEngine;

namespace EverydayDevup.Framework
{
    /// <summary>Legacy call surface; Game Framework Entity owns registration, pooling and hide.</summary>
    public class PoolManager : Singleton<PoolManager>
    {
        public bool mLogStatus;
        public Transform mRoot;
        public void warmPool(GameObject prefab, int size) => LunaEntities.Warm(prefab, size);
        public GameObject spawnObject(GameObject prefab) => spawnObject(prefab, Vector3.zero, Quaternion.identity);
        public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
            => LunaEntities.Spawn(prefab, position, rotation, mRoot, true);
        public void releaseObject(GameObject clone)
        {
            if (clone == null) return;
            if (LunaEntities.IsSpawned(clone)) LunaEntities.Hide(clone);
            else if (clone.GetComponent<LunaEntityLogic>() == null) clone.SetActive(false);
        }
        public void ResetPoolManager() => LunaEntities.ClearScene();
        public void releaseObjects() => LunaEntities.ClearScene();
        public void PrintStatus() => Debug.Log("Luna active Game Framework entities: " + LunaEntities.ActiveCount);
        public static bool IsSpawned(GameObject clone) => LunaEntities.IsSpawned(clone);
        public static void WarmPool(GameObject prefab, int size) => Instance.warmPool(prefab, size);
        public static GameObject SpawnObject(GameObject prefab) => Instance.spawnObject(prefab);
        public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation) => Instance.spawnObject(prefab, position, rotation);
        public static void ReleaseObject(GameObject clone) { if (Instance != null) Instance.releaseObject(clone); }
    }
}

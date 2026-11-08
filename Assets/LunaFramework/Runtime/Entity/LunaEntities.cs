using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Entity;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    public static class LunaEntities
    {
        private const string ReusableGroup = "LunaGameplay";
        private const string TransientGroup = "LunaTransient";
        private static EntityComponent entities;
        private static ResourceManager resources;
        private static readonly Dictionary<GameObject, int> live = new Dictionary<GameObject, int>();
        private static readonly HashSet<string> residentKeys = new HashSet<string>();
        private static int serial;
        public static bool IsReady => entities != null && resources != null;
        public static int ActiveCount => live.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            entities = null; resources = null; serial = 0; live.Clear(); residentKeys.Clear();
        }
        internal static void Initialize(Game game)
        {
            entities = GameEntry.GetComponent<EntityComponent>();
            resources = (ResourceManager)GameEntry.GetComponent<ResourceComponent>().ResourceManager;
            var helperObject = new GameObject("Luna Entity Helper");
            helperObject.transform.SetParent(game.transform, false);
            var helper = helperObject.AddComponent<LunaEntityHelper>();
            helper.Initialize(resources);
            GameFrameworkEntry.GetModule<IEntityManager>().SetEntityHelper(helper);
            entities.AddEntityGroup(ReusableGroup, 60f, 512, float.MaxValue, 0);
            entities.AddEntityGroup(TransientGroup, 0f, 0, 0f, 0);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool reusable = false)
        {
            if (!IsReady) throw new InvalidOperationException("Luna Entity service has not initialized.");
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            int id = ++serial;
            string key = reusable ? "luna-prefab://" + prefab.GetEntityId() : "luna-transient://" + id;
            Register(key, prefab, false, reusable);
            var data = new LunaEntitySpawnData { Position = position, Rotation = rotation, Parent = parent };
            entities.ShowEntity(id, typeof(LunaEntityLogic), key, reusable ? ReusableGroup : TransientGroup, data);
            if (data.Result == null) throw new InvalidOperationException("Game Framework did not show Luna entity: " + prefab.name);
            return data.Result;
        }
        public static GameObject Spawn(GameObject prefab) => Spawn(prefab, prefab.transform.position, prefab.transform.rotation);
        public static void Adopt(GameObject instance)
        {
            if (instance == null || IsSpawned(instance)) return;
            if (!IsReady) throw new InvalidOperationException("Luna Entity service has not initialized.");
            int id = ++serial;
            string key = "luna-scene://" + id;
            Register(key, instance, true, false);
            entities.ShowEntity(id, typeof(LunaEntityLogic), key, TransientGroup, new LunaEntitySpawnData { Adopted = true });
        }
        private static void Register(string key, GameObject prefab, bool adopted, bool reusable)
        {
            if (!residentKeys.Add(key)) return;
            resources.RegisterResidentAsset(key, new LunaEntityAsset { Key = key, Prefab = prefab, Adopted = adopted, Reusable = reusable });
        }
        public static void Warm(GameObject prefab, int capacity)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            Register("luna-prefab://" + prefab.GetEntityId(), prefab, false, true);
            // GF creates instances on demand; initialization must not run during warm registration.
            var group = entities.GetEntityGroup(ReusableGroup);
            group.InstanceCapacity = Math.Max(group.InstanceCapacity, capacity);
        }
        internal static void UnregisterResident(string key) { residentKeys.Remove(key); resources.UnregisterResidentAsset(key); }
        public static bool IsSpawned(GameObject instance) => instance != null && live.ContainsKey(instance);
        internal static void RegisterShown(GameObject instance, int id) { live[instance] = id; }
        internal static void UnregisterShown(GameObject instance) { if (!ReferenceEquals(instance, null)) live.Remove(instance); }
        public static void Hide(GameObject instance)
        {
            if (instance == null) return;
            if (live.TryGetValue(instance, out int id))
            {
                live.Remove(instance);
                entities.HideEntity(id);
            }
            else if (instance.GetComponent<LunaEntityLogic>() == null) UnityEngine.Object.Destroy(instance);
        }
        public static void ClearScene()
        {
            if (!IsReady) return;
            foreach (var instance in new List<GameObject>(live.Keys)) Hide(instance);
            live.Clear();
        }
    }
}

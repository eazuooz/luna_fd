using System;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
	public class PoolManager : Singleton<PoolManager>
	{
		public bool mLogStatus;
		public Transform mRoot;

		private Dictionary<GameObject, ObjectPool<GameObject>> mPrefabLookup;
		private Dictionary<GameObject, ObjectPool<GameObject>> mInstanceLookup;

		private bool dirty = false;

        public void ResetPoolManager()
        {
			mPrefabLookup.Clear();
			mInstanceLookup.Clear();
		}

        void Awake()
		{
			mPrefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
			mInstanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		}

		void Update()
		{
			if (mLogStatus && dirty)
			{
				PrintStatus();
				dirty = false;
			}
		}

		public void warmPool(GameObject prefab, int size)
		{
			if (mPrefabLookup.ContainsKey(prefab))
			{
				//throw new Exception("Pool for prefab " + prefab.name + " has already been created");

				return;
			}
			var pool = new ObjectPool<GameObject>(() => { return InstantiatePrefab(prefab); }, size);
			mPrefabLookup[prefab] = pool;

			dirty = true;
		}

		public GameObject spawnObject(GameObject prefab)
		{
			return spawnObject(prefab, Vector3.zero, Quaternion.identity);
		}

		public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			if (!mPrefabLookup.ContainsKey(prefab))
			{
				WarmPool(prefab, 1);
			}

			var pool = mPrefabLookup[prefab];

			var clone = pool.GetItem();

			clone.transform.position = position;
			clone.transform.rotation = rotation;
			clone.SetActive(true);

			mInstanceLookup.Add(clone, pool);
			dirty = true;
			return clone;
		}

		public void releaseObject(GameObject clone)
		{
			clone.SetActive(false);

			if (mInstanceLookup.ContainsKey(clone))
			{
				mInstanceLookup[clone].ReleaseItem(clone);
				mInstanceLookup.Remove(clone);
				dirty = true;
			}
			else
			{
				Debug.LogWarning("No pool contains the object: " + clone.name);
			}
		}

		public void releaseObjects()
        {
			foreach( var prefab in mPrefabLookup )
            {
				prefab.Value.ReleaseItems();
			}
			mPrefabLookup.Clear();

			foreach (var inst in mInstanceLookup)
			{
				inst.Value.ReleaseItems();
			}
			mInstanceLookup.Clear();
		}

		private GameObject InstantiatePrefab(GameObject prefab)
		{
			var go = Instantiate(prefab) as GameObject;

			if (go == null)
				return null;

			if (mRoot != null) go.transform.parent = mRoot;
			return go;
		}

		public void PrintStatus()
		{
			foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in mPrefabLookup)
			{
				Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
			}
		}

		#region Static API

		public static void WarmPool(GameObject prefab, int size)
		{
			Instance.warmPool(prefab, size);
		}

		public static GameObject SpawnObject(GameObject prefab)
		{
			return Instance.spawnObject(prefab);
		}

		public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			return Instance.spawnObject(prefab, position, rotation);
		}

		public static void ReleaseObject(GameObject clone)
		{
			Instance.releaseObject(clone);
		}

		#endregion
	}
}


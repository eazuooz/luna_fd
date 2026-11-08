//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.ObjectPool;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Object Pool")]
    public sealed class ObjectPoolComponent : GameFrameworkComponent
    {
        private IObjectPoolManager mObjectPoolManager = null;

        public int Count
        {
            get
            {
                return mObjectPoolManager.Count;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mObjectPoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            if (mObjectPoolManager == null)
            {
                Log.Fatal("Object pool manager is invalid.");
                return;
            }
        }

        private void Start()
        {
        }

        public bool HasObjectPool<T>() where T : ObjectBase
        {
            return mObjectPoolManager.HasObjectPool<T>();
        }

        public bool HasObjectPool(Type objectType)
        {
            return mObjectPoolManager.HasObjectPool(objectType);
        }

        public bool HasObjectPool<T>(string name) where T : ObjectBase
        {
            return mObjectPoolManager.HasObjectPool<T>(name);
        }

        public bool HasObjectPool(Type objectType, string name)
        {
            return mObjectPoolManager.HasObjectPool(objectType, name);
        }

        public bool HasObjectPool(Predicate<ObjectPoolBase> condition)
        {
            return mObjectPoolManager.HasObjectPool(condition);
        }

        public IObjectPool<T> GetObjectPool<T>() where T : ObjectBase
        {
            return mObjectPoolManager.GetObjectPool<T>();
        }

        public ObjectPoolBase GetObjectPool(Type objectType)
        {
            return mObjectPoolManager.GetObjectPool(objectType);
        }

        public IObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase
        {
            return mObjectPoolManager.GetObjectPool<T>(name);
        }

        public ObjectPoolBase GetObjectPool(Type objectType, string name)
        {
            return mObjectPoolManager.GetObjectPool(objectType, name);
        }

        public ObjectPoolBase GetObjectPool(Predicate<ObjectPoolBase> condition)
        {
            return mObjectPoolManager.GetObjectPool(condition);
        }

        public ObjectPoolBase[] GetObjectPools(Predicate<ObjectPoolBase> condition)
        {
            return mObjectPoolManager.GetObjectPools(condition);
        }

        public void GetObjectPools(Predicate<ObjectPoolBase> condition, List<ObjectPoolBase> results)
        {
            mObjectPoolManager.GetObjectPools(condition, results);
        }

        public ObjectPoolBase[] GetAllObjectPools()
        {
            return mObjectPoolManager.GetAllObjectPools();
        }

        public void GetAllObjectPools(List<ObjectPoolBase> results)
        {
            mObjectPoolManager.GetAllObjectPools(results);
        }

        public ObjectPoolBase[] GetAllObjectPools(bool sort)
        {
            return mObjectPoolManager.GetAllObjectPools(sort);
        }

        public void GetAllObjectPools(bool sort, List<ObjectPoolBase> results)
        {
            mObjectPoolManager.GetAllObjectPools(sort, results);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>();
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(expireTime);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, expireTime);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, expireTime);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, expireTime);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity, expireTime);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity, expireTime);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(expireTime, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, expireTime, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity, expireTime);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity, expireTime);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, expireTime, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, expireTime, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity, expireTime, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity, expireTime, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity, expireTime, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity, expireTime, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, autoReleaseInterval, capacity, expireTime, priority);
        }

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, autoReleaseInterval, capacity, expireTime, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>() where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>();
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(expireTime);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, expireTime);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, expireTime);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, expireTime);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity, expireTime);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity, expireTime);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(expireTime, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, expireTime, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity, expireTime);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity, expireTime);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, expireTime, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, expireTime, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity, expireTime, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity, expireTime, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity, expireTime, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity, expireTime, priority);
        }

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, autoReleaseInterval, capacity, expireTime, priority);
        }

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            return mObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, autoReleaseInterval, capacity, expireTime, priority);
        }

        public bool DestroyObjectPool<T>() where T : ObjectBase
        {
            return mObjectPoolManager.DestroyObjectPool<T>();
        }

        public bool DestroyObjectPool(Type objectType)
        {
            return mObjectPoolManager.DestroyObjectPool(objectType);
        }

        public bool DestroyObjectPool<T>(string name) where T : ObjectBase
        {
            return mObjectPoolManager.DestroyObjectPool<T>(name);
        }

        public bool DestroyObjectPool(Type objectType, string name)
        {
            return mObjectPoolManager.DestroyObjectPool(objectType, name);
        }

        public bool DestroyObjectPool<T>(IObjectPool<T> objectPool) where T : ObjectBase
        {
            return mObjectPoolManager.DestroyObjectPool(objectPool);
        }

        public bool DestroyObjectPool(ObjectPoolBase objectPool)
        {
            return mObjectPoolManager.DestroyObjectPool(objectPool);
        }

        public void Release()
        {
            Log.Info("Object pool release...");
            mObjectPoolManager.Release();
        }

        public void ReleaseAllUnused()
        {
            Log.Info("Object pool release all unused...");
            mObjectPoolManager.ReleaseAllUnused();
        }
    }
}

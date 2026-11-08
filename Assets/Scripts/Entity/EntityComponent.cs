//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Entity;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Entity")]
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IEntityManager mEntityManager = null;
        private EventComponent mEventComponent = null;

        private readonly List<IEntity> mInternalEntityResults = new List<IEntity>();

        [SerializeField]
        private bool mEnableShowEntityUpdateEvent = false;

        [SerializeField]
        private bool mEnableShowEntityDependencyAssetEvent = false;

        [SerializeField]
        private Transform mInstanceRoot = null;

        [SerializeField]
        private string mEntityHelperTypeName = "UnityGameFramework.Runtime.DefaultEntityHelper";

        [SerializeField]
        private EntityHelperBase mCustomEntityHelper = null;

        [SerializeField]
        private string mEntityGroupHelperTypeName = "UnityGameFramework.Runtime.DefaultEntityGroupHelper";

        [SerializeField]
        private EntityGroupHelperBase mCustomEntityGroupHelper = null;

        [SerializeField]
        private EntityGroup[] mEntityGroups = null;

        public int EntityCount
        {
            get
            {
                return mEntityManager.EntityCount;
            }
        }

        public int EntityGroupCount
        {
            get
            {
                return mEntityManager.EntityGroupCount;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mEntityManager = GameFrameworkEntry.GetModule<IEntityManager>();
            if (mEntityManager == null)
            {
                Log.Fatal("Entity manager is invalid.");
                return;
            }

            mEntityManager.ShowEntitySuccess += OnShowEntitySuccess;
            mEntityManager.ShowEntityFailure += OnShowEntityFailure;

            if (mEnableShowEntityUpdateEvent)
            {
                mEntityManager.ShowEntityUpdate += OnShowEntityUpdate;
            }

            if (mEnableShowEntityDependencyAssetEvent)
            {
                mEntityManager.ShowEntityDependencyAsset += OnShowEntityDependencyAsset;
            }

            mEntityManager.HideEntityComplete += OnHideEntityComplete;
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

            mEntityManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);

            mEntityManager.SetObjectPoolManager(GameFrameworkEntry.GetModule<IObjectPoolManager>());

            EntityHelperBase entityHelper = Helper.CreateHelper(mEntityHelperTypeName, mCustomEntityHelper);
            if (entityHelper == null)
            {
                Log.Error("Can not create entity helper.");
                return;
            }

            entityHelper.name = "Entity Helper";
            Transform transform = entityHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mEntityManager.SetEntityHelper(entityHelper);

            if (mInstanceRoot == null)
            {
                mInstanceRoot = new GameObject("Entity Instances").transform;
                mInstanceRoot.SetParent(gameObject.transform);
                mInstanceRoot.localScale = Vector3.one;
            }

            for (int i = 0; i < mEntityGroups.Length; i++)
            {
                if (!AddEntityGroup(mEntityGroups[i].Name, mEntityGroups[i].InstanceAutoReleaseInterval, mEntityGroups[i].InstanceCapacity, mEntityGroups[i].InstanceExpireTime, mEntityGroups[i].InstancePriority))
                {
                    Log.Warning("Add entity group '{0}' failure.", mEntityGroups[i].Name);
                    continue;
                }
            }
        }

        public bool HasEntityGroup(string entityGroupName)
        {
            return mEntityManager.HasEntityGroup(entityGroupName);
        }

        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            return mEntityManager.GetEntityGroup(entityGroupName);
        }

        public IEntityGroup[] GetAllEntityGroups()
        {
            return mEntityManager.GetAllEntityGroups();
        }

        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            mEntityManager.GetAllEntityGroups(results);
        }

        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority)
        {
            if (mEntityManager.HasEntityGroup(entityGroupName))
            {
                return false;
            }

            EntityGroupHelperBase entityGroupHelper = Helper.CreateHelper(mEntityGroupHelperTypeName, mCustomEntityGroupHelper, EntityGroupCount);
            if (entityGroupHelper == null)
            {
                Log.Error("Can not create entity group helper.");
                return false;
            }

            entityGroupHelper.name = Utility.Text.Format("Entity Group - {0}", entityGroupName);
            Transform transform = entityGroupHelper.transform;
            transform.SetParent(mInstanceRoot);
            transform.localScale = Vector3.one;

            return mEntityManager.AddEntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime, instancePriority, entityGroupHelper);
        }

        public bool HasEntity(int entityId)
        {
            return mEntityManager.HasEntity(entityId);
        }

        public bool HasEntity(string entityAssetName)
        {
            return mEntityManager.HasEntity(entityAssetName);
        }

        public Entity GetEntity(int entityId)
        {
            return (Entity)mEntityManager.GetEntity(entityId);
        }

        public Entity GetEntity(string entityAssetName)
        {
            return (Entity)mEntityManager.GetEntity(entityAssetName);
        }

        public Entity[] GetEntities(string entityAssetName)
        {
            IEntity[] entities = mEntityManager.GetEntities(entityAssetName);
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        public void GetEntities(string entityAssetName, List<Entity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            mEntityManager.GetEntities(entityAssetName, mInternalEntityResults);
            foreach (IEntity entity in mInternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        public Entity[] GetAllLoadedEntities()
        {
            IEntity[] entities = mEntityManager.GetAllLoadedEntities();
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        public void GetAllLoadedEntities(List<Entity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            mEntityManager.GetAllLoadedEntities(mInternalEntityResults);
            foreach (IEntity entity in mInternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        public int[] GetAllLoadingEntityIds()
        {
            return mEntityManager.GetAllLoadingEntityIds();
        }

        public void GetAllLoadingEntityIds(List<int> results)
        {
            mEntityManager.GetAllLoadingEntityIds(results);
        }

        public bool IsLoadingEntity(int entityId)
        {
            return mEntityManager.IsLoadingEntity(entityId);
        }

        public bool IsValidEntity(Entity entity)
        {
            return mEntityManager.IsValidEntity(entity);
        }

        public void ShowEntity<T>(int entityId, string entityAssetName, string entityGroupName) where T : EntityLogic
        {
            ShowEntity(entityId, typeof(T), entityAssetName, entityGroupName, DefaultPriority, null);
        }

        public void ShowEntity(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName)
        {
            ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, DefaultPriority, null);
        }

        public void ShowEntity<T>(int entityId, string entityAssetName, string entityGroupName, int priority) where T : EntityLogic
        {
            ShowEntity(entityId, typeof(T), entityAssetName, entityGroupName, priority, null);
        }

        public void ShowEntity(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority)
        {
            ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, null);
        }

        public void ShowEntity<T>(int entityId, string entityAssetName, string entityGroupName, object userData) where T : EntityLogic
        {
            ShowEntity(entityId, typeof(T), entityAssetName, entityGroupName, DefaultPriority, userData);
        }

        public void ShowEntity(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, object userData)
        {
            ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, DefaultPriority, userData);
        }

        public void ShowEntity<T>(int entityId, string entityAssetName, string entityGroupName, int priority, object userData) where T : EntityLogic
        {
            ShowEntity(entityId, typeof(T), entityAssetName, entityGroupName, priority, userData);
        }

        public void ShowEntity(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            if (entityLogicType == null)
            {
                Log.Error("Entity type is invalid.");
                return;
            }

            mEntityManager.ShowEntity(entityId, entityAssetName, entityGroupName, priority, ShowEntityInfo.Create(entityLogicType, userData));
        }

        public void HideEntity(int entityId)
        {
            mEntityManager.HideEntity(entityId);
        }

        public void HideEntity(int entityId, object userData)
        {
            mEntityManager.HideEntity(entityId, userData);
        }

        public void HideEntity(Entity entity)
        {
            mEntityManager.HideEntity(entity);
        }

        public void HideEntity(Entity entity, object userData)
        {
            mEntityManager.HideEntity(entity, userData);
        }

        public void HideAllLoadedEntities()
        {
            mEntityManager.HideAllLoadedEntities();
        }

        public void HideAllLoadedEntities(object userData)
        {
            mEntityManager.HideAllLoadedEntities(userData);
        }

        public void HideAllLoadingEntities()
        {
            mEntityManager.HideAllLoadingEntities();
        }

        public Entity GetParentEntity(int childEntityId)
        {
            return (Entity)mEntityManager.GetParentEntity(childEntityId);
        }

        public Entity GetParentEntity(Entity childEntity)
        {
            return (Entity)mEntityManager.GetParentEntity(childEntity);
        }

        public int GetChildEntityCount(int parentEntityId)
        {
            return mEntityManager.GetChildEntityCount(parentEntityId);
        }

        public Entity GetChildEntity(int parentEntityId)
        {
            return (Entity)mEntityManager.GetChildEntity(parentEntityId);
        }

        public Entity GetChildEntity(IEntity parentEntity)
        {
            return (Entity)mEntityManager.GetChildEntity(parentEntity);
        }

        public Entity[] GetChildEntities(int parentEntityId)
        {
            IEntity[] entities = mEntityManager.GetChildEntities(parentEntityId);
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        public void GetChildEntities(int parentEntityId, List<IEntity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            mEntityManager.GetChildEntities(parentEntityId, mInternalEntityResults);
            foreach (IEntity entity in mInternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        public Entity[] GetChildEntities(Entity parentEntity)
        {
            IEntity[] entities = mEntityManager.GetChildEntities(parentEntity);
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        public void GetChildEntities(IEntity parentEntity, List<IEntity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            mEntityManager.GetChildEntities(parentEntity, mInternalEntityResults);
            foreach (IEntity entity in mInternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        public void AttachEntity(int childEntityId, int parentEntityId)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), string.Empty, null);
        }

        public void AttachEntity(int childEntityId, Entity parentEntity)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, string.Empty, null);
        }

        public void AttachEntity(Entity childEntity, int parentEntityId)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), string.Empty, null);
        }

        public void AttachEntity(Entity childEntity, Entity parentEntity)
        {
            AttachEntity(childEntity, parentEntity, string.Empty, null);
        }

        public void AttachEntity(int childEntityId, int parentEntityId, string parentTransformPath)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransformPath, null);
        }

        public void AttachEntity(int childEntityId, Entity parentEntity, string parentTransformPath)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransformPath, null);
        }

        public void AttachEntity(Entity childEntity, int parentEntityId, string parentTransformPath)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransformPath, null);
        }

        public void AttachEntity(Entity childEntity, Entity parentEntity, string parentTransformPath)
        {
            AttachEntity(childEntity, parentEntity, parentTransformPath, null);
        }

        public void AttachEntity(int childEntityId, int parentEntityId, Transform parentTransform)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransform, null);
        }

        public void AttachEntity(int childEntityId, Entity parentEntity, Transform parentTransform)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransform, null);
        }

        public void AttachEntity(Entity childEntity, int parentEntityId, Transform parentTransform)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransform, null);
        }

        public void AttachEntity(Entity childEntity, Entity parentEntity, Transform parentTransform)
        {
            AttachEntity(childEntity, parentEntity, parentTransform, null);
        }

        public void AttachEntity(int childEntityId, int parentEntityId, object userData)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), string.Empty, userData);
        }

        public void AttachEntity(int childEntityId, Entity parentEntity, object userData)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, string.Empty, userData);
        }

        public void AttachEntity(Entity childEntity, int parentEntityId, object userData)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), string.Empty, userData);
        }

        public void AttachEntity(Entity childEntity, Entity parentEntity, object userData)
        {
            AttachEntity(childEntity, parentEntity, string.Empty, userData);
        }

        public void AttachEntity(int childEntityId, int parentEntityId, string parentTransformPath, object userData)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransformPath, userData);
        }

        public void AttachEntity(int childEntityId, Entity parentEntity, string parentTransformPath, object userData)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransformPath, userData);
        }

        public void AttachEntity(Entity childEntity, int parentEntityId, string parentTransformPath, object userData)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransformPath, userData);
        }

        public void AttachEntity(Entity childEntity, Entity parentEntity, string parentTransformPath, object userData)
        {
            if (childEntity == null)
            {
                Log.Warning("Child entity is invalid.");
                return;
            }

            if (parentEntity == null)
            {
                Log.Warning("Parent entity is invalid.");
                return;
            }

            Transform parentTransform = null;
            if (string.IsNullOrEmpty(parentTransformPath))
            {
                parentTransform = parentEntity.Logic.CachedTransform;
            }
            else
            {
                parentTransform = parentEntity.Logic.CachedTransform.Find(parentTransformPath);
                if (parentTransform == null)
                {
                    Log.Warning("Can not find transform path '{0}' from parent entity '{1}'.", parentTransformPath, parentEntity.Logic.Name);
                    parentTransform = parentEntity.Logic.CachedTransform;
                }
            }

            AttachEntity(childEntity, parentEntity, parentTransform, userData);
        }

        public void AttachEntity(int childEntityId, int parentEntityId, Transform parentTransform, object userData)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransform, userData);
        }

        public void AttachEntity(int childEntityId, Entity parentEntity, Transform parentTransform, object userData)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransform, userData);
        }

        public void AttachEntity(Entity childEntity, int parentEntityId, Transform parentTransform, object userData)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransform, userData);
        }

        public void AttachEntity(Entity childEntity, Entity parentEntity, Transform parentTransform, object userData)
        {
            if (childEntity == null)
            {
                Log.Warning("Child entity is invalid.");
                return;
            }

            if (parentEntity == null)
            {
                Log.Warning("Parent entity is invalid.");
                return;
            }

            if (parentTransform == null)
            {
                parentTransform = parentEntity.Logic.CachedTransform;
            }

            mEntityManager.AttachEntity(childEntity, parentEntity, AttachEntityInfo.Create(parentTransform, userData));
        }

        public void DetachEntity(int childEntityId)
        {
            mEntityManager.DetachEntity(childEntityId);
        }

        public void DetachEntity(int childEntityId, object userData)
        {
            mEntityManager.DetachEntity(childEntityId, userData);
        }

        public void DetachEntity(Entity childEntity)
        {
            mEntityManager.DetachEntity(childEntity);
        }

        public void DetachEntity(Entity childEntity, object userData)
        {
            mEntityManager.DetachEntity(childEntity, userData);
        }

        public void DetachChildEntities(int parentEntityId)
        {
            mEntityManager.DetachChildEntities(parentEntityId);
        }

        public void DetachChildEntities(int parentEntityId, object userData)
        {
            mEntityManager.DetachChildEntities(parentEntityId, userData);
        }

        public void DetachChildEntities(Entity parentEntity)
        {
            mEntityManager.DetachChildEntities(parentEntity);
        }

        public void DetachChildEntities(Entity parentEntity, object userData)
        {
            mEntityManager.DetachChildEntities(parentEntity, userData);
        }

        public void SetEntityInstanceLocked(Entity entity, bool locked)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            IEntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return;
            }

            entityGroup.SetEntityInstanceLocked(entity.gameObject, locked);
        }

        public void SetInstancePriority(Entity entity, int priority)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            IEntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return;
            }

            entityGroup.SetEntityInstancePriority(entity.gameObject, priority);
        }

        private void OnShowEntitySuccess(object sender, GameFramework.Entity.ShowEntitySuccessEventArgs e)
        {
            mEventComponent.Fire(this, ShowEntitySuccessEventArgs.Create(e));
        }

        private void OnShowEntityFailure(object sender, GameFramework.Entity.ShowEntityFailureEventArgs e)
        {
            Log.Warning("Show entity failure, entity id '{0}', asset name '{1}', entity group name '{2}', error message '{3}'.", e.EntityId, e.EntityAssetName, e.EntityGroupName, e.ErrorMessage);
            mEventComponent.Fire(this, ShowEntityFailureEventArgs.Create(e));
        }

        private void OnShowEntityUpdate(object sender, GameFramework.Entity.ShowEntityUpdateEventArgs e)
        {
            mEventComponent.Fire(this, ShowEntityUpdateEventArgs.Create(e));
        }

        private void OnShowEntityDependencyAsset(object sender, GameFramework.Entity.ShowEntityDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, ShowEntityDependencyAssetEventArgs.Create(e));
        }

        private void OnHideEntityComplete(object sender, GameFramework.Entity.HideEntityCompleteEventArgs e)
        {
            mEventComponent.Fire(this, HideEntityCompleteEventArgs.Create(e));
        }
    }
}

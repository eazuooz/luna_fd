using GameFramework.Entity;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    internal sealed class LunaEntityAsset
    {
        internal string Key;
        internal GameObject Prefab;
        internal bool Adopted;
        internal bool Reusable;
    }
    internal sealed class LunaEntitySpawnData
    {
        internal Vector3 Position;
        internal Quaternion Rotation;
        internal Transform Parent;
        internal bool Adopted;
        internal GameObject Result;
    }
    public sealed class LunaEntityHelper : EntityHelperBase
    {
        private Transform staging;
        private ResourceManager resource;
        internal void Initialize(ResourceManager manager)
        {
            resource = manager;
            var stage = new GameObject("Inactive Entity Staging");
            stage.SetActive(false);
            stage.transform.SetParent(transform, false);
            staging = stage.transform;
        }
        public override object InstantiateEntity(object entityAsset)
        {
            var asset = (LunaEntityAsset)entityAsset;
            if (asset.Adopted) return asset.Prefab;
            var instance = Instantiate(asset.Prefab, staging);
            instance.SetActive(false);
            return instance;
        }
        public override IEntity CreateEntity(object entityInstance, IEntityGroup group, object userData)
        {
            var instance = (GameObject)entityInstance;
            var entity = instance.GetComponent<Entity>();
            return entity != null ? entity : instance.AddComponent<Entity>();
        }
        public override void ReleaseEntity(object entityAsset, object entityInstance)
        {
            var asset = (LunaEntityAsset)entityAsset;
            var instance = entityInstance as GameObject;
            if (instance != null)
            {
                instance.GetComponent<LunaEntityLogic>()?.ReleaseModules();
                Destroy(instance);
            }
            if (!asset.Reusable) LunaEntities.UnregisterResident(asset.Key);
        }
    }
}

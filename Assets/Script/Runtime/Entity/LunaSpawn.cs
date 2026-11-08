using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    /// <summary>Routes asynchronous native spawn results. It never runs or forwards game logic.</summary>
    public static class LunaSpawn
    {
        private sealed class Request
        {
            public LunaEntityData Data;
            public int Round;
            public bool Cancelled;
            public Action<EntityLogicBase> Shown;
            public Action<string> Failed;
        }
        private static readonly Dictionary<int, Request> pending = new Dictionary<int, Request>();
        private static readonly Dictionary<GameObject, LunaEntityCatalog.Entry> catalog = new Dictionary<GameObject, LunaEntityCatalog.Entry>();
        private static EntityComponent entities;
        private static EventComponent events;
        public static int PendingCount => pending.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            // Native module shutdown returns claimed data. Pending loads never handed
            // to an entity must also be returned when entering play without domain reload.
            foreach (var pair in pending) ReleaseUnclaimed(pair.Key, pair.Value);
            pending.Clear();
            catalog.Clear();
            entities = null;
            events = null;
        }
        internal static void NotifyHidden(int id)
        {
            // A logic can retire during OnShow, before the queued success event is dispatched.
            if (!pending.TryGetValue(id, out var request)) return;
            pending.Remove(id);
            if (!request.Cancelled)
            {
                try { request.Failed?.Invoke("Entity retired before its spawn completion was observed."); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        public static void Initialize()
        {
            if (entities != null) return;
            entities = GameEntry.GetComponent<EntityComponent>();
            events = GameEntry.GetComponent<EventComponent>();
            if (entities == null || events == null) throw new InvalidOperationException("Native Entity/Event components are missing.");
            var source = Resources.Load<LunaEntityCatalog>("Luna/EntityCatalog");
            if (source == null) throw new InvalidOperationException("Luna entity catalog is missing.");
            LunaResourcesProvider.Register(source);
            catalog.Clear();
            foreach (var entry in source.Entries)
            {
                if (entry.Prefab == null || string.IsNullOrEmpty(entry.Address))
                    throw new InvalidOperationException("Entity catalog contains an empty asset.");
                catalog.Add(entry.Prefab, entry);
                if (!entities.HasEntityGroup(entry.Group)) entities.AddEntityGroup(entry.Group, 60f, 256, 60f, 0);
            }
            events.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShown);
            events.Subscribe(ShowEntityFailureEventArgs.EventId, OnFailed);
        }

        public static int Show<T>(GameObject prefab, LunaEntityData data, Action<T> shown = null, Action<string> failed = null) where T : EntityLogicBase
        {
            if (data == null || data.Id <= 0 || data.OwnedByEntity) throw new ArgumentException("Spawn needs newly acquired, fully configured EntityData.");
            int id = data.Id;
            if (entities == null || prefab == null || !catalog.TryGetValue(prefab, out var entry))
            {
                ReferencePool.Release(data);
                failed?.Invoke("Entity prefab is not registered: " + (prefab != null ? prefab.name : "null"));
                return id;
            }
            Type logicType = Type.GetType(entry.LogicType + ", Assembly-CSharp") ?? Type.GetType(entry.LogicType);
            if (logicType == null || !typeof(T).IsAssignableFrom(logicType))
            {
                ReferencePool.Release(data);
                throw new InvalidOperationException("Entity catalog logic does not match request: " + entry.LogicType);
            }
            var request = new Request { Data = data, Round = data.RoundId, Shown = logic => shown?.Invoke((T)logic), Failed = failed };
            pending.Add(id, request);
            try { entities.ShowEntity(id, logicType, entry.Address, entry.Group, data); }
            catch
            {
                pending.Remove(id);
                ReleaseUnclaimed(id, request);
                throw;
            }
            return id;
        }

        public static T Find<T>(int id) where T : EntityLogicBase
        {
            if (id <= 0 || entities == null || !entities.HasEntity(id)) return null;
            var logic = entities.GetEntity(id).Logic as T;
            return logic != null && logic.Available && !logic.IsHiding && logic.ID == id ? logic : null;
        }

        public static void Cancel(int id)
        {
            if (pending.TryGetValue(id, out var request)) request.Cancelled = true;
            // Pending resource loads finish normally so their data has one explicit owner on every path.
            var logic = Find<EntityLogicBase>(id);
            if (logic != null) logic.SafeHide();
        }

        public static void CancelRound(int roundId)
        {
            foreach (var pair in pending) if (pair.Value.Round == roundId) pair.Value.Cancelled = true;
            if (entities == null) return;
            foreach (var entity in entities.GetAllLoadedEntities())
                if (entity.Logic is EntityLogicBase logic && logic.SpawnData != null && logic.SpawnData.RoundId == roundId)
                    logic.SafeHide();
        }

        public static void CancelChildren(Transform parent)
        {
            if (parent == null) return;
            foreach (var request in pending.Values)
                if (request.Data.Parent != null && (request.Data.Parent == parent || request.Data.Parent.IsChildOf(parent)))
                    request.Cancelled = true;
        }

        private static void OnShown(object sender, GameEventArgs args)
        {
            var result = (ShowEntitySuccessEventArgs)args;
            int id = result.Entity.Id;
            if (!pending.TryGetValue(id, out var request)) return;
            pending.Remove(id);
            var logic = result.Entity.Logic as EntityLogicBase;
            if (!string.IsNullOrEmpty(result.Entity.InitializationError))
            {
                string error = result.Entity.InitializationError;
                entities.HideEntity(result.Entity);
                ReleaseUnclaimed(id, request);
                if (!request.Cancelled) request.Failed?.Invoke(error);
                return;
            }
            if (request.Cancelled || logic == null || logic.ID != id || !logic.Available || logic.IsHiding)
            {
                if (logic != null && logic.ID == id) logic.SafeHide();
                ReleaseUnclaimed(id, request);
                return;
            }
            try { request.Shown?.Invoke(logic); }
            catch { logic.SafeHide(); throw; }
        }

        private static void OnFailed(object sender, GameEventArgs args)
        {
            var result = (ShowEntityFailureEventArgs)args;
            if (!pending.TryGetValue(result.EntityId, out var request)) return;
            pending.Remove(result.EntityId);
            var logic = Find<EntityLogicBase>(result.EntityId);
            if (logic != null) logic.SafeHide();
            ReleaseUnclaimed(result.EntityId, request);
            if (!request.Cancelled) request.Failed?.Invoke(result.ErrorMessage);
        }

        private static void ReleaseUnclaimed(int id, Request request)
        {
            if (request.Data.Id == id && !request.Data.OwnedByEntity) ReferencePool.Release(request.Data);
        }
    }
}

using UnityEngine;
using Spine.Unity;

namespace Luna.Gameplay
{
    public class DropItemLogic : EntityLogicBase
    {
        private LootData data;
        private float wait;
        private float speed;
        private bool collected;
        private int targetId;
        private SkeletonAnimation skeleton;
        protected override void OnInit(object userData) { base.OnInit(userData); skeleton = GetComponentInChildren<SkeletonAnimation>(true); }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData); data = (LootData)userData;
            wait = 1.5f; speed = 0.5f; collected = false; targetId = 0;
            if (skeleton != null)
            {
                skeleton.Initialize(false);
                if (skeleton.Skeleton.Data.FindAnimation("ready") != null)
                {
                    skeleton.AnimationState.SetAnimation(0, "ready", false);
                    if (skeleton.Skeleton.Data.FindAnimation("idle") != null) skeleton.AnimationState.AddAnimation(0, "idle", true, 0f);
                }
            }
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (collected) return;
            wait -= elapsed;
            if (wait > 0f) return;
            var target = LunaSpawn.Find<UnitLogic>(targetId);
            if (target == null)
            {
                var player = LunaSession.Current?.LunaPlayer;
                target = UnitLogic.From(player);
                if (target == null || !target.Available || target.IsHiding || target.SpawnData.RoundId != data.RoundId) return;
                targetId = target.ID;
            }
            var destination = target.ProjecttileArrivalLocation != null ? target.ProjecttileArrivalLocation.transform.position : target.CachedTransform.position;
            speed = Mathf.Min(18f, speed + elapsed * 12f);
            CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, destination, speed * elapsed);
            if ((destination - CachedTransform.position).sqrMagnitude > 0.01f) return;
            collected = true;
            Grant(data);
            var effect = Resources.Load<GameObject>("Prefabs/06Effects/GetCoinEffect");
            LunaEffects.Show(effect, target.CachedTransform.position, targetId, true);
            SafeHide();
        }
        protected virtual void Grant(LootData loot)
        {
            if (GrantRoundLoot(loot)) return;
            if (!LunaDataTable.Instance.itemTable.TryGetValue(loot.ItemId, out var item))
                throw new System.InvalidOperationException("Loot refers to unknown item " + loot.ItemId);
            item.itemCount += loot.Count;
            var rewards = LunaSession.Current.pickUpData;
            rewards.TryGetValue(loot.ItemId, out int count); rewards[loot.ItemId] = count + loot.Count;
        }
        protected bool GrantRoundLoot(LootData loot)
        {
            if (loot.RoundId <= 0) return false; // Isolated entity fixtures have no live round.
            var round = LunaScenes.CurrentGame;
            if (round != null && round.RoundId == loot.RoundId) round.TryCollectLoot(ID);
            return true;
        }
        protected override void OnHide(bool isShutdown, object userData) { data = null; targetId = 0; base.OnHide(isShutdown, userData); }
    }
}

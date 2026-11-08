using UnityEngine;

namespace Luna.Gameplay
{
    public sealed class BuffIconData : LunaEntityData
    {
        public eBuffType BuffType;
        public static BuffIconData Create(int ownerId, eBuffType type, int roundId)
        {
            var data = Create<BuffIconData>(); data.OwnerId = ownerId; data.BuffType = type; data.RoundId = roundId;
            return data;
        }
        public override void Clear() { base.Clear(); BuffType = default; }
    }
    public sealed class BuffIconLogic : EntityLogicBase
    {
        private SpriteRenderer sprite;
        private BuffIconData data;
        private float blink;
        protected override void OnInit(object userData) { base.OnInit(userData); sprite = GetComponentInChildren<SpriteRenderer>(true); }
        protected override void OnShow(object userData) { base.OnShow(userData); data = (BuffIconData)userData; blink = 0f; }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            var owner = LunaSpawn.Find<UnitLogic>(data.OwnerId);
            if (owner == null || owner.stat.Hp <= 0f) { SafeHide(); return; }
            var buffs = owner.BuffImp.Buffs;
            int index = buffs.FindIndex(buff => buff.BuffType == data.BuffType);
            if (index < 0) { SafeHide(); return; }
            CachedTransform.position = owner.CachedTransform.position + new Vector3((index - (buffs.Count - 1) * 0.5f) * 0.28f, owner.BuffImp.IconHeight + 0.28f, 0f);
            blink += elapsed;
            if (sprite != null)
            {
                var color = sprite.color;
                color.a = buffs[index].BuffLeftTime <= 3f ? 0.5f + 0.4f * Mathf.Sin(blink * 6f) : 1f;
                sprite.color = color;
            }
        }
        protected override void OnHide(bool isShutdown, object userData) { data = null; base.OnHide(isShutdown, userData); }
    }
}

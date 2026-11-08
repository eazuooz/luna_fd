using UnityEngine;
using UnityEngine.UI;

namespace Luna.Gameplay
{
    public sealed class HealthBarLogic : EntityLogicBase
    {
        private Slider slider;
        private Image[] images;
        private int ownerId;
        private float hitTime;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData); slider = GetComponentInChildren<Slider>(true); images = GetComponentsInChildren<Image>(true);
        }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData); ownerId = ((HealthBarData)userData).OwnerId; hitTime = 0f;
        }
        public void Hit() { hitTime = 0.3f; }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            var owner = LunaSpawn.Find<UnitLogic>(ownerId);
            if (owner == null || owner.stat.Hp <= 0f) { SafeHide(); return; }
            var bounds = owner.CachedTransform.Find("Canvas") as RectTransform;
            CachedTransform.position = bounds != null ? bounds.position + Vector3.up * bounds.rect.height * 0.5f : owner.CachedTransform.position + Vector3.up;
            if (slider != null) slider.value = Mathf.Clamp01(owner.stat.Hp / Mathf.Max(1f, owner.HealthBarHpPoint));
            hitTime = Mathf.Max(0f, hitTime - elapsed);
            float alpha = owner.unitState.IsLobbyMove ? 0f : hitTime > 0f ? 1f : 0.4f;
            foreach (var image in images) { var color = image.color; color.a = alpha; image.color = color; }
        }
    }
}

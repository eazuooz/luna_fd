using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.Gameplay
{
    public sealed class DamageTextLogic : EntityLogicBase
    {
        private Text label;
        private float remaining;
        protected override void OnInit(object userData) { base.OnInit(userData); label = GetComponentInChildren<Text>(true); }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            var data = (DamageTextData)userData;
            remaining = 0.3f;
            if (label != null)
            {
                label.text = Mathf.CeilToInt(Mathf.Abs(data.Value)).ToString(CultureInfo.InvariantCulture);
                label.color = data.Recovery ? Color.green : data.Critical ? Color.yellow : Color.white;
            }
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            CachedTransform.position += Vector3.up * (1.5f * elapsed);
            remaining -= elapsed;
            if (remaining <= 0f) SafeHide();
        }
    }
}

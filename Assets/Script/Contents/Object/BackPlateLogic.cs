using UnityEngine;

namespace Luna.Gameplay
{
    public sealed class BackPlateLogic : EntityLogicBase
    {
        private Vector3 origin;
        private float offset;
        protected override void OnShow(object userData) { base.OnShow(userData); origin = transform.position; offset = 0f; }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (IsHiding) return;
            offset = Mathf.Repeat(offset + 0.1f * elapsed, 13.6f);
            transform.position = origin + Vector3.left * offset;
        }
    }
}

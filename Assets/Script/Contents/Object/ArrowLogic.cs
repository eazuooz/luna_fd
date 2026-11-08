using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Authored arcing shot, driven once by the native Entity update instead of an iTween MonoBehaviour.</summary>
    public sealed class ArrowLogic : ProjectileLogic
    {
        private Vector3 start;
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            start = transform.position;
        }

        protected override void UpdateFlight(UnitLogic target, float elapsedSeconds)
        {
            float t = Mathf.Clamp01(FlightAge / Data.TravelSeconds);
            Vector3 end = target.ProjecttileArrivalLocation != null ? target.ProjecttileArrivalLocation.transform.position : Data.EndPosition;
            Vector3 control = (start + end) * 0.5f + Vector3.up * Data.ArcHeight;
            Vector3 next = (1f - t) * (1f - t) * start + 2f * (1f - t) * t * control + t * t * end;
            Vector3 direction = next - transform.position;
            transform.position = next;
            if (direction.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            if (t >= 1f) Impact(target);
        }
    }
}

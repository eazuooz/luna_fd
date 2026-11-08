using System;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Native projectile lifecycle. No caller patches fields after asynchronous Show.</summary>
    public class ProjectileLogic : EntityLogicBase
    {
        public ProjectileData Data { get; private set; }
        protected float FlightAge { get; private set; }
        private float speed;
        private bool impacted;
        private Transform sprite;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            sprite = transform.Find("Sprite");
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            Data = userData as ProjectileData ?? throw new ArgumentException("ProjectileLogic requires ProjectileData.");
            FlightAge = 0f;
            speed = Data.Speed;
            impacted = false;
        }

        protected override void OnUpdate(float elapsedSeconds, float realElapsedSeconds)
        {
            base.OnUpdate(elapsedSeconds, realElapsedSeconds);
            if (IsHiding || Data == null) return;
            FlightAge += elapsedSeconds;
            UnitLogic target = UnitLogic.Find(Data.TargetId);
            UnitLogic attacker = UnitLogic.Find(Data.AttackerId);
            // An ID names one entity life. A reused GameObject cannot become this shot's target or shooter.
            if (FlightAge >= Data.Lifetime || !Living(target) || !Living(attacker)) { SafeHide(); return; }
            UpdateFlight(target, elapsedSeconds);
        }

        protected virtual void UpdateFlight(UnitLogic target, float elapsedSeconds)
        {
            Vector3 destination = target.ProjecttileArrivalLocation != null ? target.ProjecttileArrivalLocation.transform.position : target.transform.position;
            Vector3 before = transform.position;
            if (Vector3.Distance(before, destination) <= 0.2f) { Impact(target); return; }
            speed = Mathf.Max(1f, speed - 2f * elapsedSeconds);
            Vector3 aim = destination;
            if (Data.Kind == ProjectileKind.Arrow) aim.y += Mathf.Abs(destination.x - before.x);
            Vector3 step = Vector3.MoveTowards(before, aim, speed * elapsedSeconds);
            transform.position = step;
            Vector3 direction = step - before;
            if (direction.sqrMagnitude > 0.000001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (Data.Kind == ProjectileKind.Arrow) transform.rotation = Quaternion.Euler(0, 0, angle);
                else if (sprite != null) sprite.rotation = Quaternion.Euler(0, 0, angle + 90f);
            }
            if (Vector3.Distance(step, destination) <= 0.2f) Impact(target);
        }

        protected void Impact(UnitLogic target)
        {
            if (impacted || IsHiding || Data == null) return;
            impacted = true;
            UnitLogic attacker = UnitLogic.Find(Data.AttackerId);
            if (Living(target) && Living(attacker) && target.ID == Data.TargetId)
            {
                if (Data.StunSeconds > 0f) target.TakeStun(Data.StunSeconds);
                target.Attack(attacker.gameObject, Data.Damage, Data.Critical, Data.Skill);
                if (Data.HitEffect != null)
                    LunaSpawn.Show<EffectLogic>(Data.HitEffect, EffectData.Create(transform.position, 0, false, Data.RoundId));
            }
            SafeHide();
        }

        private static bool Living(UnitLogic unit) => unit != null && unit.Available && !unit.IsHiding && unit.stat.Hp > 0f && !unit.unitState.IsDeath;

        protected override void OnHide(bool isShutdown, object userData)
        {
            impacted = true;
            Data = null;
            FlightAge = speed = 0f;
            base.OnHide(isShutdown, userData);
        }
    }
}

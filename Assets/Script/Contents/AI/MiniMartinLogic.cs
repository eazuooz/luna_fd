using System;
using GameFramework.Fsm;
using Spine;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    /// <summary>Damageable summoned bomb. Native FSM owns travel, fuse animation and one impact.</summary>
    public class MiniMartinLogic : UnitLogic
    {
        private IFsm<MiniMartinLogic> summonFsm;
        private bool exploding;
        private bool detonated;
        private float fuseAge;
        private int explosionTargetId;
        private float moveSpeed, triggerRange, damage;
        private GameObject explosionEffect;
        protected virtual bool Flying => false;
        protected virtual string ExplosionAnimation => "death";

        protected override void ConfigureUnitForSpawn()
        {
            base.ConfigureUnitForSpawn();
            exploding = detonated = false;
            fuseAge = 0f; explosionTargetId = 0;
            moveSpeed = Mathf.Max(0f, Data.SummonMoveSpeed);
            triggerRange = Mathf.Max(0.1f, Data.SummonRange);
            damage = Mathf.Max(0f, Data.SummonDamage);
            explosionEffect = Data.ImpactEffect;
        }

        protected override void CreateCombatFsm()
        {
            DestroyCombatFsm();
            summonFsm = GameEntry.GetComponent<FsmComponent>().CreateFsm("LunaSummonBomb/" + ID, this,
                new SummonTravelState(), new SummonFuseState(), new SummonDeadState());
            summonFsm.Start<SummonTravelState>();
        }

        protected override void DestroyCombatFsm()
        {
            if (summonFsm == null) return;
            var component = GameEntry.GetComponent<FsmComponent>();
            if (component != null && !summonFsm.IsDestroyed) component.DestroyFsm(summonFsm);
            summonFsm = null;
        }

        internal Type SelectSummonState()
        {
            if (stat.Hp <= 0f || unitState.IsDeath) return typeof(SummonDeadState);
            return exploding ? typeof(SummonFuseState) : typeof(SummonTravelState);
        }

        internal void Travel(float elapsed)
        {
            if (Data.OwnerId != 0 && Find(Data.OwnerId) == null) { SafeHide(); return; }
            if (unitState.IsLobbyMove || isStun) { SetAnimation("idle", true); return; }
            UnitLogic closest = null;
            float distance = float.PositiveInfinity;
            foreach (UnitLogic candidate in ActiveUnits)
            {
                if (candidate == this || !LunaCombat.IsInCombat(candidate) || candidate.Data.RoundId != Data.RoundId || candidate.UnitSide == UnitSide) continue;
                float candidateDistance = Mathf.Abs(candidate.transform.position.x - transform.position.x);
                if (candidateDistance <= triggerRange && candidateDistance < distance) { closest = candidate; distance = candidateDistance; }
            }
            if (closest != null)
            {
                explosionTargetId = closest.ID; exploding = true;
                return;
            }
            SetAnimation(Flying ? "idle" : "run", true);
            int direction = UnitSide == eUnitSide.Enemy ? -1 : 1;
            Vector3 desired = transform.position + Vector3.right * direction * moveSpeed * elapsed;
            if (Flying)
            {
                bool aboveAlly = false;
                foreach (UnitLogic candidate in ActiveUnits)
                    if (candidate != this && LunaCombat.IsInCombat(candidate) && candidate.Data.RoundId == Data.RoundId && candidate.UnitSide == UnitSide && Mathf.Abs(candidate.transform.position.x - transform.position.x) < 2f) { aboveAlly = true; break; }
                desired.y = Mathf.MoveTowards(desired.y, aboveAlly ? 0f : 3f, moveSpeed * 1.5f * elapsed);
            }
            transform.position = LunaCombat.ClampPosition(this, desired);
        }

        internal void StartFuse() { fuseAge = 0f; SetAnimation(ExplosionAnimation, false); }
        internal void TickFuse(float elapsed)
        {
            // Missing/broken animation cannot leave an armed bomb alive forever.
            fuseAge += elapsed;
            if (fuseAge >= 5f) Detonate();
        }

        protected override void OnAnimationComplete(TrackEntry entry)
        {
            if (exploding && entry.Animation.Name == ExplosionAnimation) { Detonate(); return; }
            base.OnAnimationComplete(entry);
        }

        private void Detonate()
        {
            if (detonated || IsHiding || Data == null) return;
            detonated = true;
            UnitLogic target = Find(explosionTargetId);
            if (LunaCombat.IsInCombat(target) && target.Data.RoundId == Data.RoundId && target.UnitSide != UnitSide)
                target.Attack(gameObject, damage, true, true);
            if (explosionEffect != null)
            {
                Transform anchor = transform.Find("AttackPos");
                LunaSpawn.Show<EffectLogic>(explosionEffect, EffectData.Create(anchor != null ? anchor.position : transform.position, 0, false, Data.RoundId));
            }
            SafeHide();
        }

        protected override void OnUnitHidden()
        {
            exploding = false; detonated = true; explosionTargetId = 0; explosionEffect = null;
            base.OnUnitHidden();
        }
    }

    public sealed class MartinLogic : MiniMartinLogic
    {
        protected override bool Flying => true;
        protected override string ExplosionAnimation => "attack";
    }

    internal abstract class SummonBombState : FsmState<MiniMartinLogic>
    {
        protected override void OnUpdate(IFsm<MiniMartinLogic> fsm, float elapsed, float realElapsed)
        {
            if (fsm.Owner.IsHiding || fsm.Owner.Data == null) return;
            Type next = fsm.Owner.SelectSummonState();
            if (next != GetType()) { ChangeState(fsm, next); return; }
            Tick(fsm.Owner, elapsed);
        }
        protected virtual void Tick(MiniMartinLogic owner, float elapsed) { }
    }
    internal sealed class SummonTravelState : SummonBombState
    {
        protected override void Tick(MiniMartinLogic owner, float elapsed) => owner.Travel(elapsed);
    }
    internal sealed class SummonFuseState : SummonBombState
    {
        protected override void OnEnter(IFsm<MiniMartinLogic> fsm) { base.OnEnter(fsm); fsm.Owner.StartFuse(); }
        protected override void Tick(MiniMartinLogic owner, float elapsed) => owner.TickFuse(elapsed);
    }
    internal sealed class SummonDeadState : SummonBombState
    {
        protected override void OnEnter(IFsm<MiniMartinLogic> fsm) { base.OnEnter(fsm); fsm.Owner.BeginDeath(); }
    }
}

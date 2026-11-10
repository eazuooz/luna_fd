using System;
using GameFramework.Fsm;
using Spine;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    public partial class UnitLogic
    {
        private IFsm<UnitLogic> combatFsm;
        private bool attackCritical;
        private bool attackInProgress;
        private bool skillInProgress;
        private bool deathStarted;
        private bool deathNotificationSent;
        public bool IsAttackInProgress => attackInProgress;
        public virtual bool HasAutomaticSkill => false;
        protected void FinishSkillExecution() { skillInProgress = false; }

        protected virtual void CreateCombatFsm()
        {
            combatFsm = GameEntry.GetComponent<FsmComponent>().CreateFsm(ID.ToString(), this,
                new UnitIdleState(), new UnitMoveState(), new UnitAttackState(), new UnitSkillState(),
                new UnitHitState(), new UnitDeathState(), new UnitLobbyState());
            combatFsm.Start<UnitIdleState>();
        }
        protected virtual void DestroyCombatFsm()
        {
            if (combatFsm == null) return;
            var retiring = combatFsm;
            combatFsm = null;
            var component = GameEntry.GetComponent<FsmComponent>();
            if (component != null) component.DestroyFsm(retiring);
        }
        public void ResetAnimation()
        {
            attackCritical = attackInProgress = skillInProgress = deathStarted = false;
            if (Animation == null) return;
            Animation.AnimationState.ClearTracks();
            Animation.timeScale = 1f;
            Animation.Skeleton.SetToSetupPose();
            SetAnimation("idle", true);
        }
        public void SetAnimation(string name, bool loop = false)
        {
            if (Animation == null || Animation.Skeleton.Data.FindAnimation(name) == null) return;
            if (Animation.AnimationName != name) Animation.AnimationState.SetAnimation(0, name, loop);
        }
        public void RefreshTarget()
        {
            if (Data == null || isStun || unitState.IsLobbyMove) return;
            var current = Target;
            float direction = UnitSide == eUnitSide.Enemy ? -1f : 1f;
            if (current != null && current.Data != null && current.Data.RoundId == Data.RoundId && current.UnitSide != UnitSide
                && current.Available && !current.IsHiding && current.stat.Hp > 0f)
            {
                // An explicit taunt can redirect an adjacent enemy behind its normal marching direction.
                float distance = Mathf.Abs(current.transform.position.x - transform.position.x);
                if (distance <= stat.AttackRange && !current.unitState.IsLobbyMove) return;
            }
            Data.TargetId = 0;
            UnitLogic nearest = null, tower = null;
            float best = stat.AttackRange, towerDistance = stat.AttackRange;
            foreach (var candidate in activeUnits)
            {
                if (candidate == null || candidate == this || candidate.Data == null || candidate.IsHiding
                    || candidate.Data.RoundId != Data.RoundId || candidate.UnitSide == UnitSide
                    || candidate.stat.Hp <= 0f || candidate.unitState.IsLobbyMove) continue;
                float offset = candidate.transform.position.x - transform.position.x;
                float distance = this is PlayerLogic ? Mathf.Abs(offset) : offset * direction;
                if (distance < 0f) continue;
                if (candidate.hitPriority == eHitPriority.TOWER)
                {
                    if (distance <= towerDistance) { tower = candidate; towerDistance = distance; }
                }
                else if (distance <= best) { nearest = candidate; best = distance; }
            }
            Data.TargetId = nearest != null ? nearest.ID : tower != null ? tower.ID : 0;
        }
        internal Type SelectCombatState()
        {
            if (stat.Hp <= 0f || unitState.IsDeath) return typeof(UnitDeathState);
            if (unitState.IsDamaged && !unitState.IsSuperArmour) return typeof(UnitHitState);
            if (unitState.IsLobbyMove) return typeof(UnitLobbyState);
            if (isStun) return typeof(UnitIdleState);
            RefreshTarget();
            if (attackInProgress || skillInProgress || unitState.IsActionDelay) return typeof(UnitIdleState);
            if (HasAutomaticSkill && stat.Mp >= 100f && (!unitState.IsTargetingSkill || Target != null)) return typeof(UnitSkillState);
            if (Target != null && stat.AttackRange > 0f) return typeof(UnitAttackState);
            return stat.MoveSpeed != 0f ? typeof(UnitMoveState) : typeof(UnitIdleState);
        }
        public virtual void MoveCombat(float direction, float elapsed)
        {
            if (!LunaCombat.IsInCombat(this) || isStun) return;
            LunaCombat.Move(this, Vector3.right * (Mathf.Abs(stat.MoveSpeed) + playerAuraMoveBonus) * direction * elapsed);
            SetAnimation(Animation != null && Animation.Skeleton.Data.FindAnimation("move") != null ? "move" : "idle", true);
        }
        public virtual void BeginAttack()
        {
            if (Data == null || Target == null || attackInProgress || isStun) return;
            attackInProgress = true;
            attackCritical = Defined.CanProbability(stat.CriticalActuationProbability);
            unitState.IsActionDelay = true;
            stat.AttackDelayTime = -Mathf.Max(0.01f, stat.AttackSpeed);
            if (isWarf && Target != null)
            {
                float direction = UnitSide == eUnitSide.Enemy ? -1f : 1f;
                float distance = (Target.transform.position.x - transform.position.x) * direction;
                if (distance > warfRange) LunaCombat.Move(this, Vector3.right * direction * (distance - warfRange));
            }
            if (Animation == null || Animation.Skeleton.Data.FindAnimation("attack") == null)
            { OnAttackAnimationEvent(); attackInProgress = false; return; }
            Animation.timeScale = stat.AttackSpeed < 1f ? 1f / Mathf.Max(0.01f, stat.AttackSpeed) : 1f;
            SetAnimation(attackCritical && Animation.Skeleton.Data.FindAnimation("attack02") != null ? "attack02" : "attack");
        }
        public virtual void BeginSkill()
        {
            if (Data == null || skillInProgress || isStun) return;
            skillInProgress = true;
            if (Animation != null && Animation.Skeleton.Data.FindAnimation("skill") != null) SetAnimation("skill");
            else { OnSkillAnimationEvent(); skillInProgress = false; }
        }
        public void BeginDamaged()
        {
            attackInProgress = skillInProgress = false;
            if (Animation != null && Animation.Skeleton.Data.FindAnimation("damage") != null) SetAnimation("damage");
            else unitState.IsDamaged = false;
        }
        public virtual void BeginDeath()
        {
            if (deathStarted) return;
            deathStarted = true; attackInProgress = skillInProgress = false;
            StopAllCoroutines();
            OnUnitDying();
            if (Animation != null && Animation.Skeleton.Data.FindAnimation("death") != null) SetAnimation("death");
            else CompleteDeath();
        }
        protected virtual void OnUnitDying() { }
        protected virtual void CompleteDeath()
        {
            if (Data == null || IsHiding || deathNotificationSent) return;
            deathNotificationSent = true;
            unitState.IsDeath = true;
            if (Data.InitialMode == UnitSpawnMode.Battle && !unitState.IsLobbyMove)
                GameEntry.GetComponent<EventComponent>().Fire(this,
                    UnitDiedEventArgs.Create(ID, Data.RoundId, UnitSide, hitPriority, transform.position));
            DeathRelease();
            SafeHide();
        }
        private void HandleAnimationEvent(TrackEntry track, Spine.Event e)
        {
            if (Data == null || IsHiding || stat.Hp <= 0f || unitState.IsLobbyMove) return;
            // Mixing-out tracks can still emit callbacks after a new action has taken ownership.
            if (Animation == null || !ReferenceEquals(track, Animation.AnimationState.GetCurrent(0))) return;
            if (e.Data.Name == "attack") OnAttackAnimationEvent();
            else if (e.Data.Name == "skill") OnSkillAnimationEvent();
        }
        protected virtual void OnAttackAnimationEvent()
        {
            var target = Target;
            if (target == null || target.Data == null || target.stat.Hp <= 0f || target.IsHiding) return;
            float damage = stat.AttackDamage * (attackCritical ? stat.CriticalMultiples : 1f);
            if (View.Projectile != null)
            {
                foreach (var point in ProjecttileCreateLocations)
                {
                    var data = ProjectileData.Create(View.Projectile, ID, target.ID, point.transform.position, damage,
                        attackCritical, false, isStunAttack ? nextStunDuring : 0f, Data.RoundId);
                    LunaSpawn.Show<ProjectileLogic>(View.Projectile, data);
                }
            }
            else
            {
                target.Attack(gameObject, damage, attackCritical);
                if (isStunAttack) target.TakeStun(nextStunDuring);
            }
            isStunAttack = false; nextStunDuring = 0f;
            GetAttackSound(); PlayOneSound(AttackSoundClip != null ? AttackSoundClip : GetLunaAssetBundle().sound_hit);
        }
        protected virtual void OnSkillAnimationEvent()
        {
            stat.Mp = 0f;
            unitState.IsSkillActivaion = true;
            unitState.IsActionDelay = true;
            stat.AttackDelayTime = -Mathf.Max(0.01f, stat.AttackSpeed);
        }
        private void HandleAnimationComplete(TrackEntry track)
        {
            if (Data == null || IsHiding) return;
            // In particular, an old attack completion must not replace the current death with idle.
            if (Animation == null || !ReferenceEquals(track, Animation.AnimationState.GetCurrent(0))) return;
            OnAnimationComplete(track);
        }
        protected virtual void OnAnimationComplete(TrackEntry track)
        {
            string name = track.Animation.Name;
            if (name == "death") { CompleteDeath(); return; }
            if (name == "damage") unitState.IsDamaged = false;
            if (name == "attack" || name == "attack02" || name == "attack2") attackInProgress = false;
            if (name == "skill") skillInProgress = false;
            if (name != "idle" && name != "move" && name != "skill_loop" && name != "defense" && name != "defense_ready") SetAnimation("idle", true);
        }
    }

    public abstract class UnitCombatState : FsmState<UnitLogic>
    {
        protected override void OnUpdate(IFsm<UnitLogic> fsm, float elapsed, float realElapsed)
        {
            var owner = fsm.Owner;
            if (!owner.Available || owner.IsHiding || owner.Data == null) return;
            Type next = owner.SelectCombatState();
            if (next != GetType()) { ChangeState(fsm, next); return; }
            Execute(owner, elapsed);
        }
        protected virtual void Execute(UnitLogic actor, float elapsed) { }
    }
    public sealed class UnitIdleState : UnitCombatState { }
    public sealed class UnitMoveState : UnitCombatState
    {
        protected override void Execute(UnitLogic actor, float elapsed) => actor.MoveCombat(actor.UnitSide == eUnitSide.Enemy ? -1f : 1f, elapsed);
    }
    public sealed class UnitAttackState : UnitCombatState { protected override void OnEnter(IFsm<UnitLogic> fsm) { fsm.Owner.BeginAttack(); } }
    public sealed class UnitSkillState : UnitCombatState { protected override void OnEnter(IFsm<UnitLogic> fsm) { fsm.Owner.BeginSkill(); } }
    public sealed class UnitHitState : UnitCombatState { protected override void OnEnter(IFsm<UnitLogic> fsm) { fsm.Owner.BeginDamaged(); } }
    public sealed class UnitDeathState : UnitCombatState { protected override void OnEnter(IFsm<UnitLogic> fsm) { fsm.Owner.BeginDeath(); } }
    public sealed class UnitLobbyState : UnitCombatState
    {
        protected override void Execute(UnitLogic actor, float elapsed)
        {
            if (actor.unitState.IsLobbyStartMove) actor.transform.position += Vector3.right * 5f * elapsed;
            else if (actor.unitState.IsLobbyBackMove) actor.transform.position += Vector3.left * 5f * elapsed;
        }
    }
}

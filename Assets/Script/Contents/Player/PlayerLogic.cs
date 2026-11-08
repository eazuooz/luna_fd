using System;
using GameFramework.Fsm;
using Spine;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    /// <summary>Native player entity. One per-life FSM owns movement, combat and death decisions.</summary>
    public class PlayerLogic : UnitLogic
    {
        private IFsm<PlayerLogic> playerFsm;
        private int auraId;
        private bool deathNotified;
        private float attackDeadline;
        public int Moving { get; private set; }
        public virtual float IncomeMultiplier => 1f;
        public PlayerAbilityController Abilities { get; private set; }
        public string CurrentStateName { get; internal set; }
        public UnitLogic AllyTower => Data != null ? Find(Data.AllyTowerId) : null;

        protected override void ConfigureUnitForSpawn()
        {
            base.ConfigureUnitForSpawn();
            Moving = 0;
            deathNotified = false;
            attackDeadline = 0f;
            CurrentStateName = "ready";
            GameObject aura = View.GetPrefab("buffPrefab");
            if (aura != null)
            {
                var data = EffectData.Create(transform.position + Vector3.down * 0.1f, ID, true, Data.RoundId);
                data.Rotation = Quaternion.Euler(-90f, 0f, 0f);
                data.Lifetime = 0f;
                data.Loop = true;
                auraId = data.Id;
                LunaSpawn.Show<EffectLogic>(aura, data);
            }
        }

        protected override void CreateCombatFsm()
        {
            DestroyCombatFsm();
            Abilities?.Dispose();
            Abilities = new PlayerAbilityController(this);
            playerFsm = GameEntry.GetComponent<FsmComponent>().CreateFsm("LunaPlayer/" + ID, this,
                new PlayerIdleState(), new PlayerMoveState(), new PlayerAttackState(), new PlayerHitState(), new PlayerSkillState(), new PlayerDeathState());
            playerFsm.Start<PlayerIdleState>();
        }

        protected override void DestroyCombatFsm()
        {
            if (playerFsm == null) return;
            var manager = GameEntry.GetComponent<FsmComponent>();
            if (manager != null && !playerFsm.IsDestroyed) manager.DestroyFsm(playerFsm);
            playerFsm = null;
        }

        public void SetMovement(int direction)
        {
            int next = Math.Sign(direction);
            if (Moving != next && next != 0) ResetAnimation();
            Moving = next;
        }
        public void LMove() => SetMovement(-1);
        public void RMove() => SetMovement(1);
        public void NMove() => SetMovement(0);

        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (Data != null && !IsHiding) Abilities?.Tick(elapsed);
        }

        protected override void OnAttackAnimationEvent()
        {
            base.OnAttackAnimationEvent();
            Abilities?.OnAttack();
        }

        internal Type SelectPlayerState()
        {
            if (stat.Hp <= 0f || unitState.IsDeath) return typeof(PlayerDeathState);
            if (isStun || (unitState.IsDamaged && !unitState.IsSuperArmour)) return typeof(PlayerHitState);
            if (unitState.IsLobbyMove || unitState.IsLobbyStartMove || unitState.IsLobbyBackMove) return typeof(PlayerIdleState);
            if (Moving != 0) return typeof(PlayerMoveState);
            if (unitState.IsSkillActivaion) return typeof(PlayerSkillState);
            if (IsAttackInProgress && Time.time < attackDeadline) return typeof(PlayerAttackState);
            if (IsAttackInProgress) ResetAnimation();
            RefreshTarget();
            if (Target != null && !unitState.IsActionDelay && Mathf.Abs(Target.transform.position.x - transform.position.x) <= stat.AttackRange)
                return typeof(PlayerAttackState);
            return typeof(PlayerIdleState);
        }

        internal void EnterPlayerAttack()
        {
            attackDeadline = Time.time + Mathf.Max(1f, stat.AttackSpeed * 3f);
            BeginAttack();
        }

        internal void TickPlayerMovement(float elapsed)
        {
            if (unitState.IsLobbyStartMove || unitState.IsLobbyBackMove)
            {
                int direction = unitState.IsLobbyBackMove ? -1 : 1;
                if (Animation != null) Animation.Skeleton.ScaleX = direction;
                transform.position += Vector3.right * direction * 5f * elapsed;
                return;
            }
            if (unitState.IsLobbyMove || Moving == 0 || isStun) return;
            if (Animation != null) Animation.Skeleton.ScaleX = Moving;
            if (Moving > 0 && Target != null && Target.transform.position.x <= transform.position.x + 1f) return;
            MoveCombat(Moving, elapsed);
        }

        protected override void OnAnimationComplete(TrackEntry entry)
        {
            if (entry.Animation.Name == "skill") unitState.IsSkillActivaion = false;
            if (entry.Animation.Name == "death") NotifyDeath();
            base.OnAnimationComplete(entry);
        }

        private void NotifyDeath()
        {
            if (deathNotified || Data == null || Data.InitialMode == UnitSpawnMode.Preview || stat.Hp > 0f) return;
            deathNotified = true;
            var events = GameEntry.GetComponent<EventComponent>();
            if (events != null) events.Fire(this, PlayerDiedEventArgs.Create(ID, Data.RoundId, Data.AllyTowerId, Mathf.Max(0f, stat.SpawnDelayTime)));
        }

        protected override void OnUnitDying()
        {
            Moving = 0;
            Abilities?.Dispose();
            Abilities = null;
            LunaSpawn.Cancel(auraId); auraId = 0;
            base.OnUnitDying();
        }

        protected override void OnUnitHidden()
        {
            NotifyDeath();
            Moving = 0;
            Abilities?.Dispose();
            Abilities = null;
            LunaSpawn.Cancel(auraId); auraId = 0;
            base.OnUnitHidden();
        }
    }

    internal abstract class PlayerState : FsmState<PlayerLogic>
    {
        protected override void OnEnter(IFsm<PlayerLogic> fsm)
        {
            base.OnEnter(fsm);
            fsm.Owner.CurrentStateName = GetType().Name;
            Enter(fsm.Owner);
        }
        protected override void OnUpdate(IFsm<PlayerLogic> fsm, float elapsed, float realElapsed)
        {
            PlayerLogic player = fsm.Owner;
            if (player.IsHiding || !player.Available || player.Data == null) return;
            Type next = player.SelectPlayerState();
            if (next != GetType()) { ChangeState(fsm, next); return; }
            Tick(player, elapsed);
        }
        protected virtual void Enter(PlayerLogic player) { }
        protected virtual void Tick(PlayerLogic player, float elapsed) { }
    }

    internal sealed class PlayerIdleState : PlayerState
    {
        protected override void Enter(PlayerLogic player) => player.SetAnimation("idle", true);
        protected override void Tick(PlayerLogic player, float elapsed) => player.TickPlayerMovement(elapsed);
    }
    internal sealed class PlayerMoveState : PlayerState
    {
        protected override void Enter(PlayerLogic player) => player.SetAnimation("idle", true);
        protected override void Tick(PlayerLogic player, float elapsed) => player.TickPlayerMovement(elapsed);
    }
    internal sealed class PlayerAttackState : PlayerState
    {
        protected override void Enter(PlayerLogic player) => player.EnterPlayerAttack();
    }
    internal sealed class PlayerHitState : PlayerState
    {
        protected override void Enter(PlayerLogic player) => player.BeginDamaged();
    }
    internal sealed class PlayerSkillState : PlayerState
    {
        protected override void Enter(PlayerLogic player) => player.BeginSkill();
    }
    internal sealed class PlayerDeathState : PlayerState
    {
        protected override void Enter(PlayerLogic player) => player.BeginDeath();
    }
}

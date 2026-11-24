#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    private IEnumerator CrossRoundPartyContracts()
    {
        if (!Includes("buffs", "party-effects-round-ownership")) yield break;
        yield return EnsureFixtureLobby();
        BeginCase("buffs", "party-effects-round-ownership");
        HeroFixture guarded = null, separate = null;
        try
        {
            yield return CreateHeroFixture("Dren", eUltState.ULT1, value => guarded = value);
            yield return CreateHeroFixture("Goldy", eUltState.ULT2, value => separate = value);
            Require(guarded.Round != separate.Round, "Party isolation fixture reused its round identity.");
            HeroPartyEffects.Refresh();
            Require(Mathf.Abs(HeroPartyEffects.GoldBonusForRound(guarded.Round)) < 0.001f &&
                Mathf.Abs(HeroPartyEffects.GoldBonusForRound(separate.Round) - 10f) < 0.001f,
                "Goldy's ultimate income bonus escaped its owning round.");
            foreach (var fixture in new[] { guarded, separate })
            {
                var recipient = fixture.Soldier;
                recipient.stat.Armour = recipient.stat.AttackAvoidance = recipient.stat.SkillAvoidance = recipient.stat.SkillResistance = 0f;
                float before = recipient.stat.Hp;
                recipient.Attack(fixture.Enemy.gameObject, 100f);
                float expected = fixture == guarded ? 90f : 100f;
                Require(Mathf.Abs(before - recipient.stat.Hp - expected) <= 0.1f,
                    "Dren's party mitigation was missing or leaked to another round: expected " + expected + ", actual " + (before - recipient.stat.Hp));
            }
            guarded.Hero.stat.Hp = 0f; guarded.Hero.BeginDeath();
            float unguardedHp = guarded.Soldier.stat.Hp;
            guarded.Soldier.Attack(guarded.Enemy.gameObject, 100f);
            Require(Mathf.Abs(unguardedHp - guarded.Soldier.stat.Hp - 100f) <= 0.1f,
                "Dead Dren kept mitigating its old round's damage.");
            current.observations.Add("Coincident actors in different rounds: Dren ally took90/100, other round100/100; Goldy10% stayed only in its own round; death-start removed mitigation.");
        }
        finally
        {
            if (guarded != null) LunaSpawn.CancelRound(guarded.Round);
            if (separate != null) LunaSpawn.CancelRound(separate.Round);
        }
        if (guarded != null) yield return RetireFixture(guarded.Round);
        if (separate != null) yield return RetireFixture(separate.Round);
        EndCase("Live party effects used their actual owning round; a simultaneous unrelated round was unchanged, and death-start retired mitigation.");
    }

    private IEnumerator MotionContracts()
    {
        if (!Includes("motion", "whirlwind-boundaries-death-reuse")) yield break;
        yield return EnsureFixtureLobby();
        BeginCase("motion", "whirlwind-boundaries-death-reuse");
        Vector3 oldLeft = Defined.AllyTowerPosition, oldRight = Defined.EnemyTowerPosition;
        float oldScale = Time.timeScale;
        HeroFixture fixture = null;
        try
        {
            Time.timeScale = 4f;
            Defined.AllyTowerPosition = new Vector3(-8f, 0f); Defined.EnemyTowerPosition = new Vector3(8f, 0f);
            yield return CreateHeroFixture("Raccon", eUltState.ULT2, value => fixture = value);
            var raccon = (Raccon)fixture.Hero;
            raccon.transform.position = new Vector3(7.3f, 0f);
            fixture.Enemy.transform.position = new Vector3(7.95f, 0f);
            float hp = fixture.Enemy.stat.Hp;
            float started = Time.time;
            raccon.Skill(); raccon.Skill();
            Require(raccon.IsSpinning, "Raccon did not begin its real whirlwind.");
            while (raccon.IsSpinning && Time.time - started < 4f)
            {
                foreach (var actor in UnitLogic.ActiveUnits.Where(actor => actor.Data.RoundId == fixture.Round))
                    Require(actor.transform.position.x >= -8.001f && actor.transform.position.x <= 8.001f, "Whirlwind/crowd movement escaped the configured battle lane: " + actor.name + " at " + actor.transform.position);
                yield return null;
            }
            float duration = Time.time - started;
            Require(!raccon.IsSpinning && duration >= 2.35f && duration < 3.5f, "Whirlwind duration/duplicate activation was not bounded to one 2.5s cast: " + duration);
            Require(fixture.Enemy.stat.Hp < hp, "Boundary whirlwind produced no actual damage.");
            fixture.Enemy.TakeNuckbackMovement();
            float knockbackStart = Time.time;
            while (Time.time - knockbackStart < 1.7f)
            {
                Require(fixture.Enemy.transform.position.x <= 8.001f, "Knockback pushed its target beyond the stage boundary.");
                yield return null;
            }
            Require(Mathf.Abs(fixture.Enemy.transform.position.y) < 0.01f, "Knockback did not restore its original ground height.");
            raccon.transform.position = new Vector3(7.3f, 0f); raccon.Data.TargetId = fixture.Enemy.ID;
            raccon.Skill();
            yield return null;
            Require(raccon.IsSpinning, "Second independent cast did not start.");
            raccon.stat.Hp = 0f; raccon.BeginDeath();
            Require(!raccon.IsSpinning, "Death-start did not cancel an active spin.");
            float afterDeathHp = fixture.Enemy.stat.Hp;
            yield return new WaitForSeconds(0.8f);
            Require(Mathf.Abs(fixture.Enemy.stat.Hp - afterDeathHp) < 0.01f, "Cancelled spin continued damaging after its caster died.");
            int oldId = raccon.ID;
            raccon.SafeHide(); yield return null;
            var prefab = Resources.Load<GameObject>("Prefabs/01Heroes/Raccon");
            var data = HeroData.CreateFrom(prefab.GetComponent<UnitView>(), Vector3.zero);
            data.RoundId = fixture.Round; data.Rank = eUltState.STAR1; data.InitialMode = UnitSpawnMode.Staged;
            Raccon reused = null;
            yield return ShowFixture<Raccon>(prefab, data, actor => reused = actor);
            Require(reused.ID != oldId && LunaSpawn.Find<Raccon>(oldId) == null && !reused.IsSpinning && !reused.unitState.IsDeath, "Fresh native identity retained interrupted spin/death state.");
            current.observations.Add("Single-cast duration=" + duration + "s; damaged target remained within [-8,8]; death cancelled ticks; new ID=" + reused.ID);
        }
        finally
        {
            if (fixture != null) LunaSpawn.CancelRound(fixture.Round);
            Defined.AllyTowerPosition = oldLeft; Defined.EnemyTowerPosition = oldRight; Time.timeScale = oldScale;
        }
        if (fixture != null) yield return RetireFixture(fixture.Round);
        EndCase("Actual whirlwind, duplicate cast guard, target knockback, death cancellation and native reuse remained bounded.");
    }

    private IEnumerator JoeBasicProcOwnership()
    {
        if (!Includes("buffs", "joe-basic-proc-marker-ownership")) yield break;
        yield return EnsureFixtureLobby();
        BeginCase("buffs", "joe-basic-proc-marker-ownership");
        var previousRandom = UnityEngine.Random.state;
        HeroFixture fixture = null;
        try
        {
            yield return CreateHeroFixture("Joe", eUltState.ULT1, value => fixture = value);
            var joe = (Joe)fixture.Hero;
            var animation = joe.Animation;
            var attack = animation.Skeleton.Data.FindAnimation("attack");
            var marker = attack.Timelines.OfType<Spine.EventTimeline>()
                .SelectMany(timeline => timeline.Events)
                .Single(authored => authored.Data.Name == "attack");
            joe.stat.Mp = 35f;
            Require(marker.Time > 0f && marker.Time < attack.Duration && joe.View.Projectile != null,
                "Joe must use the original authored basic attack marker and projectile.");

            int procSeed = FindJoeProcSeed(true);
            int ordinarySeed = FindJoeProcSeed(false);
            BeginJoeSwing(joe, procSeed, armed: true);
            int pendingBefore = LunaSpawn.PendingCount;
            joe.BeginDamaged();
            Require(!joe.IsAttackInProgress && !joe.isStunAttack && joe.nextStunDuring == 0f && joe.stat.Mp == 35f,
                "Hit interruption did not clear the old basic stun reservation or changed mana.");
            animation.Update(1f);
            Require(LunaSpawn.PendingCount == pendingBefore && !FindJoeShots(joe.ID).Any(),
                "An interrupted authored attack track still released a projectile.");

            BeginJoeSwing(joe, procSeed, armed: true);
            int lostId = fixture.Enemy.ID;
            // Deliberately remove the original target before its real marker is dispatched.
            fixture.Enemy.SafeHide();
            joe.Data.TargetId = 0;
            animation.Update(marker.Time + .001f);
            Require(!joe.isStunAttack && joe.nextStunDuring == 0f && joe.stat.Mp == 35f &&
                !FindJoeShots(joe.ID).Any(), "A lost target left a pending stun or emitted a basic shot.");
            animation.Update(attack.Duration + .001f);

            var prefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), new Vector3(2.5f, 0f));
            data.RoundId = fixture.Round;
            data.Level = 1;
            data.Side = eUnitSide.Enemy;
            data.InitialMode = UnitSpawnMode.Staged;
            data.HealthMultiplier = 10000f;
            UnitLogic replacement = null;
            yield return ShowFixture<UnitLogic>(prefab, data, shown => replacement = shown);
            HoldAutomaticActions(replacement);
            replacement.stat.AttackDamage = replacement.stat.Armour = replacement.stat.AttackAvoidance = 0f;
            replacement.stat.DebuffAvoidance = replacement.stat.StunResistance = 0f;
            Require(replacement.ID != lostId && replacement.Data.RoundId == fixture.Round && UnitLogic.Find(lostId) == null,
                "The controlled replacement must have a distinct native ID in the same combat round.");
            joe.Data.TargetId = replacement.ID;
            float mana = joe.stat.Mp;
            float full = replacement.stat.Hp;
            BeginJoeSwing(joe, ordinarySeed, armed: false);
            animation.Update(marker.Time + .001f);
            Require(joe.stat.Mp == mana && !joe.isStunAttack && joe.nextStunDuring == 0f,
                "A new non-proc basic inherited state or spent skill mana.");
            yield return Until(() => replacement.stat.Hp < full, "Native non-proc projectile damages the replacement", 5f);
            Require(!replacement.isStun, "The replacement inherited the cancelled stun on a later non-proc basic.");
            yield return Until(() => !FindJoeShots(joe.ID).Any() && !joe.IsAttackInProgress,
                "Previous ordinary projectile and attack retire", 5f);

            joe.Data.TargetId = replacement.ID;
            mana = joe.stat.Mp;
            float ordinaryHp = replacement.stat.Hp;
            BeginJoeSwing(joe, procSeed, armed: true);
            animation.Update(marker.Time + .001f);
            Require(joe.stat.Mp == mana && !joe.isStunAttack && joe.nextStunDuring == 0f,
                "A valid proc was not consumed at its authored marker or changed mana.");
            yield return Until(() => replacement.stat.Hp < ordinaryHp && replacement.isStun, "Original Joe stun and actual hit", 5f);
            current.observations.Add("Interrupted and lost-target attacks retired. A new same-round ID took an ordinary hit without stun. A positive 15-percent roll produced an actual two-second stun request and hit.");

            static IEnumerable<ProjectileLogic> FindJoeShots(int source) =>
                Entities().OfType<ProjectileLogic>().Where(shot => shot.Data?.AttackerId == source);
        }
        finally
        {
            UnityEngine.Random.state = previousRandom;
            if (fixture != null) LunaSpawn.CancelRound(fixture.Round);
        }
        if (fixture != null) yield return RetireFixture(fixture.Round);
        EndCase("Joe's native attack marker owns one stun proc; interruption and target loss cannot transfer a proc to a new swing.");
    }

    private static int FindJoeProcSeed(bool accepted)
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            UnityEngine.Random.InitState(seed);
            if (Defined.CanProbability(15f) == accepted) return seed;
        }
        throw new InvalidOperationException("Failed to find the isolated deterministic Joe chance fixture.");
    }

    private void BeginJoeSwing(Joe joe, int seed, bool armed)
    {
        joe.stat.CriticalActuationProbability = 0;
        UnityEngine.Random.InitState(seed);
        joe.BeginAttack();
        Require(joe.IsAttackInProgress && joe.Animation.AnimationState.GetCurrent(0)?.Animation.Name == "attack" &&
            joe.isStunAttack == armed && (armed ? joe.nextStunDuring == 2f : joe.nextStunDuring == 0f),
            "Joe did not arm the original 15-percent two-second proc exclusively for the tested native swing.");
    }

    private IEnumerator BuffContracts()
    {
        yield return JoeBasicProcOwnership();
        if (!Includes("buffs", "durations-overlap-damage-and-interrupted-reuse")) yield break;
        yield return EnsureFixtureLobby();
        BeginCase("buffs", "durations-overlap-damage-and-interrupted-reuse");
        float oldScale = Time.timeScale;
        HeroFixture fixture = null;
        try
        {
            Time.timeScale = 4f;
            yield return CreateHeroFixture("Pochi", eUltState.STAR3, value => fixture = value);
            UnitLogic target = fixture.Soldier;
            float speed = target.coefficient.AttackSpeed, move = target.coefficient.MoveSpeed;
            target.RoenSkill(3f); target.TakeBerserkMode(5f);
            Require(Mathf.Abs(target.coefficient.AttackSpeed - speed - 0.9f) < 0.001f, "Overlapping speed buffs did not combine additively.");
            yield return new WaitForSeconds(3.3f);
            Require(Mathf.Abs(target.coefficient.AttackSpeed - speed - 0.5f) < 0.001f && target.IsBerserk, "Roen expiry removed or leaked the overlapping berserk modifier.");
            yield return new WaitForSeconds(2.1f);
            Require(!target.IsBerserk && Mathf.Abs(target.coefficient.AttackSpeed - speed) < 0.001f && Mathf.Abs(target.coefficient.MoveSpeed - move) < 0.001f, "Timed speed/movement modifiers did not restore their baseline.");

            fixture.Hero.Skill();
            float barrierStarted = Time.time;
            Require(target.HasPochiBarrier, "Pochi STAR3 ability did not apply its barrier.");
            target.stat.Armour = 0f;
            float targetHp = target.stat.Hp;
            target.Attack(fixture.Enemy.gameObject, 100f);
            Require(target.stat.Hp == targetHp, "Barrier allowed normal damage.");
            yield return new WaitForSeconds(4.8f);
            Require(target.HasPochiBarrier, "STAR3 barrier expired before its authored five seconds.");
            yield return Until(() => !target.HasPochiBarrier, "Barrier natural expiry", 5f);
            Require(Time.time - barrierStarted >= 5f && Time.time - barrierStarted < 5.6f, "STAR3 barrier duration differs from five game seconds.");

            target.stat.Armour = 0f; target.stat.AttackAvoidance = target.stat.SkillAvoidance = target.stat.DebuffAvoidance = 0f;
            float enemyHp = fixture.Enemy.stat.Hp;
            target.TakePochiBarrier(1f, 0.3f);
            target.Attack(fixture.Enemy.gameObject, 100f);
            Require(Mathf.Abs((enemyHp - fixture.Enemy.stat.Hp) - 30f) <= 1f, "Barrier reflection did not apply 30% once without recursion.");
            yield return new WaitForSeconds(1.2f);

            target.Invincible(1f); targetHp = target.stat.Hp;
            target.Attack(fixture.Enemy.gameObject, 100f, false, true);
            Require(target.stat.Hp == targetHp, "Invincibility did not prevent actual skill damage.");
            yield return new WaitForSeconds(1.2f);
            Require(!target.IsInvincible && target.stat.SkillAvoidance < 100f, "Invincibility avoidance leaked after expiry.");
            target.stat.Armour = 0f;
            target.Attack(fixture.Enemy.gameObject, 100f, false, true);
            Require(target.stat.Hp < targetHp, "Damage remained blocked after invincibility ended.");

            for (int i = 0; i < 6; i++) target.Curse();
            Require(Mathf.Abs(target.CurseSeverity - 80f) < 0.001f, "Curse did not cap at 80 percentage points.");
            target.CleanseCurse(10f);
            Require(Mathf.Abs(target.CurseSeverity - 70f) < 0.001f, "Cleanse did not remove ten percentage points.");
            targetHp = target.stat.Hp;
            target.TakeBleeding(10f, 2f); target.TakePoisoning(10f, 2f);
            yield return new WaitForSeconds(2.4f);
            Require(!target.IsBleeding && !target.IsPoisoning && Mathf.Abs((targetHp - target.stat.Hp) - 40f) <= 1f, "Independent bleed/poison ticks or their natural expiry are incorrect.");

            target.TakeBerserkMode(5f); target.GrowUp(5f); target.Stealth(5f);
            yield return new WaitForSeconds(0.5f);
            int previousId = target.ID;
            target.SafeHide(); yield return null;
            var prefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), Vector3.zero);
            data.RoundId = fixture.Round; data.InitialMode = UnitSpawnMode.Staged;
            UnitLogic reused = null;
            yield return ShowFixture<UnitLogic>(prefab, data, actor => reused = actor);
            Require(reused.ID != previousId && LunaSpawn.Find<UnitLogic>(previousId) == null, "Old buff target identity still resolves.");
            Require(!reused.IsBerserk && !reused.IsStealthed && !reused.HasPochiBarrier && reused.CurseSeverity == 0f && !reused.IsBleeding && !reused.IsPoisoning, "Pooled unit retained interrupted buff/debuff flags.");
            Require(Vector3.Distance(reused.transform.localScale, prefab.transform.localScale) < 0.001f && Mathf.Abs(reused.coefficient.AttackSpeed - 1f) < 0.001f, "Pooled unit retained modified scale or coefficients.");
            current.observations.Add("Overlapping 3s/5s modifiers, STAR3 5s barrier, 30% reflection, invincibility, 4 DOT ticks, curse cap/cleanse and interrupted reuse verified.");
        }
        finally { if (fixture != null) LunaSpawn.CancelRound(fixture.Round); Time.timeScale = oldScale; }
        if (fixture != null) yield return RetireFixture(fixture.Round);
        EndCase("Timed buffs, actual hit prevention/reflection, damage-over-time, curse cleanup and pooled interruption state matched explicit gameplay contracts.");
    }
}
#endif

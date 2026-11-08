#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    // These cases advance actual scaled game time. They never modify a private timer,
    // lifetime, rank implementation flag, or periodic callback.
    private IEnumerator HeroScheduleContracts()
    {
        yield return EnsureFixtureLobby();
        float originalScale = Time.timeScale;
        var random = UnityEngine.Random.state;
        try
        {
            foreach (string name in new[] { "Ria", "Lena", "Matilda", "Clare", "Tira", "Joe", "Lene" })
            {
                eUltState[] ranks = name == "Ria" ? new[] { eUltState.STAR1, eUltState.ULT1, eUltState.ULT2 }
                    : name == "Matilda" ? new[] { eUltState.STAR4, eUltState.STAR5, eUltState.ULT1, eUltState.ULT2 }
                    : name == "Tira" ? new[] { eUltState.STAR4, eUltState.STAR5 }
                    : new[] { eUltState.ULT1, eUltState.ULT2 };
                foreach (eUltState rank in ranks)
                {
                    string label = "schedule-" + name + "/" + rank;
                    if (!Includes("heroes", label)) continue;
                    BeginCase("heroes", label);
                    // Resource setup at normal speed avoids counting asynchronous fixture loading
                    // as dozens of seconds of Joe's real coroutine lifetime.
                    Time.timeScale = 1f;
                    HeroFixture fixture = null;
                    var source = CaptureScheduleSources(name);
                    yield return CreateHeroFixture(name, rank, value => fixture = value);
                    try
                    {
                        UnityEngine.Random.InitState(19483);
                        Time.timeScale = 8f;
                        bool summoner = name == "Ria" || name == "Lena" || name == "Matilda";
                        if (summoner)
                        {
                            // No enemy can detonate a bomb or kill a timed summon during its
                            // lifetime measurement. The normal staged-target predicate does this.
                            fixture.Enemy.Data.InitialMode = UnitSpawnMode.Staged;
                            fixture.Enemy.unitState.IsLobbyMove = true;
                        }
                        switch (name)
                        {
                            case "Ria": yield return RiaSchedule(fixture, rank); break;
                            case "Lena": yield return LenaSchedule(fixture, rank); break;
                            case "Matilda": yield return MatildaSchedule(fixture, rank); break;
                            case "Clare": yield return ClareSchedule(fixture, rank); break;
                            case "Tira": yield return TiraSchedule(fixture, rank); break;
                            case "Joe": yield return JoeSchedule(fixture, rank); break;
                            case "Lene": yield return LeneSchedule(fixture, rank); break;
                        }
                        foreach (var pair in source)
                            Require(JsonUtility.ToJson(pair.Key) == pair.Value, "Scheduled ability changed authored data/bindings: " + pair.Key.name);
                        int owner = fixture.Hero.ID;
                        // Alternate actual death and explicit scene-style retirement. Both must
                        // cancel scheduled activity and every pending/live child of this life.
                        if (rank == eUltState.ULT2)
                        {
                            fixture.Hero.stat.Hp = 0f;
                            fixture.Hero.BeginDeath();
                            yield return Until(() => UnitLogic.Find(owner) == null, "Scheduled hero real death retirement", 15f);
                        }
                        else fixture.Hero.SafeHide();
                        yield return Until(() => !Entities().Any(actor => actor.SpawnData != null && actor.SpawnData.OwnerId == owner)
                            && LunaSpawn.PendingCount == 0, "Scheduled hero owned cleanup", 15f);
                        float afterDeathHp = fixture.Enemy.stat.Hp;
                        yield return ScheduleWait(fixture, 4f, _ => Require(!Entities().Any(actor => actor.SpawnData != null && actor.SpawnData.OwnerId == owner),
                            "Retired hero created a new owned entity from a surviving timer."), false);
                        Require(Mathf.Abs(fixture.Enemy.stat.Hp - afterDeathHp) < 0.01f, "Retired hero's delayed ability still damaged its target.");
                    }
                    finally { LunaSpawn.CancelRound(fixture.Round); }
                    yield return RetireFixture(fixture.Round);
                    EndCase("Natural native timers, below-rank gates, observable duration/repetition, immutable authored sources and per-life cleanup.");
                }
            }
        }
        finally { Time.timeScale = originalScale; UnityEngine.Random.state = random; }
    }

    private static Dictionary<UnityEngine.Object, string> CaptureScheduleSources(string name)
    {
        var sources = new Dictionary<UnityEngine.Object, string>();
        var view = Resources.Load<GameObject>("Prefabs/01Heroes/" + name).GetComponent<UnitView>();
        Action<UnitView> capture = binding =>
        {
            if (binding == null) return;
            sources[binding] = JsonUtility.ToJson(binding);
            if (binding.Definition != null) sources[binding.Definition] = JsonUtility.ToJson(binding.Definition);
        };
        capture(view);
        foreach (var binding in view.Prefabs)
            if (binding.Value != null) capture(binding.Value.GetComponent<UnitView>());
        return sources;
    }

    private static UnitLogic[] ScheduledChildren(HeroFixture fixture, GameObject prefab = null)
    {
        string key = prefab != null ? prefab.GetComponent<UnitView>().ParameterKey : null;
        int owner = fixture.Hero.ID;
        return UnitLogic.ActiveUnits.Where(actor => actor != null && actor.Data != null && !actor.IsHiding
            && actor.Data.OwnerId == owner && actor.Data.RoundId == fixture.Round
            && (prefab == null || actor.View.ParameterKey == key)).ToArray();
    }

    private void ObserveSchedule(HeroFixture fixture, bool requireOwner = true)
    {
        // Retirement assertions use the captured original ID in their caller; the pooled
        // object must no longer be read as the identity of that retired life.
        if (!requireOwner) return;
        Require(fixture.Hero.Data != null && fixture.Hero.stat.Hp > 0f && fixture.Hero.IsRankReady,
            "Schedule owner died or lost initialized rank before its timer completed.");
        foreach (UnitLogic child in ScheduledChildren(fixture))
        {
            Require(child.Data.RoundId == fixture.Round && child.UnitSide == eUnitSide.Allied && child.ID > 0,
                "Scheduled summon lost typed round/owner/side identity.");
            Require(child.Data.Definition != child.View.Definition && Finite(child.stat.Hp) && Finite(child.stat.Mp),
                "Scheduled summon reused its immutable definition or produced non-finite state.");
            // Prevent ordinary combat from obscuring cap/lifetime observations. This leaves
            // native FSMs, summon motion, periodic updates and expiry running normally.
            if (!child.unitState.IsActionDelay || child.stat.AttackDelayTime > -500f) HoldAutomaticActions(child);
        }
    }

    private IEnumerator ScheduleWait(HeroFixture fixture, float seconds, Action<float> sample = null, bool requireOwner = true)
    {
        float start = Time.time, realStart = Time.realtimeSinceStartup;
        while (Time.time - start < seconds)
        {
            ObserveSchedule(fixture, requireOwner);
            sample?.Invoke(Time.time - start);
            Require(Time.realtimeSinceStartup - realStart < seconds + 15f, "Scaled schedule time stopped advancing.");
            yield return null;
        }
        ObserveSchedule(fixture, requireOwner);
        sample?.Invoke(Time.time - start);
    }

    private IEnumerator ScheduleUntil(HeroFixture fixture, Func<bool> condition, float gameSeconds, string label)
    {
        float start = Time.time;
        while (!condition())
        {
            ObserveSchedule(fixture);
            Require(Time.time - start <= gameSeconds, label + " did not occur within " + gameSeconds + " game seconds.");
            yield return null;
        }
        ObserveSchedule(fixture);
    }

    private IEnumerator RiaSchedule(HeroFixture fixture, eUltState rank)
    {
        var ria = (Ria)fixture.Hero;
        float started = Time.time;
        int normalCap = rank >= eUltState.ULT1 ? 3 : 2;
        yield return ScheduleWait(fixture, 5f, _ => Require(ScheduledChildren(fixture, ria.NormalMinion).Length == 0,
            "Ria normal skeleton appeared before its six-second interval."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, ria.NormalMinion).Length == normalCap, 3f, "Ria normal batch");
        var firstNormal = ScheduledChildren(fixture, ria.NormalMinion).Select(actor => actor.ID).ToArray();
        yield return ScheduleWait(fixture, 7f, _ => Require(ScheduledChildren(fixture, ria.NormalMinion).Select(actor => actor.ID).OrderBy(id => id)
            .SequenceEqual(firstNormal.OrderBy(id => id)), "Ria duplicated/replaced living capped skeletons."));
        UnitLogic.Find(firstNormal[0]).SafeHide();
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, ria.NormalMinion).Length == normalCap, 7f, "Ria natural replenishment");
        Require(ScheduledChildren(fixture, ria.NormalMinion).All(actor => actor.ID != firstNormal[0]), "Ria replenished an old native identity.");
        // The king is independently gated at ULT2; normal summons continue at every rank.
        if (rank < eUltState.ULT2)
        {
            yield return ScheduleWait(fixture, 35f, _ =>
            {
                Require(ScheduledChildren(fixture, ria.UltMinion).Length == 0, "Ria king unlocked below ULT2.");
                Require(ScheduledChildren(fixture, ria.NormalMinion).Length <= normalCap, "Ria normal cap was exceeded.");
            });
            current.observations.Add("Normal skeleton cap=" + normalCap + "; replenished after retirement; no king across >33s at " + rank);
            yield break;
        }
        yield return ScheduleWait(fixture, Mathf.Max(0f, 31f - (Time.time - started)), _ => Require(ScheduledChildren(fixture, ria.UltMinion).Length == 0,
            "Ria king appeared before its thirty-three-second threshold."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, ria.UltMinion).Length == 1, 5f, "Ria 33-second king");
        Require(Time.time - started >= 31f && Time.time - started <= 36f, "Ria first king interval differs from thirty-three seconds.");
        UnitLogic king = ScheduledChildren(fixture, ria.UltMinion).Single();
        int firstId = king.ID; float birth = Time.time;
        Require(Mathf.Abs(king.Data.Lifetime - 60f) < 0.001f, "Ria king request did not declare sixty-second lifetime.");
        yield return ScheduleWait(fixture, 58.5f, _ => Require(UnitLogic.Find(firstId) != null && ScheduledChildren(fixture, ria.UltMinion).Length == 1,
            "Ria king expired early or duplicated while alive."));
        yield return ScheduleUntil(fixture, () => UnitLogic.Find(firstId) == null, 3f, "Ria king natural sixty-second expiry");
        float lifetime = Time.time - birth;
        Require(lifetime >= 58.5f && lifetime <= 61.5f, "Ria observed lifetime differs from sixty seconds: " + lifetime);
        yield return ScheduleWait(fixture, 31f, _ => Require(ScheduledChildren(fixture, ria.UltMinion).Length == 0,
            "Ria cooldown ran while the preceding king was alive."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, ria.UltMinion).Length == 1, 5f, "Ria king full cooldown after expiry");
        Require(ScheduledChildren(fixture, ria.UltMinion).Single().ID != firstId, "Ria king reused a retired spawn identity.");
        current.observations.Add("King lifetime=" + lifetime + "s; one live king; natural expiry then complete 33s cooldown and fresh ID.");
    }

    private IEnumerator LenaSchedule(HeroFixture fixture, eUltState rank)
    {
        var lena = (Lena)fixture.Hero;
        float started = Time.time;
        yield return ScheduleWait(fixture, 3f, _ => Require(ScheduledChildren(fixture, lena.NormalMinion).Length == 0,
            "Lena normal slime appeared before four seconds."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, lena.NormalMinion).Length == 2, 3f, "Lena normal slime batch");
        if (rank < eUltState.ULT2)
        {
            yield return ScheduleWait(fixture, 26f, _ =>
            {
                Require(ScheduledChildren(fixture, lena.UltMinion).Length == 0, "Lena king unlocked below ULT2.");
                Require(ScheduledChildren(fixture, lena.NormalMinion).Length == 2, "Lena duplicated its two living normal slimes.");
            });
            current.observations.Add("Two normal slimes; no king over two twelve-second intervals below ULT2.");
            yield break;
        }
        yield return ScheduleWait(fixture, Mathf.Max(0f, 10.5f - (Time.time - started)), _ => Require(ScheduledChildren(fixture, lena.UltMinion).Length == 0,
            "Lena king appeared before its twelve-second threshold."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, lena.UltMinion).Length == 1, 4f, "Lena twelve-second king");
        var king = ScheduledChildren(fixture, lena.UltMinion).Single(); int firstId = king.ID;
        Require(king.Data.Lifetime == 0f, "Lena king must live until death/owner retirement, without invented expiry.");
        yield return ScheduleWait(fixture, 26f, _ => Require(ScheduledChildren(fixture, lena.UltMinion).Length == 1 && UnitLogic.Find(firstId) != null,
            "Lena king expired or duplicated while living through two further intervals."));
        king.stat.Hp = 0f; king.BeginDeath();
        yield return ScheduleUntil(fixture, () => UnitLogic.Find(firstId) == null, 8f, "Lena king actual death");
        yield return ScheduleWait(fixture, 10.5f, _ => Require(ScheduledChildren(fixture, lena.UltMinion).Length == 0,
            "Lena replacement did not wait a complete absence cooldown."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, lena.UltMinion).Length == 1, 4f, "Lena replacement after twelve seconds");
        Require(ScheduledChildren(fixture, lena.UltMinion).Single().ID != firstId, "Lena replacement retained the dead king's ID.");
        current.observations.Add("King Lifetime=0; one live ID across >24s; actual death followed by full12s replacement cooldown.");
    }

    private IEnumerator MatildaSchedule(HeroFixture fixture, eUltState rank)
    {
        var matilda = (Matilda)fixture.Hero;
        if (rank < eUltState.STAR5)
        {
            yield return ScheduleWait(fixture, 32f, _ => Require(ScheduledChildren(fixture).Length == 0,
                "Matilda periodic fire/Martin appeared below their rank thresholds."));
            current.observations.Add("No automatic fire or Martin below STAR5 across 32s.");
            yield break;
        }
        yield return ScheduleWait(fixture, 11.5f, _ => Require(ScheduledChildren(fixture, matilda.minion2).Length == 0,
            "Matilda fire batch appeared before thirteen seconds."));
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, matilda.minion2).Length == 2, 4f, "Matilda two-fire batch");
        UnitLogic[] firstFire = ScheduledChildren(fixture, matilda.minion2); int[] firstFireIds = firstFire.Select(actor => actor.ID).ToArray();
        float fireBirth = Time.time;
        foreach (var fire in firstFire)
        {
            Require(Mathf.Abs(fire.Data.Lifetime - 30f) < 0.001f, "Matilda fire request did not declare thirty-second lifetime.");
            float hp = fire.stat.Hp;
            fire.Attack(fixture.Enemy.gameObject, 25f, false, true);
            Require(rank >= eUltState.ULT1 ? Mathf.Abs(fire.stat.Hp - hp) < 0.01f : fire.stat.Hp < hp,
                "Matilda fire invulnerability does not match ULT1 gate.");
        }
        if (rank < eUltState.ULT2)
        {
            yield return ScheduleWait(fixture, 20f, _ => Require(ScheduledChildren(fixture, matilda.minion3).Length == 0,
                "Matilda Martin appeared below ULT2."));
            Require(ScheduledChildren(fixture, matilda.minion2).Length >= 4, "Matilda failed to repeat the fire batch at twenty-six seconds.");
            current.observations.Add("Two fire per13s; declared lifetime30; ULT1 invulnerability gate observed; no Martin past30s.");
            yield break;
        }
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, matilda.minion2).Length == 4, 15f, "Matilda second fire batch");
        Require(ScheduledChildren(fixture, matilda.minion2).Select(actor => actor.ID).Distinct().Count() == 4, "Matilda fire batches shared native identities.");
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, matilda.minion3).Length == 1, 7f, "Matilda thirty-second Martin");
        UnitLogic martin = ScheduledChildren(fixture, matilda.minion3).Single(); int martinId = martin.ID; float martinBirth = Time.time;
        Require(Mathf.Abs(martin.Data.Lifetime - 16f) < 0.001f, "Matilda Martin request did not declare sixteen-second lifetime.");
        var fireExpiry = new Dictionary<int, float>();
        yield return ScheduleWait(fixture, 14.5f, _ =>
        {
            Require(UnitLogic.Find(martinId) != null && ScheduledChildren(fixture, matilda.minion3).Length == 1,
                "Matilda Martin expired early or duplicated.");
            foreach (int id in firstFireIds)
                if (UnitLogic.Find(id) == null && !fireExpiry.ContainsKey(id)) fireExpiry.Add(id, Time.time - fireBirth);
            if (Time.time - fireBirth < 28.5f) Require(fireExpiry.Count == 0, "Matilda fire expired before its thirty-second lifetime.");
        });
        Require(firstFireIds.All(id => UnitLogic.Find(id) == null), "Matilda first fire batch did not naturally expire after thirty seconds.");
        Require(Time.time - fireBirth >= 30f, "Fire expiry was checked before its full lifetime.");
        Require(fireExpiry.Count == 2 && fireExpiry.Values.All(value => value >= 28.5f && value <= 31.5f),
            "Observed fire lifetimes differ from thirty seconds.");
        yield return ScheduleUntil(fixture, () => UnitLogic.Find(martinId) == null, 3f, "Matilda Martin sixteen-second expiry");
        float martinLifetime = Time.time - martinBirth;
        Require(martinLifetime >= 14.5f && martinLifetime <= 17.5f, "Observed Martin lifetime differs from sixteen seconds.");
        yield return ScheduleUntil(fixture, () => ScheduledChildren(fixture, matilda.minion3).Length == 1, 17f, "Matilda next thirty-second Martin");
        Require(ScheduledChildren(fixture, matilda.minion3).Single().ID != martinId, "Matilda repeated Martin reused a retired ID.");
        current.observations.Add("Fire batches2 at13/26s with actual30s expiry; Martin at30s, actual lifetime=" + martinLifetime + "s, fresh next ID at60s.");
    }

    private IEnumerator ClareSchedule(HeroFixture fixture, eUltState rank)
    {
        if (rank < eUltState.ULT2)
        {
            yield return ScheduleWait(fixture, 15f, _ => Require(!fixture.Hero.IsStealthed, "Clare stealth unlocked below ULT2."));
            yield break;
        }
        float attackAvoid = fixture.Hero.stat.AttackAvoidance, skillAvoid = fixture.Hero.stat.SkillAvoidance, debuffAvoid = fixture.Hero.stat.DebuffAvoidance;
        yield return ScheduleWait(fixture, 11.5f, _ => Require(!fixture.Hero.IsStealthed, "Clare stealth started before thirteen seconds."));
        yield return ScheduleUntil(fixture, () => fixture.Hero.IsStealthed, 4f, "Clare periodic stealth");
        float started = Time.time;
        Require(fixture.Hero.stat.AttackAvoidance >= attackAvoid + 99.9f && fixture.Hero.stat.SkillAvoidance >= skillAvoid + 99.9f,
            "Clare's visible stealth flag did not apply avoidance.");
        yield return ScheduleWait(fixture, 2.5f, _ => Require(fixture.Hero.IsStealthed, "Clare stealth ended before3.5s."));
        yield return ScheduleUntil(fixture, () => !fixture.Hero.IsStealthed, 2f, "Clare stealth expiry");
        Require(Mathf.Abs(fixture.Hero.stat.AttackAvoidance - attackAvoid) < 0.01f && Mathf.Abs(fixture.Hero.stat.SkillAvoidance - skillAvoid) < 0.01f
            && Mathf.Abs(fixture.Hero.stat.DebuffAvoidance - debuffAvoid) < 0.01f, "Clare stealth did not restore exact avoidance values.");
        yield return ScheduleUntil(fixture, () => fixture.Hero.IsStealthed, 11f, "Clare repeated stealth");
        Require(Time.time - started >= 12f && Time.time - started <= 14.5f, "Clare repeated stealth interval is not thirteen seconds.");
        current.observations.Add("Natural13s stealth cadence;3.5s duration and exact avoidance restoration; repeated second activation.");
    }

    private IEnumerator TiraSchedule(HeroFixture fixture, eUltState rank)
    {
        Vector3 scale = fixture.Enemy.transform.localScale;
        if (rank < eUltState.STAR5)
        {
            yield return ScheduleWait(fixture, 22f, _ => Require(Vector3.Distance(fixture.Enemy.transform.localScale, scale) < 0.001f,
                "Tira shrink unlocked below STAR5."));
            yield break;
        }
        yield return ScheduleWait(fixture, 18.5f, _ => Require(Vector3.Distance(fixture.Enemy.transform.localScale, scale) < 0.001f,
            "Tira shrink started before twenty seconds."));
        yield return ScheduleUntil(fixture, () => fixture.Enemy.transform.localScale.x <= scale.x * 0.66f, 4f, "Tira periodic shrink");
        float started = Time.time;
        yield return ScheduleWait(fixture, 2.5f, _ => Require(fixture.Enemy.transform.localScale.x <= scale.x * 0.66f,
            "Tira shrink ended before its four-second duration."));
        yield return ScheduleUntil(fixture, () => Vector3.Distance(fixture.Enemy.transform.localScale, scale) < 0.001f, 3f, "Tira shrink exact restoration");
        yield return ScheduleUntil(fixture, () => fixture.Enemy.transform.localScale.x <= scale.x * 0.66f, 18f, "Tira repeated shrink");
        Require(Time.time - started >= 18.5f && Time.time - started <= 21.5f, "Tira repeated shrink interval differs from twenty seconds.");
        current.observations.Add("Natural20s cadence,0.65 scale for4s then authored scale restored, followed by repeat.");
    }

    private IEnumerator JoeSchedule(HeroFixture fixture, eUltState rank)
    {
        if (rank < eUltState.ULT2)
        {
            yield return ScheduleWait(fixture, 22f, _ => Require(!fixture.Hero.IsBerserk, "Joe periodic berserk unlocked below ULT2."));
            yield break;
        }
        float speed = fixture.Hero.coefficient.AttackSpeed, move = fixture.Hero.coefficient.MoveSpeed;
        yield return ScheduleUntil(fixture, () => fixture.Hero.IsBerserk, 21f, "Joe twenty-second berserk");
        float started = Time.time;
        Require(Mathf.Abs(fixture.Hero.coefficient.AttackSpeed - speed - 0.5f) < 0.001f
            && Mathf.Abs(fixture.Hero.coefficient.MoveSpeed - move - 1f) < 0.001f, "Joe periodic berserk did not apply authored coefficients.");
        yield return ScheduleWait(fixture, 3f, _ => Require(fixture.Hero.IsBerserk, "Joe berserk ended before four seconds."));
        yield return ScheduleUntil(fixture, () => !fixture.Hero.IsBerserk, 2f, "Joe berserk expiry");
        Require(Mathf.Abs(fixture.Hero.coefficient.AttackSpeed - speed) < 0.001f && Mathf.Abs(fixture.Hero.coefficient.MoveSpeed - move) < 0.001f,
            "Joe periodic berserk left coefficient residue.");
        yield return ScheduleUntil(fixture, () => fixture.Hero.IsBerserk, 18f, "Joe repeated twenty-second berserk");
        Require(Time.time - started >= 19f && Time.time - started <= 21.5f, "Joe repeated berserk interval differs from twenty seconds.");
        current.observations.Add("Actual20s coroutine cadence,4s berserk, exact coefficient restoration and repeat.");
    }

    private IEnumerator LeneSchedule(HeroFixture fixture, eUltState rank)
    {
        float hp = fixture.Enemy.stat.Hp;
        if (rank < eUltState.ULT2)
        {
            yield return ScheduleWait(fixture, 8f, _ => Require(Mathf.Abs(fixture.Enemy.stat.Hp - hp) < 0.01f,
                "Lene periodic strike unlocked below ULT2."));
            yield break;
        }
        float damage = fixture.Hero.stat.AttackDamage * 6f;
        yield return ScheduleWait(fixture, 2f, _ => Require(Mathf.Abs(fixture.Enemy.stat.Hp - hp) < 0.01f,
            "Lene periodic strike fired before its three-second timer."));
        for (int pulse = 1; pulse <= 3; pulse++)
        {
            int expectedPulse = pulse;
            yield return ScheduleUntil(fixture, () => hp - fixture.Enemy.stat.Hp >= damage * expectedPulse - 2f, 4f, "Lene natural periodic strike " + pulse);
            Require(Mathf.Abs(hp - fixture.Enemy.stat.Hp - damage * pulse) <= 2f,
                "Lene periodic strike duplicated or used a different damage multiplier.");
        }
        current.observations.Add("Three natural3s strikes, each6x current ATK, no duplicate damage.");
    }
}
#endif

#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private sealed class HeroFixture
    {
        public int Round;
        public HeroUnitLogic Hero, FriendlyHero;
        public UnitLogic Soldier, Enemy;
        public PlayerLogic Player;
        public Vector3 PlayerScale;
    }
    private static readonly string[] HeroNames = { "Alice", "Clare", "Dren", "Elena", "Goldy", "Joe", "Lena", "Lene", "Lennon", "Matilda", "Mei", "Miu", "Pobi", "Pochi", "Pring", "Raccon", "Ria", "Roen", "Roni", "Rose", "Scarlet", "Tira" };

    private IEnumerator EnsureFixtureLobby()
    {
        if (LunaScenes.CurrentScene != "Lobby" || LunaScenes.IsTransitioning)
        {
            LunaScenes.Load("Lobby");
            yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning && LunaScenes.CurrentGame == null, "Fixture lobby transition");
        }
        yield return Until(() => LunaSpawn.PendingCount == 0, "Outstanding native resource requests before fixture");
    }

    private IEnumerator ShowFixture<T>(GameObject prefab, LunaEntityData data, Action<T> accepted) where T : EntityLogicBase
    {
        T actor = null; string error = null;
        int id = data.Id;
        LunaSpawn.Show<T>(prefab, data, shown => actor = shown, failure => error = failure);
        yield return Until(() => actor != null || error != null, "Native fixture show " + (prefab != null ? prefab.name : "null"));
        Require(error == null && actor != null && actor.ID == id, "Native fixture show failed: " + error);
        accepted(actor);
    }

    private static void HoldAutomaticActions(UnitLogic actor)
    {
        // Stage release is an ordinary runtime state change. The native FSM remains alive;
        // its action-delay guard prevents unrelated basic attacks from obscuring the tested ability.
        actor.unitState.IsActionDelay = true; actor.stat.AttackDelayTime = -1000f;
        actor.stat.Mp = 0f; actor.Data.InitialMode = UnitSpawnMode.Battle; actor.unitState.IsLobbyMove = false;
        actor.stat.CriticalActuationProbability = 0;
    }

    private IEnumerator RetireFixture(int round)
    {
        LunaSpawn.CancelRound(round);
        yield return Until(() => !Entities().Any(actor => actor.SpawnData != null && actor.SpawnData.RoundId == round) && LunaSpawn.PendingCount == 0, "Fixture native retirement");
        yield return null;
        yield return Until(() => !Entities().Any(actor => actor.SpawnData != null && actor.SpawnData.RoundId == round) && LunaSpawn.PendingCount == 0, "Settled fixture retirement before data-pool balance");
        AssertEntityDataOwnershipBalance("retired-round-" + round);
    }

    private IEnumerator CreateHeroFixture(string name, eUltState rank, Action<HeroFixture> accepted)
    {
        var fixture = new HeroFixture { Round = NewFixtureRound() };
        var heroPrefab = Resources.Load<GameObject>("Prefabs/01Heroes/" + name);
        var heroData = HeroData.CreateFrom(heroPrefab.GetComponent<UnitView>(), Vector3.zero);
        heroData.RoundId = fixture.Round; heroData.Rank = rank; heroData.Level = 1;
        heroData.Side = eUnitSide.Allied; heroData.InitialMode = UnitSpawnMode.Staged; heroData.HealthMultiplier = 1000f;
        yield return ShowFixture<HeroUnitLogic>(heroPrefab, heroData, actor => fixture.Hero = actor);

        var friendPrefab = Resources.Load<GameObject>("Prefabs/01Heroes/Roni");
        var friendData = HeroData.CreateFrom(friendPrefab.GetComponent<UnitView>(), new Vector3(-0.7f, 0f));
        friendData.RoundId = fixture.Round; friendData.Rank = eUltState.STAR1; friendData.Level = 1;
        friendData.Side = eUnitSide.Allied; friendData.InitialMode = UnitSpawnMode.Staged; friendData.HealthMultiplier = 1000f;
        yield return ShowFixture<HeroUnitLogic>(friendPrefab, friendData, actor => fixture.FriendlyHero = actor);
        var soldierPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
        foreach (bool enemy in new[] { false, true })
        {
            var data = UnitData.CreateFrom(soldierPrefab.GetComponent<UnitView>(), new Vector3(enemy ? 1f : -1.3f, 0f));
            data.RoundId = fixture.Round; data.Level = 1; data.Side = enemy ? eUnitSide.Enemy : eUnitSide.Allied;
            data.InitialMode = UnitSpawnMode.Staged; data.HealthMultiplier = 10000f;
            yield return ShowFixture<UnitLogic>(soldierPrefab, data, actor => { if (enemy) fixture.Enemy = actor; else fixture.Soldier = actor; });
        }
        if (name == "Tira")
        {
            var prefab = LunaSession.Current.Players[0];
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), new Vector3(-2f, 0f));
            data.RoundId = fixture.Round; data.Side = eUnitSide.Allied; data.InitialMode = UnitSpawnMode.Staged;
            data.HealthMultiplier = 1000f;
            yield return ShowFixture<PlayerLogic>(prefab, data, actor => fixture.Player = actor);
            fixture.PlayerScale = fixture.Player.transform.localScale;
        }
        foreach (UnitLogic actor in UnitLogic.ActiveUnits.Where(actor => actor.Data.RoundId == fixture.Round).ToArray())
        {
            HoldAutomaticActions(actor);
            if (actor != fixture.Hero) actor.stat.AttackDamage = 0f;
            if (actor.UnitSide == eUnitSide.Allied) actor.stat.Hp = actor.HealthBarHpPoint * 0.5f;
        }
        fixture.Enemy.stat.Armour = fixture.Enemy.stat.SkillResistance = 0f;
        fixture.Enemy.stat.AttackAvoidance = fixture.Enemy.stat.SkillAvoidance = fixture.Enemy.stat.DebuffAvoidance = 0f;
        fixture.Hero.Data.TargetId = fixture.Enemy.ID;
        fixture.Hero.stat.AttackRange = Mathf.Max(2f, fixture.Hero.stat.AttackRange);
        HeroPartyEffects.Refresh();
        yield return null;
        accepted(fixture);
    }

    private sealed class AbilityObservation
    {
        public float EnemyHp, AllyHp, Speed, Gold, MaxGrowth = 1f;
        public bool Bleeding, Berserk, Barrier;
        public int MaxOwned;
        public readonly HashSet<int> SummonIds = new HashSet<int>();
        public void Sample(HeroFixture fixture)
        {
            EnemyHp = Mathf.Min(EnemyHp, fixture.Enemy.stat.Hp);
            AllyHp = Mathf.Max(AllyHp, fixture.FriendlyHero.stat.Hp + fixture.Soldier.stat.Hp + fixture.Hero.stat.Hp);
            Speed = Mathf.Max(Speed, fixture.Soldier.coefficient.AttackSpeed);
            Gold = Mathf.Max(Gold, HeroPartyEffects.GoldBonusForRound(fixture.Round));
            Bleeding |= fixture.Enemy.IsBleeding;
            Berserk |= fixture.Hero.IsBerserk || fixture.FriendlyHero.IsBerserk || fixture.Soldier.IsBerserk;
            Barrier |= fixture.Soldier.HasPochiBarrier;
            if (fixture.Player != null) MaxGrowth = Mathf.Max(MaxGrowth, fixture.Player.transform.localScale.x / fixture.PlayerScale.x);
            var owned = UnitLogic.ActiveUnits.Where(actor => actor.Data != null && actor.Data.OwnerId == fixture.Hero.ID && !actor.IsHiding).ToArray();
            MaxOwned = Mathf.Max(MaxOwned, owned.Length);
            foreach (var actor in owned) SummonIds.Add(actor.ID);
        }
    }

    private IEnumerator HeroContracts(bool reuseSweep = false)
    {
        yield return EnsureFixtureLobby();
        float originalScale = Time.timeScale;
        var randomState = UnityEngine.Random.state;
        try
        {
            Time.timeScale = 4f;
            foreach (string name in HeroNames)
            {
            string ultimateCoefficients = null;
            GameObject previousInstance = null;
            int previousSpawn = 0, cycle = 0;
            var ranks = reuseSweep ? new[] { eUltState.ULT2, eUltState.STAR1, eUltState.ULT2 } : (eUltState[])Enum.GetValues(typeof(eUltState));
            foreach (eUltState rank in ranks)
            {
                cycle++;
                string label = reuseSweep ? "reuse-" + name + "/" + cycle + "-" + rank : name + "/" + rank;
                if (!Includes("heroes", label)) continue;
                BeginCase("heroes", label);
                HeroFixture fixture = null;
                yield return CreateHeroFixture(name, rank, value => fixture = value);
                int spawnId = fixture.Hero.ID;
                try
                {
                    Require(fixture.Hero.IsRankReady && fixture.Hero.Data.Rank == rank, "Hero rank was not applied before native show completion.");
                    Require(GameEntry.GetComponent<UnityGameFramework.Runtime.FsmComponent>().HasFsm<UnitLogic>(fixture.Hero.ID.ToString()), "Native hero combat FSM is missing.");
                    if (reuseSweep)
                    {
                        var hero = fixture.Hero;
                        Require(!hero.unitState.IsDeath && !hero.IsBerserk && !hero.IsStealthed && !hero.IsInvincible && !hero.IsBleeding && !hero.IsPoisoning && hero.CurseSeverity == 0f,
                            "New ranked hero life retained death or timed status state.");
                        Require(!UnitLogic.ActiveUnits.Any(unit => unit.Data != null && unit.Data.OwnerId == spawnId), "Ranked hero life inherited an old summon identity before any skill activation.");
                        if (rank == eUltState.STAR1)
                        {
                            var c = hero.coefficient;
                            Require(Mathf.Abs(c.HP - 1000f) < 0.001f && Mathf.Abs(c.MP - 1f) < 0.001f && Mathf.Abs(c.Stamina - 1f) < 0.001f &&
                                Mathf.Abs(c.AttackDamage - 1f) < 0.001f && Mathf.Abs(c.SkillPoint - 1f) < 0.001f && Mathf.Abs(c.Armour - 1f) < 0.001f &&
                                Mathf.Abs(c.AttackSpeed - 1f) < 0.001f && Mathf.Abs(c.MoveSpeed - 1f) < 0.001f,
                                "STAR1 retained high-rank coefficients; only the fixture's explicit health multiplier1000 may differ from1: " + JsonUtility.ToJson(c));
                            Require(hero.stat.StunResistance == 0f && Mathf.Abs(hero.stat.SkillResistance - hero.objectData.SkillResistance) < 0.001f,
                                "STAR1 retained ultimate resistance modifiers.");
                        }
                        string coefficients = JsonUtility.ToJson(hero.coefficient);
                        if (cycle == 1) ultimateCoefficients = coefficients;
                        if (cycle == 3) Require(coefficients == ultimateCoefficients, "ULT2 coefficients changed after high→low→high reuse: first=" + ultimateCoefficients + ", repeated=" + coefficients);
                        GameObject instance = hero.gameObject;
                        Require(spawnId != previousSpawn && (previousSpawn == 0 || LunaSpawn.Find<HeroUnitLogic>(previousSpawn) == null), "Retired hero spawn identity resolves during a later rank cycle.");
                        current.observations.Add("Rank cycle=" + cycle + "; spawn=" + spawnId + "; physical instance=" + instance.GetEntityId() + "; reused previous physical instance=" + ReferenceEquals(instance, previousInstance) + "; coefficients=" + coefficients);
                        previousSpawn = spawnId; previousInstance = instance;
                    }
                    float beforeEnemy = fixture.Enemy.stat.Hp;
                    float beforeAllies = fixture.FriendlyHero.stat.Hp + fixture.Soldier.stat.Hp + fixture.Hero.stat.Hp;
                    float beforeSpeed = fixture.Soldier.coefficient.AttackSpeed;
                    float castAttack = fixture.Hero.stat.AttackDamage;
                    var observation = new AbilityObservation { EnemyHp = beforeEnemy, AllyHp = beforeAllies, Speed = beforeSpeed };
                    UnityEngine.Random.InitState(15031);
                    fixture.Hero.Skill();
                    // Chance-based abilities are sampled across bounded real casts, never inferred from a method name.
                    if (name == "Clare" || name == "Lene" || name == "Rose")
                        for (int attempt = 1; attempt < 20; attempt++) fixture.Hero.Skill();
                    float started = Time.time;
                    while (Time.time - started < 2.8f)
                    {
                        observation.Sample(fixture);
                        Require(Finite(fixture.Hero.stat.Hp) && Finite(fixture.Hero.stat.Mp), "Hero produced non-finite combat state.");
                        yield return null;
                    }
                    observation.Sample(fixture);
                    float damage = beforeEnemy - observation.EnemyHp;
                    float healing = observation.AllyHp - beforeAllies;
                    switch (name)
                    {
                        case "Alice": case "Dren": case "Joe": case "Mei": case "Scarlet": case "Raccon":
                            Require(damage > 0.1f, "Actual hero skill produced no target damage."); break;
                        case "Clare": Require(observation.Bleeding && damage > 0f, "Bleed skill never applied or ticked across 20 bounded chance rolls."); break;
                        case "Roni": case "Miu": case "Pring": Require(healing > 0.1f, "Healing skill did not restore wounded live allies."); break;
                        case "Lene": Require(observation.Berserk, "Berserk never applied across 20 bounded chance rolls."); break;
                        case "Rose": Require(observation.Berserk && observation.Bleeding, "Rose did not apply both ally berserk and enemy bleed across bounded casts."); break;
                        case "Pochi": Require(observation.Barrier, "Barrier did not protect a non-tower ally."); break;
                        case "Roen": Require(Mathf.Abs(observation.Speed - beforeSpeed - 0.4f) < 0.001f, "Roen did not apply the authored +0.4 attack-speed coefficient."); break;
                        case "Tira": Require(observation.MaxGrowth > 1.45f && observation.MaxGrowth <= 1.501f, "Player grow effect did not approach authored 1.5x scale."); break;
                        case "Goldy":
                            float expected = rank >= eUltState.ULT2 ? 10f : rank >= eUltState.ULT1 ? 9f : rank >= eUltState.STAR5 ? 8f : rank >= eUltState.STAR3 ? 7f : 6f;
                            Require(Mathf.Abs(observation.Gold - expected) < 0.001f, "Goldy passive gold-rate bonus differs from the authored rank table."); break;
                        case "Elena": Require(observation.MaxOwned == 1, "Elena did not own exactly one Dwarf."); break;
                        case "Ria": Require(observation.MaxOwned == (rank >= eUltState.ULT1 ? 3 : 2), "Ria archer batch differs from its rank contract."); break;
                        case "Lena": Require(observation.MaxOwned == 2, "Lena skill did not produce two hedgehogs."); break;
                        case "Matilda": Require(observation.SummonIds.Count == 4, "Matilda skill did not produce four separately owned mini Martins."); break;
                        case "Pobi": case "Lennon": Require(observation.MaxOwned >= 3 && observation.MaxOwned <= (rank >= eUltState.ULT1 ? 4 : 3), "Summon batch differs from its baseline or ultimate contract."); break;
                        default: throw new InvalidOperationException("Missing actual behavior oracle for " + name);
                    }
                    if (name == "Mei" && rank >= eUltState.ULT1) Require(fixture.Hero.stat.StunResistance >= 20f, "Mei ultimate stun resistance is missing.");
                    if (name == "Scarlet" && rank >= eUltState.ULT1) Require(fixture.Hero.stat.SkillResistance >= 0.2f, "Scarlet ultimate skill resistance is missing.");
                    // These multipliers are the reviewed authored/legacy balance contract, independent
                    // of private implementation fields. The fixture target has zero armour/resistance.
                    float expectedDamage = 0f;
                    switch (name)
                    {
                        case "Alice": expectedDamage = castAttack * (rank >= eUltState.ULT1 ? 27f : 9f) * (rank >= eUltState.STAR3 ? 1.05f : 1f); break;
                        case "Dren": expectedDamage = castAttack * 3f * (rank >= eUltState.STAR5 ? 1.2f : 1f); break;
                        case "Joe": expectedDamage = castAttack * 3f; break;
                        case "Mei": expectedDamage = castAttack * 5f * (rank >= eUltState.STAR3 ? 1.05f : 1f) * (rank >= eUltState.STAR5 ? 4f : 1f); break;
                        case "Scarlet": expectedDamage = castAttack * 40f * (rank >= eUltState.STAR3 ? 1.05f : 1f) * (rank >= eUltState.STAR5 ? 3f : 1f); break;
                    }
                    if (expectedDamage > 0f)
                    {
                        bool single = Mathf.Abs(damage - expectedDamage) <= Mathf.Max(2f, expectedDamage * 0.005f);
                        bool triple = name == "Alice" && rank >= eUltState.STAR5 && Mathf.Abs(damage - expectedDamage * 3f) <= Mathf.Max(2f, expectedDamage * 0.015f);
                        Require(single || triple, name + " damage contract: expected " + expectedDamage + (name == "Alice" ? " (or authored triple proc)" : "") + ", actual " + damage);
                    }
                    current.observations.Add("damage=" + damage + ", healing=" + healing + ", peakOwned=" + observation.MaxOwned + ", distinctOwned=" + observation.SummonIds.Count + ", goldRate=" + observation.Gold + ", growth=" + observation.MaxGrowth);
                    int ownerId = fixture.Hero.ID;
                    fixture.Hero.stat.Hp = 0f; fixture.Hero.BeginDeath();
                    yield return Until(() => !UnitLogic.ActiveUnits.Any(actor => actor.Data != null && actor.Data.OwnerId == ownerId) && LunaSpawn.PendingCount == 0, "Hero death-start owned summon cancellation", 15f);
                    current.observations.Add("Death-start cancelled owned pending/live summons before the death entity was recycled.");
                }
                finally { LunaSpawn.CancelRound(fixture.Round); }
                yield return RetireFixture(fixture.Round);
                if (reuseSweep) Require(!GameEntry.GetComponent<UnityGameFramework.Runtime.FsmComponent>().HasFsm<UnitLogic>(spawnId.ToString()), "Retired rank cycle kept its native combat FSM.");
                EndCase("Typed rank initialization, live native FSM, actual authored ability outcome, finite combat state and owner-death summon cleanup.");
            }
            }
        }
        finally { Time.timeScale = originalScale; UnityEngine.Random.state = randomState; }
    }
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
#endif

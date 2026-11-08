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
    private static Type CatalogType(LunaEntityCatalog.Entry entry) => Type.GetType(entry.LogicType + ", Assembly-CSharp");
    private static GameObject PassivePrefab => Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
    private static UnitData ControlledData(GameObject prefab, Vector3 position, int round, eUnitSide side)
    {
        var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), position);
        data.RoundId = round; data.Side = side; data.Level = 1; data.InitialMode = UnitSpawnMode.Staged;
        data.HealthMultiplier = 10000f;
        return data;
    }
    private static void NoDefence(UnitLogic unit)
    {
        unit.stat.Armour = unit.stat.AttackAvoidance = unit.stat.SkillAvoidance = unit.stat.DebuffAvoidance = unit.stat.SkillResistance = 0f;
    }
    private static void RequireFiniteActor(UnitLogic unit)
    {
        Require(unit.Data != null && unit.SpawnData == unit.Data && unit.ID == unit.Data.Id, "Actor lost its typed native data identity.");
        Require(Finite(unit.stat.Hp) && Finite(unit.stat.Mp) && Finite(unit.stat.MoveSpeed) && Finite(unit.stat.AttackDamage), "Actor produced non-finite stats.");
        Require(Finite(unit.transform.position.x) && Finite(unit.transform.position.y), "Actor produced non-finite coordinates.");
        Require(unit.transform.position.x >= Mathf.Min(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x) - 0.02f &&
            unit.transform.position.x <= Mathf.Max(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x) + 0.02f, "Actor escaped combat world bounds.");
    }
    private IEnumerator ActorContracts()
    {
        yield return EnsureFixtureLobby();
        float originalTime = Time.timeScale; var random = UnityEngine.Random.state;
        try
        {
            Time.timeScale = 4f;
            foreach (var entry in Resources.Load<LunaEntityCatalog>("Luna/EntityCatalog").Entries.OrderBy(value => value.Address))
            {
                Type type = CatalogType(entry);
                if (type == null || !typeof(UnitLogic).IsAssignableFrom(type) || typeof(HeroUnitLogic).IsAssignableFrom(type) ||
                    typeof(PlayerLogic).IsAssignableFrom(type) || typeof(TowerLogic).IsAssignableFrom(type)) continue;
                string name = entry.Address.Replace("Assets/Resources/Prefabs/", string.Empty).Replace(".prefab", string.Empty);
                if (!Includes("actors", name)) continue;
                BeginCase("actors", name);
                int round = NewFixtureRound(), otherRound = 0; UnitLogic actor = null, target = null, decoy = null;
                float decoyHp = 0f;
                var unboundEffects = new List<int>();
                var events = GameEntry.GetComponent<EventComponent>();
                EventHandler<GameFramework.Event.GameEventArgs> observeEffects = (sender, args) =>
                {
                    var shown = (ShowEntitySuccessEventArgs)args;
                    if (shown.Entity.Logic is EffectLogic effect && effect.SpawnData != null
                        && effect.SpawnData.OwnerId == 0 && effect.SpawnData.RoundId == 0) unboundEffects.Add(effect.ID);
                };
                events.Subscribe(ShowEntitySuccessEventArgs.EventId, observeEffects);
                try
                {
                    UnitView view = entry.Prefab.GetComponent<UnitView>();
                    string source = JsonUtility.ToJson(view.Definition);
                    eUnitSide side = view.Data.UnitSide;
                    float direction = side == eUnitSide.Enemy ? -1f : 1f;
                    yield return ShowFixture<UnitLogic>(PassivePrefab, ControlledData(PassivePrefab, new Vector3(direction * 0.45f, 0f), round,
                        side == eUnitSide.Enemy ? eUnitSide.Allied : eUnitSide.Enemy), value => target = value);
                    HoldAutomaticActions(target); target.stat.AttackDamage = 0f; NoDefence(target);
                    if (type == typeof(Bellerin) || type == typeof(Skeleton_ax_skill) || type == typeof(Rugani)
                        || type == typeof(Dekoo_skill) || type == typeof(Deus_skill) || type == typeof(Wolf)
                        || type == typeof(Skeleton_magicianLeader_skill))
                    {
                        otherRound = NewFixtureRound();
                        yield return ShowFixture<UnitLogic>(PassivePrefab, ControlledData(PassivePrefab, target.transform.position,
                            otherRound, target.UnitSide), value => decoy = value);
                        HoldAutomaticActions(decoy); decoy.stat.AttackDamage = 0f; NoDefence(decoy);
                        decoy.stat.Hp = decoyHp = 10000f;
                    }
                    var data = ControlledData(entry.Prefab, Vector3.zero, round, side); data.TargetId = target.ID;
                    yield return ShowFixture<UnitLogic>(entry.Prefab, data, value => actor = value);
                    int firstId = actor.ID; float originalMaxHp = actor.HealthBarHpPoint;
                    Require(actor.GetType() == type && actor.Target == target, "Catalog type or pre-Show target snapshot differs from its request.");
                    HoldAutomaticActions(actor);
                    actor.unitState.IsActionDelay = false; actor.stat.AttackDelayTime = 0f;
                    float before = target.stat.Hp;
                    if (actor is DwarfLogic)
                    {
                        yield return Until(() => actor.Animation.AnimationName == "defense", "Dwarf native defensive action", 8f);
                        NoDefence(actor); actor.stat.Hp = Mathf.Min(actor.stat.Hp, 10000f);
                        float hp = actor.stat.Hp; actor.Attack(target.gameObject, 23f);
                        Require(Mathf.Abs(hp - actor.stat.Hp - 23f) < 0.01f, "Defensive summon is not damageable before an explicit invincibility upgrade.");
                    }
                    else if (actor.stat.AttackDamage > 0f || actor is MiniMartinLogic)
                    {
                        yield return Until(() => target.stat.Hp < before, "Actual native attack/impact: " + name, 12f);
                        if (actor is EnemySkillLogic && (actor.stat.AttackRange <= 0f || actor.Animation == null
                            || actor.Animation.Skeleton.Data.FindAnimation("attack") == null))
                            current.observations.Add("Initial damage came from the naturally charged native skill; this actor has no normal attack range/animation.");
                    }
                    else
                    {
                        NoDefence(actor); float hp = actor.stat.Hp; actor.Attack(target.gameObject, 23f);
                        Require(actor.stat.Hp < hp, "Passive actor did not accept combat damage.");
                    }
                    if (actor.Available && !actor.IsHiding && actor.Data != null)
                    {
                        RequireFiniteActor(actor);
                        if (actor is EnemySkillLogic enemySkill)
                        {
                            HoldAutomaticActions(actor); actor.Data.TargetId = target.ID;
                            // Skill-only actors can satisfy the initial damage wait at their skill
                            // event, before that same track completes. Respect the real duplicate-
                            // cast guard instead of submitting a rejected overlapping request.
                            if (actor.Animation != null && actor.Animation.AnimationState.GetCurrent(0)?.Animation.Name == "skill")
                            {
                                yield return Until(() => actor.Animation.AnimationState.GetCurrent(0)?.Animation.Name != "skill",
                                    "Previous native skill track completion before a distinct request", 10f);
                                current.observations.Add("Waited for the previous skill track to finish before requesting a separate observable skill.");
                            }
                            float skillBefore = target.stat.Hp;
                            actor.BeginSkill();
                            if (actor is MartinKing king) yield return Until(() => king.IsCharging, "MartinKing real charge request", 10f);
                            else yield return Until(() => target.stat.Hp < skillBefore || target.IsBleeding || target.IsPoisoning || target.isStun,
                                "Actual enemy skill outcome: " + name, 15f);
                        }
                        actor.stat.Hp = 0f;
                        yield return Until(() => LunaSpawn.Find<UnitLogic>(firstId) == null, "Native actor death retirement", 12f);
                    }
                    yield return null;
                    var reusedData = ControlledData(entry.Prefab, Vector3.zero, round, side); reusedData.TargetId = target.ID;
                    UnitLogic reused = null;
                    yield return ShowFixture<UnitLogic>(entry.Prefab, reusedData, value => reused = value);
                    Require(reused.ID != firstId && reused.Data.TargetId == target.ID, "Reuse retained an old spawn identity/target.");
                    Require(Mathf.Abs(reused.stat.Hp - originalMaxHp) < 0.01f && !reused.unitState.IsDeath && !reused.IsBleeding && !reused.IsPoisoning,
                        "Reuse retained health or status from the completed life.");
                    if (reused is MartinKing reusedKing) Require(!reusedKing.IsCharging && reused.transform.Find("Sprite").gameObject.activeSelf,
                        "MartinKing reused a charged flag or hidden sprite.");
                    Require(JsonUtility.ToJson(view.Definition) == source, "Combat mutated the authored source definition.");
                    yield return Until(() => LunaSpawn.PendingCount == 0, "Actor effect requests before ownership assertions");
                    Require(unboundEffects.Count == 0, "Actor spawned ownerless effects outside its round: " + string.Join(",", unboundEffects));
                    if (decoy != null)
                    {
                        Require(Mathf.Abs(decoy.stat.Hp - decoyHp) < 0.01f && !decoy.IsBleeding && !decoy.IsPoisoning && !decoy.isStun,
                            "Actor area skill affected an identically positioned target from another round.");
                        current.observations.Add("Live other-round decoy remained undamaged and free of bleed/poison/stun.");
                    }
                    current.observations.Add("firstId=" + firstId + "; freshId=" + reused.ID + "; target damage=" + (before - target.stat.Hp));
                    CheckOwnership(); EndCase("Real native attack/skill or defensive damage response, completed death, fresh-life reset, immutable source and ownership checked.");
                }
                finally
                {
                    events.Unsubscribe(ShowEntitySuccessEventArgs.EventId, observeEffects);
                    LunaSpawn.CancelRound(round);
                    if (otherRound != 0) LunaSpawn.CancelRound(otherRound);
                }
                yield return RetireFixture(round);
                if (otherRound != 0) yield return RetireFixture(otherRound);
            }
            yield return BackdropContracts();
        }
        finally { Time.timeScale = originalTime; UnityEngine.Random.state = random; }
    }
    private IEnumerator BackdropContracts()
    {
        foreach (var prefab in Resources.LoadAll<GameObject>("Prefabs/08StageBackground").Where(value => value.GetComponent<LunaStageBackdropView>() != null).OrderBy(value => value.name))
        {
            if (!Includes("actors", "backdrop/" + prefab.name)) continue;
            BeginCase("actors", "backdrop/" + prefab.name); int round = NewFixtureRound();
            try
            {
                LunaStageBackdropLogic backdrop = null; var data = LunaEntityData.Create<LunaStageEntityData>(); data.RoundId = round;
                yield return ShowFixture<LunaStageBackdropLogic>(prefab, data, value => backdrop = value);
                var view = backdrop.GetComponent<LunaStageBackdropView>();
                Vector3 root = backdrop.transform.position;
                Vector3[] plates = view.ScrollingPlates.Select(value => value.localPosition).ToArray();
                Vector3[] demons = view.WanderingDemons.Select(value => value.Root.localPosition).ToArray();
                Vector3 doorClosed = view.Door != null ? view.Door.localPosition : Vector3.zero;
                if (view.Door != null) { backdrop.OpenDoor(); backdrop.OpenDoor(); }
                float begin = Time.time; int peak = 0; bool checkedDoorSpeed = false;
                while (Time.time - begin < 2f)
                {
                    peak = Mathf.Max(peak, Entities().Count(value => value.SpawnData.OwnerId == backdrop.ID));
                    if (view.Door != null)
                    {
                        Require(view.Door.localPosition.y >= doorClosed.y && view.Door.localPosition.y <= 2.5001f, "Lobby door moved outside its authored closed/open bounds.");
                        if (!checkedDoorSpeed && Time.time - begin >= 0.5f)
                        {
                            float expectedY = Mathf.Min(2.5f, doorClosed.y + 1.2f * (Time.time - begin));
                            Require(Mathf.Abs(view.Door.localPosition.y - expectedY) < 0.3f, "Repeated OpenDoor multiplied movement or ignored the original 60 FPS speed.");
                            checkedDoorSpeed = true;
                        }
                    }
                    yield return null;
                }
                Require(backdrop.transform.position == root, "Ambient scroll moved the whole stage root.");
                for (int i = 0; i < plates.Length; i++) Require(view.ScrollingPlates[i].localPosition.x < plates[i].x - 0.1f, "Authored scrolling plate did not move.");
                for (int i = 0; i < demons.Length; i++) Require(view.WanderingDemons[i].Root.localPosition.x > demons[i].x + 0.5f, "Authored background demon did not pace.");
                if (view.Emitters.Length > 0) Require(peak > 0, "Authored backdrop emitters created no owned native effects.");
                if (view.Door != null) Require(checkedDoorSpeed && backdrop.IsDoorOpen && Mathf.Abs(view.Door.localPosition.y - 2.5f) < 0.001f, "Native lobby door did not reach its authored open position.");
                int owner = backdrop.ID; backdrop.SafeHide();
                yield return Until(() => !Entities().Any(value => value.SpawnData.OwnerId == owner) && LunaSpawn.PendingCount == 0, "Backdrop-owned effect cleanup");
                if (view.Door != null)
                {
                    var reusedData = LunaEntityData.Create<LunaStageEntityData>(); reusedData.RoundId = round;
                    LunaStageBackdropLogic reused = null;
                    yield return ShowFixture<LunaStageBackdropLogic>(prefab, reusedData, value => reused = value);
                    Require(reused.ID != owner && !reused.IsDoorOpen && reused.GetComponent<LunaStageBackdropView>().Door.localPosition == doorClosed,
                        "Lobby backdrop reuse retained an open door or old identity.");
                }
                EndCase("Stationary world root, explicit ambient motion/emission, owned cleanup and (for lobby) idempotent bounded door motion plus closed-state reuse checked.");
            }
            finally { LunaSpawn.CancelRound(round); }
            yield return RetireFixture(round);
        }
    }
    private IEnumerator ProjectileContracts()
    {
        yield return EnsureFixtureLobby(); float originalTime = Time.timeScale;
        try
        {
            Time.timeScale = 4f;
            foreach (var entry in Resources.Load<LunaEntityCatalog>("Luna/EntityCatalog").Entries.OrderBy(value => value.Address))
            {
                Type type = CatalogType(entry);
                if (type == null || !typeof(ProjectileLogic).IsAssignableFrom(type) || !Includes("projectiles", entry.Prefab.name)) continue;
                BeginCase("projectiles", entry.Prefab.name); int round = NewFixtureRound();
                try
                {
                    UnitLogic shooter = null, target = null;
                    yield return ShowFixture<UnitLogic>(PassivePrefab, ControlledData(PassivePrefab, new Vector3(-2f, 0f), round, eUnitSide.Allied), value => shooter = value);
                    yield return ShowFixture<UnitLogic>(PassivePrefab, ControlledData(PassivePrefab, new Vector3(2f, 0f), round, eUnitSide.Enemy), value => target = value);
                    HoldAutomaticActions(shooter); HoldAutomaticActions(target); NoDefence(target);
                    float hp = target.stat.Hp;
                    var data = ProjectileData.Create(entry.Prefab, shooter.ID, target.ID, shooter.ProjecttileArrivalLocation.transform.position, 123f, roundId: round);
                    int shotId = data.Id;
                    LunaSpawn.Show<ProjectileLogic>(entry.Prefab, data);
                    yield return Until(() => LunaSpawn.PendingCount == 0 && LunaSpawn.Find<ProjectileLogic>(shotId) == null, "Projectile impact and retirement", 15f);
                    Require(Mathf.Abs(hp - target.stat.Hp - 123f) < 0.1f, "Projectile did not deliver its immutable damage snapshot exactly once.");
                    yield return new WaitForSeconds(0.4f);
                    Require(Mathf.Abs(hp - target.stat.Hp - 123f) < 0.1f, "Retired projectile hit twice.");
                    int retiredTargetId = target.ID;
                    var orphan = ProjectileData.Create(entry.Prefab, shooter.ID, target.ID, new Vector3(-7f, 2f), 777f, roundId: round);
                    int orphanId = orphan.Id;
                    LunaSpawn.Show<ProjectileLogic>(entry.Prefab, orphan);
                    target.SafeHide();
                    yield return null;
                    yield return ShowFixture<UnitLogic>(PassivePrefab, ControlledData(PassivePrefab, new Vector3(2f, 0f), round, eUnitSide.Enemy), value => target = value);
                    HoldAutomaticActions(target); NoDefence(target); float freshHp = target.stat.Hp;
                    Require(target.ID != retiredTargetId, "Projectile fixture reused a spawn identity.");
                    yield return Until(() => LunaSpawn.PendingCount == 0 && LunaSpawn.Find<ProjectileLogic>(orphanId) == null, "Projectile invalid target retirement", 15f);
                    Require(Mathf.Abs(target.stat.Hp - freshHp) < 0.01f, "Old projectile damaged a reused target object under a new ID.");
                    EndCase("Actual prefab hit exactly once; stale target identity could not hit its replacement life.");
                }
                finally { LunaSpawn.CancelRound(round); }
                yield return RetireFixture(round);
            }
        }
        finally { Time.timeScale = originalTime; }
    }
    private IEnumerator CombinationContracts()
    {
        yield return EnsureFixtureLobby(); float originalTime = Time.timeScale;
        string[][] parties = { new[] { "Lene", "Alice", "Tira" }, new[] { "Ria", "Lena", "Pochi" }, new[] { "Roen", "Rose", "Dren" } };
        try
        {
            Time.timeScale = 4f;
            foreach (var party in parties)
            {
                string label = string.Join("+", party) + "-vs-mixed-enemies";
                if (!Includes("combos", label)) continue;
                BeginCase("combos", label); int round = NewFixtureRound(); var actors = new List<UnitLogic>();
                try
                {
                    foreach (string name in party)
                    {
                        var prefab = Resources.Load<GameObject>("Prefabs/01Heroes/" + name);
                        var data = HeroData.CreateFrom(prefab.GetComponent<UnitView>(), new Vector3(-1f, 0f, actors.Count * 0.001f));
                        data.RoundId = round; data.Rank = eUltState.ULT2; data.Side = eUnitSide.Allied;
                        data.HealthMultiplier = 10f; data.InitialMode = UnitSpawnMode.Staged;
                        yield return ShowFixture<HeroUnitLogic>(prefab, data, value => actors.Add(value));
                    }
                    foreach (string name in new[] { "SkeletonSword", "SkeletonArcher", "PoisonSlime" })
                    {
                        var prefab = Resources.Load<GameObject>("Prefabs/04Monsters/" + name);
                        var data = ControlledData(prefab, new Vector3(1f, 0f, actors.Count * 0.001f), round, eUnitSide.Enemy);
                        data.HealthMultiplier = 10f;
                        yield return ShowFixture<UnitLogic>(prefab, data, value => actors.Add(value));
                    }
                    var initial = actors.ToDictionary(value => value.ID, value => value.stat.Hp);
                    foreach (var actor in actors)
                    {
                        actor.Data.InitialMode = UnitSpawnMode.Battle; actor.unitState.IsLobbyMove = false;
                        actor.stat.Mp = 100f;
                    }
                    float begin = Time.time; float measuredDamage = 0f;
                    while (Time.time - begin < 8f)
                    {
                        foreach (var actor in UnitLogic.ActiveUnits.Where(value => value.Data != null && value.Data.RoundId == round)) RequireFiniteActor(actor);
                        measuredDamage = initial.Sum(pair => pair.Value - (UnitLogic.Find(pair.Key)?.stat.Hp ?? 0f));
                        yield return null;
                    }
                    Require(measuredDamage > 0.1f, "Concurrent native combat produced no actual damage/death.");
                    current.observations.Add("Named party only; net measured damage=" + measuredDamage + "; native units including summons=" +
                        UnitLogic.ActiveUnits.Count(value => value.Data != null && value.Data.RoundId == round));
                    CheckOwnership(); EndCase("Three real heroes and three enemy types fought through their native FSMs; measured damage, bounds and ownership checked.");
                }
                finally { LunaSpawn.CancelRound(round); }
                yield return RetireFixture(round);
            }
        }
        finally { Time.timeScale = originalTime; }
    }
    private IEnumerator LoadDungeonFixture(string key)
    {
        yield return EnsureFixtureLobby();
        var prefab = Resources.Load<GameObject>("Prefabs/11Dungeons/" + key);
        Require(prefab != null, "Dungeon fixture asset is missing: " + key);
        LunaSession.Current.selectDungeon = prefab; LunaSession.Current.SelectDungeonString = key;
        LunaScenes.Load("Dungeon");
        yield return Until(() => LunaScenes.CurrentScene == "Dungeon" && !LunaScenes.IsTransitioning && LunaScenes.CurrentGame != null && LunaScenes.CurrentGame.StageKey == key, "Native dungeon entry: " + key);
        yield return Until(() => LunaScenes.CurrentGame.FailureMessage != null || (LunaScenes.CurrentGame.Player != null && !LunaScenes.CurrentGame.isStageStop), "Dungeon combat intro");
        Require(LunaScenes.CurrentGame.FailureMessage == null, "Dungeon failed: " + LunaScenes.CurrentGame.FailureMessage);
        HoldRoundActors(LunaScenes.CurrentGame);
    }
    private static void HoldRoundActors(LunaGame game)
    {
        foreach (var actor in UnitLogic.ActiveUnits.Where(value => value.Data != null && value.Data.RoundId == game.RoundId).ToArray())
        {
            HoldAutomaticActions(actor);
            if (actor.UnitSide == eUnitSide.Allied) actor.Invincible(0f);
        }
    }
    private IEnumerator CompleteEndlessFloor(EndlessDungeon game)
    {
        int previous = game.dungeonLevel;
        foreach (var enemy in UnitLogic.ActiveUnits.Where(value => value.Data != null && value.Data.RoundId == game.RoundId && value.UnitSide == eUnitSide.Enemy).ToArray())
            enemy.stat.Hp = 0f;
        yield return Until(() => game.dungeonLevel == previous + 1 && game.PhaseName == "Combat" && !game.isStageStop && game.PendingSpawnCount == 0,
            "Actual endless floor transition", 30f);
        HoldRoundActors(game);
        foreach (var unit in UnitLogic.ActiveUnits.Where(value => value.Data != null && value.Data.RoundId == game.RoundId)) RequireFiniteActor(unit);
    }
    private IEnumerator DungeonContracts()
    {
        float originalTime = Time.timeScale;
        try
        {
            Time.timeScale = 4f;
            foreach (var prefab in Resources.LoadAll<GameObject>("Prefabs/11Dungeons").Where(value => value.GetComponent<LunaStageDefinition>() != null).OrderBy(value => value.name))
            {
                if (!Includes("dungeons", prefab.name)) continue;
                BeginCase("dungeons", prefab.name);
                yield return LoadDungeonFixture(prefab.name);
                var game = LunaScenes.CurrentGame; int oldRound = game.RoundId;
                if (game is EndlessDungeon endless)
                {
                    yield return CompleteEndlessFloor(endless);
                    Require(endless.dungeonLevel == 1, "Endless transition did not commit exactly one completed floor.");
                    game.Player.stat.Hp = 0f;
                }
                else
                {
                    foreach (var enemy in UnitLogic.ActiveUnits.Where(value => value.Data != null && value.Data.RoundId == oldRound && value.UnitSide == eUnitSide.Enemy).ToArray()) enemy.stat.Hp = 0f;
                }
                yield return Until(() => Active<UIResult>() != null && game.isStageStop, "Normal dungeon result", 20f);
                Require(game.isStageWin == (game is BossDungeon), "Dungeon result outcome differs from actual boss defeat/player defeat.");
                Require(!Active<UIResult>().transform.Find("ImagePlate/NextStage").gameObject.activeSelf, "Dungeon result incorrectly exposes campaign next-floor navigation.");
                Require(!game.PlayerRespawnPending, "Completed dungeon retained a player respawn.");
                Active<UIResult>().ReStage();
                yield return Until(() => LunaScenes.CurrentGame != null && LunaScenes.CurrentGame.RoundId != oldRound &&
                    LunaScenes.CurrentScene == "Dungeon" && !LunaScenes.IsTransitioning, "Normal dungeon retry navigation");
                yield return Until(() => LunaScenes.CurrentGame.Player != null && !LunaScenes.CurrentGame.isStageStop, "Retried dungeon combat");
                var retry = LunaScenes.CurrentGame;
                Require(retry.StageKey == prefab.name, "Retry selected a different dungeon.");
                Require(!Entities().Any(value => value.SpawnData.RoundId == oldRound), "Retry retained the completed round's entities.");
                HoldRoundActors(retry); retry.Player.stat.Hp = 0f;
                yield return Until(() => Active<UIResult>() != null && retry.isStageStop, "Dungeon player defeat result", 20f);
                Require(!retry.isStageWin && !retry.PlayerRespawnPending, "Dungeon player defeat incorrectly entered campaign resurrection.");
                Active<UIResult>().ReleaseBt();
                yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning && LunaScenes.CurrentGame == null, "Dungeon result return to lobby");
                CheckOwnership(); EndCase("Actual intro, boss/floor completion, result, same-dungeon retry, player defeat and return navigation checked.");
            }
        }
        finally { Time.timeScale = originalTime; }
    }
    private static Dictionary<int, int> ItemCounts() => LunaDataTable.Instance.itemTable.ToDictionary(pair => pair.Key, pair => pair.Value.itemCount);
    private static int InventoryDelta(Dictionary<int, int> before) => LunaDataTable.Instance.itemTable.Sum(pair => pair.Value.itemCount - (before.TryGetValue(pair.Key, out int value) ? value : 0));
    private IEnumerator RewardContracts()
    {
        float originalTime = Time.timeScale; var random = UnityEngine.Random.state;
        try
        {
            Time.timeScale = 4f;
            if (Includes("rewards", "native-loot-prefab-identity")) yield return LootIdentityContract();
            if (Includes("rewards", "campaign-victory-fast-leave"))
            {
                BeginCase("rewards", "campaign-victory-fast-leave");
                yield return LoadStage("Stage01-01"); var game = (BasicDungeon)LunaScenes.CurrentGame;
                HoldRoundActors(game); var player = LunaDataTable.Instance.playerData;
                int goldBefore = player.Gold; var itemsBefore = ItemCounts();
                UnitLogic.From(game.gameEnemyTower).stat.Hp = 0f;
                yield return Until(() => game.isStageWin && game.isStageStop && game.PendingLootCount > 0, "Victory creates owned reward records");
                int earned = game.PendingLootCount; int rewardBefore = player.RewardGold;
                Require(earned >= 5, "First stage's authored five coins were not registered before asynchronous visuals.");
                LunaScenes.Load("Lobby");
                yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning, "Immediate victory exit");
                int goldAfter = player.Gold; int delta = goldAfter - goldBefore;
                Require(delta == 500 || delta == 1000, "Fast exit lost or duplicated the first stage's five coins (Rich may double them).");
                Require(game.PendingLootCount == 0 && InventoryDelta(itemsBefore) == 0, "Victory exit did not settle its ledger or invented an unauthored first-stage item reward.");
                game.SettleLoot(); yield return new WaitForSeconds(2f);
                Require(player.Gold == goldAfter && game.PendingLootCount == 0, "Late pickup or repeated settlement granted victory twice.");
                current.observations.Add("registered=" + earned + "; goldDelta=" + delta + "; rewardCountBeforeExit=" + rewardBefore);
                EndCase("Natural campaign victory registered coins before visual load; immediate exit settled once and late callbacks did not duplicate income.");
            }
            if (Includes("rewards", "campaign-enemy-death")) yield return MonsterRewardContract();
            if (Includes("rewards", "endless-authored-and-controlled-payout")) yield return EndlessRewardContract();
        }
        finally { Time.timeScale = originalTime; UnityEngine.Random.state = random; }
    }
    private IEnumerator LootIdentityContract()
    {
        BeginCase("rewards", "native-loot-prefab-identity"); yield return EnsureFixtureLobby();
        int round = NewFixtureRound(); GameObject priorPlayer = LunaSession.Current.LunaPlayer;
        try
        {
            PlayerLogic player = null; var prefab = LunaSession.Current.Players[0];
            yield return ShowFixture<PlayerLogic>(prefab, ControlledData(prefab, Vector3.zero, round, eUnitSide.Allied), value => player = value);
            HoldAutomaticActions(player); LunaSession.Current.LunaPlayer = player.gameObject;
            foreach (var source in Resources.LoadAll<ItemData>("ScriptableObjects/Items").Where(value => value.cunnectPrefab != null).OrderBy(value => value.itemNumber))
            {
                if (!LunaDataTable.Instance.itemTable.TryGetValue(source.itemNumber, out var runtime)) continue;
                string original = JsonUtility.ToJson(source); int before = runtime.itemCount;
                var data = LootData.Create(source.itemNumber, 2, new Vector3(0.2f, 0f), roundId: round);
                int id = data.Id;
                LunaSpawn.Show<DropItemLogic>(source.cunnectPrefab, data);
                yield return Until(() => runtime.itemCount == before + 2 && LunaSpawn.Find<DropItemLogic>(id) == null, "Actual item pickup " + source.itemNumber, 12f);
                Require(JsonUtility.ToJson(source) == original, "Item pickup mutated its source asset.");
                current.observations.Add("ItemId " + source.itemNumber + " -> " + source.cunnectPrefab.name + ": +2 exactly.");
            }
            EndCase("Each real item prefab collected the pre-Show item identity/count; source ItemData assets stayed unchanged.");
        }
        finally { LunaSession.Current.LunaPlayer = priorPlayer; LunaSpawn.CancelRound(round); }
        yield return RetireFixture(round);
    }
    private IEnumerator MonsterRewardContract()
    {
        BeginCase("rewards", "campaign-enemy-death"); yield return LoadStage("Stage01-01");
        var game = LunaScenes.CurrentGame; HoldRoundActors(game);
        var row = LunaStageTables.GetRewards(game.Definition.Row.Id, "Monster").Single();
        var item = Resources.Load<ItemData>(row.Item); int before = LunaDataTable.Instance.itemTable[item.itemNumber].itemCount;
        UnitLogic enemy = null;
        game.SpawnUnit(Resources.Load<GameObject>("Prefabs/04Monsters/SkeletonSword"), Vector3.zero, shown: value => enemy = value);
        yield return Until(() => enemy != null, "Reward-bearing enemy spawn"); HoldAutomaticActions(enemy);
        int seed = 0;
        for (; seed < 100000; seed++) { UnityEngine.Random.InitState(seed); if (Defined.CanProbability(row.Probability * 100f)) break; }
        Require(seed < 100000, "No deterministic success roll found for the authored monster probability.");
        UnityEngine.Random.InitState(seed); enemy.stat.Hp = 0f;
        yield return Until(() => game.PendingLootCount > 0 || LunaDataTable.Instance.itemTable[item.itemNumber].itemCount > before, "Completed native enemy death reward", 12f);
        game.SettleLoot();
        Require(LunaDataTable.Instance.itemTable[item.itemNumber].itemCount == before + 1, "One eligible enemy death did not grant exactly one authored monster reward.");
        game.SettleLoot(); yield return new WaitForSeconds(2f);
        Require(LunaDataTable.Instance.itemTable[item.itemNumber].itemCount == before + 1, "Death/pickup completion duplicated its monster reward.");
        current.observations.Add("Seed=" + seed + "; item=" + item.itemNumber + "; configured probability=" + row.Probability);
        EndCase("A real completed enemy death used the authored probability/table, with one grant across settlement and late visual pickup.");
        yield return EnsureFixtureLobby();
    }
    private IEnumerator EndlessRewardContract()
    {
        BeginCase("rewards", "endless-authored-and-controlled-payout");
        const string key = "Dungeon04"; int stageId = LunaStageTables.Get(key).Id;
        Require(LunaStageTables.GetRewards(stageId, "Victory").Count == 0 && LunaStageTables.GetRewards(stageId, "EndlessBoss").Count == 0,
            "Authored Endless reward table changed; update the explicit empty-table oracle.");
        yield return LoadDungeonFixture(key); var authored = (EndlessDungeon)LunaScenes.CurrentGame;
        var authoredBefore = ItemCounts(); authored.Player.stat.Hp = 0f;
        yield return Until(() => Active<UIResult>() != null, "Authored zero-floor Endless defeat", 20f);
        Require(!authored.isStageWin && authored.dungeonLevel == 0 && authored.PendingLootCount == 0 && InventoryDelta(authoredBefore) == 0,
            "Zero completed floors or empty authored rewards invented loot.");
        yield return EnsureFixtureLobby();
        const int bossRow = 900001, normalRow = 900002;
        var bossItem = Resources.Load<ItemData>("ScriptableObjects/Items/1000_equip_stone");
        var normalItem = Resources.Load<ItemData>("ScriptableObjects/Items/1001_skill_stone");
        string bossSource = JsonUtility.ToJson(bossItem), normalSource = JsonUtility.ToJson(normalItem);
        Require(!LunaStageTables.Rewards.HasDataRow(bossRow) && !LunaStageTables.Rewards.HasDataRow(normalRow), "Reserved isolated reward fixture IDs collide.");
        try
        {
            Require(LunaStageTables.Rewards.AddDataRow(bossRow + "," + stageId + ",EndlessBoss,ScriptableObjects/Items/1000_equip_stone,1", null), "Could not add controlled boss reward row.");
            Require(LunaStageTables.Rewards.AddDataRow(normalRow + "," + stageId + ",Victory,ScriptableObjects/Items/1001_skill_stone,1", null), "Could not add controlled normal reward row.");
            yield return LoadDungeonFixture(key); var controlled = (EndlessDungeon)LunaScenes.CurrentGame;
            yield return CompleteEndlessFloor(controlled); yield return CompleteEndlessFloor(controlled);
            int beforeBoss = LunaDataTable.Instance.itemTable[bossItem.itemNumber].itemCount;
            int beforeNormal = LunaDataTable.Instance.itemTable[normalItem.itemNumber].itemCount;
            controlled.Player.stat.Hp = 0f;
            yield return Until(() => Active<UIResult>() != null, "Controlled two-floor Endless defeat", 20f);
            Require(controlled.dungeonLevel == 2 && !controlled.isStageWin && !controlled.PlayerRespawnPending, "Completed-floor defeat/respawn policy changed.");
            Require(LunaDataTable.Instance.itemTable[bossItem.itemNumber].itemCount == beforeBoss + 2 &&
                LunaDataTable.Instance.itemTable[normalItem.itemNumber].itemCount == beforeNormal + 2, "Mixed boss/normal rewards used the wrong item identity or floor multiplier.");
            controlled.SettleLoot(); yield return EnsureFixtureLobby();
            Require(LunaDataTable.Instance.itemTable[bossItem.itemNumber].itemCount == beforeBoss + 2 &&
                LunaDataTable.Instance.itemTable[normalItem.itemNumber].itemCount == beforeNormal + 2, "Endless settlement repeated at exit.");
            Require(JsonUtility.ToJson(bossItem) == bossSource && JsonUtility.ToJson(normalItem) == normalSource, "Controlled rewards mutated source assets.");
            current.observations.Add("Authored Dungeon04 reward rows=0 (expected no payout); temporary native rows exercised two real completed floors: boss2 + normal2. No production CSV edit.");
        }
        finally { LunaStageTables.Rewards.RemoveDataRow(bossRow); LunaStageTables.Rewards.RemoveDataRow(normalRow); }
        EndCase("Empty authored data and zero-floor semantics distinguished from a controlled native-table mixed reward regression; temporary rows removed.");
    }
}
#endif

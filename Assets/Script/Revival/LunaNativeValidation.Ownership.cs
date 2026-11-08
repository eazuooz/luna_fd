#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private IEnumerator ResourceFailureOwnership()
    {
        yield return EnsureFixtureLobby();
        BeginCase("ownership", "resource-failure-and-retry");
        var catalog = (Dictionary<GameObject, LunaEntityCatalog.Entry>)typeof(LunaSpawn)
            .GetField("catalog", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        var prefab = Resources.Load<GameObject>("Prefabs/01Heroes/Alice");
        var original = catalog[prefab];
        int round = NewFixtureRound();
        int baseline = ReferenceUsage(typeof(LunaNativeFaultData));
        string missing = "LUNA_INJECTED_OWNERSHIP/missing-resource-" + Guid.NewGuid().ToString("N");
        expectedErrorToken = "LUNA_INJECTED_OWNERSHIP"; expectedErrors.Clear();
        try
        {
            catalog[prefab] = new LunaEntityCatalog.Entry { Prefab = prefab, Address = missing, Group = original.Group, LogicType = typeof(LunaNativeFaultLogic).FullName };
            var data = LunaEntityData.Create<LunaNativeFaultData>();
            data.RoundId = round; data.Token = Guid.NewGuid().ToString("N"); data.ReleaseCount = 0;
            string token = data.Token; int id = data.Id, completed = 0, failures = 0;
            LunaSpawn.Show<LunaNativeFaultLogic>(prefab, data, actor => completed++, error =>
            {
                Require(error.Contains(missing), "Missing-resource failure lost its exact requested address: " + error);
                failures++;
            });
            yield return Until(() => failures > 0, "Actual native unknown-address load failure");
            yield return null;
            Require(failures == 1 && completed == 0, "Unknown resource completed or delivered duplicate failures.");
            Require(data.ReleaseCount == 1 && LunaNativeFaultData.ClearCounts[token] == 1, "Unknown resource payload was not returned exactly once.");
            Require(!GameEntry.GetComponent<EntityComponent>().HasEntity(id) && LunaSpawn.PendingCount == 0, "Unknown resource retained a native ID or pending request.");
            Require(ReferenceUsage(typeof(LunaNativeFaultData)) == baseline, "Failed native resource load leaked a ReferencePool payload.");
            Require(expectedErrors.Count > 0 && expectedErrors.All(error => error.Contains(missing)), "Native resource failure did not produce its exact tagged diagnostics.");
            report.expectedDiagnostics.AddRange(expectedErrors);
            current.observations.AddRange(expectedErrors);
            var resourceWarnings = report.warnings.Where(warning => warning.Contains(missing)).ToArray();
            Require(resourceWarnings.Length == 1 && resourceWarnings[0].Contains("NotExist"), "Missing resource did not produce exactly one native NotExist warning for its requested address.");
            report.expectedWarnings.Add(resourceWarnings[0]);
            current.observations.Add("Expected resource-failure warning verified; retained verbatim in raw warnings: " + resourceWarnings[0]);
            // Restore a valid resource but keep the same diagnostic logic/data type for a real retry.
            catalog[prefab] = new LunaEntityCatalog.Entry { Prefab = prefab, Address = original.Address, Group = original.Group, LogicType = typeof(LunaNativeFaultLogic).FullName };
            var retry = LunaEntityData.Create<LunaNativeFaultData>();
            retry.RoundId = round; retry.Token = Guid.NewGuid().ToString("N"); retry.ReleaseCount = 0;
            string retryToken = retry.Token;
            LunaNativeFaultLogic recovered = null;
            yield return ShowFixture<LunaNativeFaultLogic>(prefab, retry, actor => recovered = actor);
            recovered.SafeHide();
            yield return Until(() => LunaSpawn.PendingCount == 0 && LunaNativeFaultData.ClearCounts.ContainsKey(retryToken), "Recovered native resource retirement");
            Require(retry.ReleaseCount == 1 && LunaNativeFaultData.ClearCounts[retryToken] == 1 && ReferenceUsage(typeof(LunaNativeFaultData)) == baseline,
                "Valid retry did not return its payload exactly once to the original pool baseline.");
        }
        finally { catalog[prefab] = original; LunaSpawn.CancelRound(round); expectedErrorToken = null; }
        EndCase("Real missing Addressables resource failed once, returned its unclaimed payload, removed its pending ID, then a valid native retry succeeded and returned to the pool baseline.");
    }

    private static int ReferenceUsage(Type type) => GameFramework.ReferencePool.GetAllReferencePoolInfos()
        .Where(info => info.Type == type).Select(info => info.UsingReferenceCount).DefaultIfEmpty(0).Single();

    private void AssertEntityDataOwnershipBalance(string checkpoint)
    {
        Require(LunaSpawn.PendingCount == 0, "EntityData balance sampled during pending native loads.");
        var loaded = Entities();
        Require(loaded.All(actor => actor != null && actor.Available && !actor.IsHiding && actor.SpawnData != null && actor.SpawnData.OwnedByEntity),
            "EntityData balance found an unavailable actor or missing/uncharged payload owner.");
        var owned = loaded.GroupBy(actor => actor.SpawnData.GetType()).ToDictionary(group => group.Key, group => group.Count());
        var pools = GameFramework.ReferencePool.GetAllReferencePoolInfos()
            .Where(info => typeof(LunaEntityData).IsAssignableFrom(info.Type)).ToArray();
        var failures = new List<string>();
        foreach (var type in owned.Keys)
            if (!pools.Any(info => info.Type == type)) failures.Add("Live payload has no ReferencePool record: " + type.FullName);
        foreach (var info in pools)
        {
            int expected = owned.TryGetValue(info.Type, out int count) ? count : 0;
            if (info.UsingReferenceCount != expected)
                failures.Add(info.Type.FullName + ": used=" + info.UsingReferenceCount + ", native owners=" + expected +
                    ", acquired=" + info.AcquireReferenceCount + ", released=" + info.ReleaseReferenceCount);
        }
        // Several content cases close their behavioral result before finally/retirement.
        // Attach this cleanup evidence to that same case and revoke its pass if cleanup fails.
        var context = current ?? report.cases.LastOrDefault();
        context?.observations.Add(checkpoint + ": EntityData usage/ownership: " + string.Join("; ", pools.OrderBy(info => info.Type.FullName)
            .Select(info => info.Type.Name + "=" + info.UsingReferenceCount + " (acquired=" + info.AcquireReferenceCount + ", released=" + info.ReleaseReferenceCount + ")")));
        if (failures.Count != 0 && current == null && context != null)
        {
            context.success = false;
            context.finishedUtc = DateTime.UtcNow.ToString("O");
            context.detail = "Behavior completed, but native EntityData cleanup failed.";
            report.checks.RemoveAll(check => check.StartsWith(context.group + "/" + context.name + ":", StringComparison.Ordinal));
        }
        Require(failures.Count == 0, checkpoint + ": native EntityData leak/ownership mismatch: " + string.Join("; ", failures));
    }

    private IEnumerator PlayerLoadoutOwnership()
    {
        yield return EnsureFixtureLobby();
        BeginCase("ownership", "pending-player-loadout-snapshots");
        var table = LunaDataTable.Instance; var player = table.playerData;
        var originalSkills = player.equippedSkill.ToList(); var originalSkin = player.SkinType;
        int originalMoveLevel = player.LunaMoveSpeedLevel, originalAuraLevel = player.AuroraLevel;
        GameObject originalPlayer = LunaSession.Current.LunaPlayer;
        Vector3 originalLeft = Defined.AllyTowerPosition, originalRight = Defined.EnemyTowerPosition;
        var fire = table.skiiTable["FireBall"];
        int originalMana = fire.mana, originalLevel = fire.level; float originalCooldown = fire.coolTime;
        int round = NewFixtureRound(), baseline = ReferenceUsage(typeof(UnitData));
        PlayerLogic first = null, second = null;
        var prefab = LunaSession.Current.Players[0]; var view = prefab.GetComponent<UnitView>();
        try
        {
            player.LunaMoveSpeedLevel = originalMoveLevel + 2; player.AuroraLevel = originalAuraLevel + 3;
            int firstAura = player.AuroraLevel;
            var definition = LunaRuntimeData.ResolveObjectData(view.Definition);
            float authoredMove = definition.MoveSpeed + definition.LevelUpMoveSpeed * Mathf.Max(0, definition.moveSpeedLevel - 1);
            float firstMove = Mathf.Abs(authoredMove + Defined.AddMoveSpeedValue * player.LunaMoveSpeedLevel);
            player.SkinType = PlayerData.eLunaSkin.Basic;
            player.equippedSkill = new List<string> { "FireBall", "Healing", "Thunder" };
            var a = UnitData.CreateFrom(view, new Vector3(-3f, -50f));
            a.RoundId = round; a.InitialMode = UnitSpawnMode.Staged; a.Level = 2; a.Side = eUnitSide.Allied;
            var capturedA = a.PlayerLoadout;
            Require(capturedA != null, "Typed player request did not capture its loadout before Show.");
            fire.mana = 7; fire.coolTime = 1f; fire.level = originalLevel + 9;
            player.LunaMoveSpeedLevel = originalMoveLevel + 5; player.AuroraLevel = originalAuraLevel + 6;
            int secondAura = player.AuroraLevel;
            float secondMove = Mathf.Abs(authoredMove + Defined.AddMoveSpeedValue * player.LunaMoveSpeedLevel);
            player.SkinType = PlayerData.eLunaSkin.Wood;
            player.equippedSkill = new List<string> { "Blizzard", "Healing", "Thunder" };
            var b = UnitData.CreateFrom(view, new Vector3(3f, -50f));
            b.RoundId = round; b.InitialMode = UnitSpawnMode.Staged; b.Level = 4; b.Side = eUnitSide.Enemy;
            var capturedB = b.PlayerLoadout;
            int firstId = a.Id, secondId = b.Id, completions = 0;
            LunaSpawn.Show<PlayerLogic>(prefab, a, actor => { first = actor; completions++; });
            LunaSpawn.Show<PlayerLogic>(prefab, b, actor => { second = actor; completions++; });
            Require(completions == 0 && LunaSpawn.PendingCount >= 2, "Fixture did not exercise overlapping native pending requests.");
            // Change the global selection again while both requests are pending. The first
            // native OnShow must consume its own immutable request, not these later values.
            player.equippedSkill = new List<string> { "GoldyBlessing", "Dwarf", "AngerArmy" };
            fire.mana = 99; fire.coolTime = 123f; fire.level = originalLevel + 20;
            player.LunaMoveSpeedLevel = originalMoveLevel + 11; player.AuroraLevel = originalAuraLevel + 12;
            yield return Until(() => completions == 2, "Overlapping typed player load requests");
            Require(first.ID == firstId && second.ID == secondId && firstId != secondId, "Overlapping player shows mixed native IDs.");
            Require(first.Data.Level == 2 && first.UnitSide == eUnitSide.Allied && second.Data.Level == 4 && second.UnitSide == eUnitSide.Enemy,
                "Overlapping player requests mixed scalar initialization data.");
            Require(Mathf.Abs(first.Data.BaseMoveSpeed - firstMove) < 0.0001f && Mathf.Abs(second.Data.BaseMoveSpeed - secondMove) < 0.0001f &&
                first.Data.AuraLevel == firstAura && second.Data.AuraLevel == secondAura,
                "Pending player requests consumed later global movement/aura upgrades.");
            Require(Mathf.Abs(first.stat.MoveSpeed - firstMove) < 0.0001f && Mathf.Abs(second.stat.MoveSpeed + secondMove) < 0.0001f,
                "First native initialization did not use each captured movement value and requested side.");
            first.coefficient.MoveSpeed = 1.5f; second.coefficient.MoveSpeed = 0.75f;
            first.StatusAfterReady(); second.StatusAfterReady();
            Require(Mathf.Abs(first.stat.MoveSpeed - firstMove * 1.5f) < 0.0001f && Mathf.Abs(second.stat.MoveSpeed + secondMove * 0.75f) < 0.0001f,
                "Status recomputation reread changed global upgrades instead of the per-request movement base.");
            Require(first.Abilities != null && second.Abilities != null, "Staged player requests completed before their owned ability controllers were initialized.");
            Require(ReferenceEquals(first.Abilities.Loadout, capturedA) && ReferenceEquals(second.Abilities.Loadout, capturedB), "Player OnShow replaced a request loadout with global state.");
            Require(first.Abilities.Loadout.Skills[0].name == "FireBall" && second.Abilities.Loadout.Skills[0].name == "Blizzard", "Pending player requests mixed skill selections.");
            var skill = first.Abilities.Loadout.Skills[0];
            Require(skill.mana == originalMana && skill.coolTime == originalCooldown && skill.level == originalLevel, "Mutable runtime SkillData changed a pending player's captured values.");
            Require(first.Abilities.Loadout.Equipment.Count == capturedA.Equipment.Count && second.Abilities.Loadout.Equipment.Count == capturedB.Equipment.Count,
                "Pending player requests mixed equipment selections.");
            // Measure the ordinary native movement handler of a same-round ally. Its
            // own captured aura/global progression differ from the actual player's value.
            var soldierPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
            var soldierData = UnitData.CreateFrom(soldierPrefab.GetComponent<UnitView>(), Vector3.right);
            soldierData.RoundId = round; soldierData.Side = eUnitSide.Allied; soldierData.InitialMode = UnitSpawnMode.Staged;
            UnitLogic recipient = null;
            yield return ShowFixture<UnitLogic>(soldierPrefab, soldierData, actor => recipient = actor);
            Defined.AllyTowerPosition = new Vector3(-100f, 0f); Defined.EnemyTowerPosition = new Vector3(100f, 0f);
            first.transform.position = Vector3.zero; recipient.transform.position = Vector3.right;
            LunaSession.Current.LunaPlayer = first.gameObject;
            HoldAutomaticActions(recipient);
            yield return Until(() => Entities().OfType<EffectLogic>().Any(effect => effect.SpawnData != null && effect.SpawnData.OwnerId == recipient.ID && effect.SpawnData.RoundId == round),
                "Matching player's aura reached its native ally");
            float beforeX = recipient.transform.position.x;
            float expectedDistance = (Mathf.Abs(recipient.stat.MoveSpeed) + Mathf.Max(0, firstAura - 1) * 0.15f) * 0.25f;
            recipient.MoveCombat(1f, 0.25f);
            float moved = recipient.transform.position.x - beforeX;
            Require(Mathf.Abs(moved - expectedDistance) < 0.0001f,
                "Native aura movement used recipient/global progression rather than the matching player's captured aura: expected " + expectedDistance + ", actual " + moved);
            current.observations.Add("Two pending native IDs kept level2/4, opposing sides, FireBall/Blizzard selections and captured mana/cooldown/level despite later global changes.");
            current.observations.Add("Captured movement=" + firstMove + "/" + secondMove + ", aura=" + firstAura + "/" + secondAura +
                "; signed status recomputation retained snapshots; allied native movement with captured player aura=" + moved);
        }
        finally
        {
            player.equippedSkill = originalSkills; player.SkinType = originalSkin;
            player.LunaMoveSpeedLevel = originalMoveLevel; player.AuroraLevel = originalAuraLevel;
            LunaSession.Current.LunaPlayer = originalPlayer;
            Defined.AllyTowerPosition = originalLeft; Defined.EnemyTowerPosition = originalRight;
            fire.mana = originalMana; fire.coolTime = originalCooldown; fire.level = originalLevel;
            LunaSpawn.CancelRound(round);
        }
        yield return RetireFixture(round);
        Require(ReferenceUsage(typeof(UnitData)) == baseline, "Overlapping player requests retained UnitData after round retirement.");
        EndCase("Two real pending native loads consumed independent immutable player loadouts; no global post-request selection/value change leaked into OnShow, and all fixture payloads returned to their pool baseline.");
    }

    private IEnumerator FailureOwnership()
    {
        BeginCase("ownership", "callback-failure-disposal");
        var catalogField = typeof(LunaSpawn).GetField("catalog", BindingFlags.NonPublic | BindingFlags.Static);
        var catalog = catalogField?.GetValue(null) as Dictionary<GameObject, LunaEntityCatalog.Entry>;
        Require(catalog != null, "Native fault fixture could not locate the private spawn catalog.");
        // A dedicated development fixture temporarily substitutes logic for an existing resource.
        // No production registration API or prefab is added for diagnostics.
        var prefab = Resources.LoadAll<GameObject>("Prefabs/01Heroes").OrderBy(asset => asset.name).First();
        Require(catalog.TryGetValue(prefab, out var original), "Fault fixture prefab is not registered.");
        var replacement = new LunaEntityCatalog.Entry { Prefab = prefab, Address = original.Address, Group = original.Group, LogicType = typeof(LunaNativeFaultLogic).FullName };
        catalog[prefab] = replacement;
        int round = NewFixtureRound();
        var temporaryParent = new GameObject("Native fault fixture owner");
        expectedErrorToken = "LUNA_INJECTED_OWNERSHIP";
        expectedErrors.Clear();
        try
        {
            for (int mode = 1; mode <= 3; mode++)
            {
                var data = LunaEntityData.Create<LunaNativeFaultData>();
                data.RoundId = round; data.Position = new Vector3(0f, -100f); data.Mode = mode;
                data.ReleaseCount = 0;
                if (mode == 3) data.Parent = temporaryParent.transform;
                data.Token = Guid.NewGuid().ToString("N");
                string token = data.Token;
                int id = data.Id, completed = 0, failed = 0;
                LunaNativeFaultLogic actor = null;
                LunaSpawn.Show<LunaNativeFaultLogic>(prefab, data, shown => { actor = shown; completed++; }, error =>
                {
                    Require(error.Contains(expectedErrorToken), "Fault fixture failed for an unrelated reason: " + error);
                    failed++;
                });
                yield return Until(() => failed != 0 || completed != 0, "Injected callback mode " + mode);
                if (mode == 3)
                {
                    Require(completed == 1 && failed == 0 && actor != null, "OnHide fault did not initialize successfully first.");
                    actor.SafeHide(); actor.SafeHide();
                }
                else Require(failed == 1 && completed == 0, "OnShow failure was reported as successful or duplicated.");
                yield return Until(() => LunaNativeFaultData.ClearCounts.TryGetValue(token, out int clears) && clears > 0, "Fault data returned to ReferencePool");
                yield return null;
                Require(LunaNativeFaultData.ClearCounts[token] == 1, "Fault data released more than once, mode " + mode);
                Require(data.ReleaseCount == 1, "The same fault payload received multiple Clear calls, mode " + mode);
                Require(!GameEntry.GetComponent<EntityComponent>().HasEntity(id) && LunaSpawn.PendingCount == 0, "Fault retained loaded or pending native entity, mode " + mode);
                if (mode == 3)
                {
                    Require(actor != null && !actor.Available && !actor.gameObject.activeSelf, "Failed OnHide left its recycled entity available or visible.");
                    Require(!actor.transform.IsChildOf(temporaryParent.transform), "Failed OnHide retained the caller parent after recycling.");
                }
                current.observations.Add("Mode " + mode + ": success=" + completed + ", failure=" + failed + ", Clear=" + LunaNativeFaultData.ClearCounts[token] + ", pending=0.");
            }
            Require(expectedErrors.Count == 3, "Expected exactly three tagged native callback failures; observed " + expectedErrors.Count);
            current.observations.AddRange(expectedErrors);
            report.expectedDiagnostics.AddRange(expectedErrors);
        }
        finally
        {
            LunaSpawn.CancelRound(round);
            catalog[prefab] = original;
            expectedErrorToken = null;
            Destroy(temporaryParent);
        }
        EndCase("Native callbacks failed before and after data ownership transfer and during Hide; every data instance returned exactly once, no success escaped an initialization failure, no pending/loaded ID remained.");
    }
}

/// <summary>Diagnostic-only reference payload; never participates in normal gameplay.</summary>
public sealed class LunaNativeFaultData : LunaEntityData
{
    public static readonly Dictionary<string, int> ClearCounts = new Dictionary<string, int>();
    public string Token;
    public int Mode;
    public int ReleaseCount;
    public override void Clear()
    {
        ReleaseCount++;
        if (Token != null) { ClearCounts.TryGetValue(Token, out int count); ClearCounts[Token] = count + 1; }
        base.Clear(); Mode = 0; Token = null;
    }
}

/// <summary>Deliberately failing native lifecycle, installed only by an explicitly enabled isolated diagnostic.</summary>
public sealed class LunaNativeFaultLogic : EntityLogicBase
{
    private int mode;
    protected override void OnShow(object userData)
    {
        if (!Application.isEditor && !Debug.isDebugBuild) throw new InvalidOperationException("Fault logic is development-only.");
        mode = ((LunaNativeFaultData)userData).Mode;
        if (mode == 1) throw new InvalidOperationException("LUNA_INJECTED_OWNERSHIP before base.OnShow");
        base.OnShow(userData);
        if (mode == 2) throw new InvalidOperationException("LUNA_INJECTED_OWNERSHIP after base.OnShow");
    }
    protected override void OnHide(bool isShutdown, object userData)
    {
        if (mode == 3) throw new InvalidOperationException("LUNA_INJECTED_OWNERSHIP before base.OnHide");
        base.OnHide(isShutdown, userData);
    }
}
#endif

#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    private static readonly int[] SkillGameplayLevels = { 20 };

    private IEnumerator SkillLevelContracts()
    {
        yield return SkillLevelSnapshotContracts();
        var table = LunaDataTable.Instance;
        var progression = table.playerData;
        var oldSkills = progression.equippedSkill.ToList(); var oldSkin = progression.SkinType;
        var originalLevels = PlayerSkillNames.ToDictionary(name => name, name => table.skiiTable[name].level);
        string csv = Resources.Load<TextAsset>("DataTables/SkillBalance").text;
        float originalScale = Time.timeScale;
        try
        {
            progression.SkinType = PlayerData.eLunaSkin.Basic;
            foreach (int level in SkillGameplayLevels)
            {
                foreach (string name in PlayerSkillNames) table.skiiTable[name].level = level;
                for (int batch = 0; batch < PlayerSkillNames.Length; batch += 3)
                {
                    var names = PlayerSkillNames.Skip(batch).Take(3).ToArray();
                    if (!names.Any(name => Includes("skill-levels", name + "/level" + level))) continue;
                    progression.SkinType = names.Contains("DemiGod") ? PlayerData.eLunaSkin.SkullAx : PlayerData.eLunaSkin.Basic;
                    progression.equippedSkill = names.ToList();
                    yield return LoadStage("Stage01-01");
                    Time.timeScale = 8f;
                    var round = LunaScenes.CurrentGame;
                    var player = (PlayerLogic)round.Player;
                    round.isStageStop = true;
                    foreach (var unit in UnitLogic.ActiveUnits.Where(unit => unit.Data.RoundId == round.RoundId).ToArray())
                    {
                        if (unit.hitPriority == eHitPriority.TOWER)
                        {
                            // Stopping round waves does not stop a tower's native FSM.
                            // Demi's target moves beside the ally tower; its authored
                            // 300-damage turret must not contaminate the clone's hit.
                            unit.Invincible(0f); HoldAutomaticActions(unit); unit.ResetAnimation();
                        }
                        else if (unit != player) unit.SafeHide();
                        else { HoldAutomaticActions(unit); unit.ResetAnimation(); }
                    }
                    // Wait the real player-owned cooldowns; never alter timers or balance rows.
                    yield return Until(() => names.All(name => player.Abilities.Ready(table.skiiTable[name])), "Natural boundary ability cooldowns", 45f);
                    yield return Until(() => LunaSpawn.PendingCount == 0 && !Entities().OfType<ProjectileLogic>().Any(shot =>
                        shot.Data != null && shot.Data.RoundId == round.RoundId && UnitLogic.Find(shot.Data.AttackerId) is TowerLogic),
                        "Pre-fixture tower projectiles retired after their old targets were hidden");
                    foreach (string name in names)
                    {
                        if (!Includes("skill-levels", name + "/level" + level)) continue;
                        yield return ExerciseSkillLevel(name, level, round, player);
                    }
                }
            }
            Require(Resources.Load<TextAsset>("DataTables/SkillBalance").text == csv, "Skill level exercise changed the native base CSV.");
        }
        finally
        {
            progression.equippedSkill = oldSkills; progression.SkinType = oldSkin;
            foreach (var pair in originalLevels) table.skiiTable[pair.Key].level = pair.Value;
            Time.timeScale = originalScale;
        }
    }

    private IEnumerator ExerciseSkillLevel(string name, int level, LunaGame round, PlayerLogic player)
    {
        BeginCase("skill-levels", name + "/level" + level);
        int tier = level < 5 ? 0 : level < 10 ? 1 : level < 15 ? 2 : level < 20 ? 3 : 4;
        var source = LunaDataTable.Instance.skiiTable[name];
        var captured = player.Abilities.Loadout.Skills.Single(skill => skill.name == name);
        var prefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
        var fixtures = new List<int>();
        int[] owned = Array.Empty<int>();
        UnitLogic ally = null, enemy = null, outside = null;
        float oldPlayerAttack = player.stat.AttackDamage;
        Vector3 oldPlayerPosition = player.transform.position;
        try
        {
            float radius = name == "FireBall" || name == "Thunder" ? source.range * (1f + tier * 0.1f) : source.range;
            foreach (int role in new[] { 0, 1, 2 })
            {
                float offset = role == 0 ? -0.5f : role == 1 ? (name == "FireBall" || name == "Thunder" ? radius - 0.05f : 1f) : (name == "FireBall" || name == "Thunder" ? radius + 0.05f : 10f);
                var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), player.transform.position + Vector3.right * offset);
                data.RoundId = round.RoundId; data.Side = role == 0 ? eUnitSide.Allied : eUnitSide.Enemy;
                data.InitialMode = UnitSpawnMode.Staged; data.HealthMultiplier = 10000f;
                fixtures.Add(data.Id);
                yield return ShowFixture<UnitLogic>(prefab, data, unit => { if (role == 0) ally = unit; else if (role == 1) enemy = unit; else outside = unit; });
            }
            foreach (var unit in new[] { ally, enemy, outside })
            {
                HoldAutomaticActions(unit); unit.stat.AttackDamage = 0f;
                unit.stat.Armour = unit.stat.SkillResistance = unit.stat.AttackAvoidance = unit.stat.SkillAvoidance = unit.stat.DebuffAvoidance = unit.stat.StunResistance = 0f;
            }
            ally.stat.Hp = ally.HealthBarHpPoint / 2f;
            float enemyHp = enemy.stat.Hp, outsideHp = outside.stat.Hp, allyHp = ally.stat.Hp;
            float armour = ally.coefficient.Armour, attack = ally.coefficient.AttackDamage;
            float attackSpeed = ally.coefficient.AttackSpeed, movement = ally.coefficient.MoveSpeed;
            if (name == "DemiGod") { player.stat.AttackDamage = 321f; enemy.stat.DebuffAvoidance = 100f; }
            int cost = source.mana;
            round.Roster.Mana = 100; round.isStageStop = false;
            float castTime = Time.time;
            Require(player.Abilities.TryUse(source, name == "Thunder" ? player.transform.position : (Vector3?)null), "Boundary ability rejected a ready, equipped, funded request.");
            Require(round.Roster.Mana == 100 - cost && player.Abilities.Remaining(source) > 0f, "Boundary ability did not pay/start cooldown once.");
            round.isStageStop = true;
            Require(captured.level == level, "Boundary request consumed another life/level snapshot.");
            switch (name)
            {
                case "FireBall": case "Thunder":
                    yield return Until(() => enemy.stat.Hp < enemyHp, "Effective boundary radius produced an actual hit");
                    float expectedDamage = source.damage * (name == "FireBall" ? 1f + tier * 0.1f : 1f) * player.coefficient.SkillPoint;
                    Require(Mathf.Abs(enemyHp - enemy.stat.Hp - expectedDamage) < 1f, "Boundary damage mismatch: expected " + expectedDamage + ", actual " + (enemyHp - enemy.stat.Hp));
                    Require(outside.stat.Hp == outsideHp, "Ability damaged a target outside its effective radius.");
                    current.observations.Add("Target distance=" + (radius - 0.05f) + "; base/effective radius=" + source.range + "/" + radius + "; actual damage=" + (enemyHp - enemy.stat.Hp));
                    break;
                case "Blizzard":
                    float expectedStun = (source.stunTime > 0f ? source.stunTime : 2f) + tier * 0.5f;
                    Require(enemy.isStun && Mathf.Abs(enemyHp - enemy.stat.Hp - source.damage * player.coefficient.SkillPoint) < 1f, "Blizzard did not immediately apply its actual damage/stun.");
                    yield return new WaitForSeconds(Mathf.Max(0.1f, expectedStun - 0.6f));
                    Require(enemy.isStun, "Blizzard stun ended before the authored level duration.");
                    yield return Until(() => !enemy.isStun, "Natural upgraded Blizzard stun expiry");
                    Require(Time.time - castTime <= expectedStun + 0.8f, "Blizzard stun exceeded its authored duration.");
                    current.observations.Add("Measured stun=" + (Time.time - castTime) + "; expected=" + expectedStun);
                    break;
                case "Healing":
                    Require(Mathf.Abs(ally.stat.Hp - allyHp - source.damage * (1f + tier * 0.1f)) < 1f, "Actual healing omitted the authored level multiplier.");
                    break;
                case "GoldyBlessing":
                    round.Roster.Gold = 0; round.isStageStop = false;
                    float incomeStart = Time.time;
                    yield return new WaitForSeconds(10f);
                    float elapsed = Time.time - incomeStart;
                    float rate = 2f * (1f + Defined.AddGoldSpeedValue * LunaDataTable.Instance.playerData.GoldSpeedLevel + (18f + tier * 3f) / 100f);
                    Require(Mathf.Abs(round.Roster.Gold - rate * elapsed) <= 1.5f, "Actual gold income did not preserve the18% base plus3 percentage points per milestone.");
                    round.isStageStop = true;
                    current.observations.Add("Actual gold=" + round.Roster.Gold + "; elapsed=" + elapsed + "; expected rate=" + rate);
                    break;
                case "LoneBleesing":
                    Require(Mathf.Abs(ally.coefficient.Armour - armour - (1f + tier * 0.05f)) < 0.0001f, "Lone armour milestone was not applied exactly once.");
                    yield return new WaitForSeconds(10.3f);
                    Require(Mathf.Abs(ally.coefficient.Armour - armour) < 0.0001f, "Lone upgraded armour did not restore after natural expiry.");
                    break;
                case "WaterDropBarrier":
                    player.stat.Armour = player.stat.AttackAvoidance = player.stat.SkillAvoidance = 0f;
                    float hp = player.stat.Hp;
                    player.Attack(enemy.gameObject, 100f);
                    Require(Mathf.Abs(hp - player.stat.Hp - (50f - tier * 10f)) < 0.05f, "Water barrier did not reduce actual incoming damage at this level.");
                    yield return new WaitForSeconds(5.3f);
                    hp = player.stat.Hp; player.Attack(enemy.gameObject, 100f);
                    Require(Mathf.Abs(hp - player.stat.Hp - 100f) < 0.05f, "Water barrier retained upgraded mitigation after expiry.");
                    break;
                case "AngerArmy":
                    Require(ally.IsBerserk && Mathf.Abs(ally.coefficient.AttackSpeed - attackSpeed - (0.5f + tier * 0.1f)) < 0.0001f &&
                        Mathf.Abs(ally.coefficient.MoveSpeed - movement - (1f + tier * 0.1f)) < 0.0001f, "Anger Army omitted the authored additions to its existing berserk stats.");
                    yield return new WaitForSeconds(3.3f);
                    Require(!ally.IsBerserk && Mathf.Abs(ally.coefficient.AttackSpeed - attackSpeed) < 0.0001f && Mathf.Abs(ally.coefficient.MoveSpeed - movement) < 0.0001f, "Upgraded berserk left coefficient residue.");
                    break;
                case "SerenBlessing":
                    Require(Mathf.Abs(ally.stat.AttackAvoidance - (25f + tier * 5f)) < 0.0001f && Mathf.Abs(ally.stat.SkillAvoidance - (25f + tier * 5f)) < 0.0001f && Mathf.Abs(ally.stat.DebuffAvoidance - (25f + tier * 5f)) < 0.0001f, "Seren omitted an authored resistance milestone/channel.");
                    yield return new WaitForSeconds(10.3f);
                    Require(Mathf.Abs(ally.stat.AttackAvoidance) < 0.0001f && Mathf.Abs(ally.stat.SkillAvoidance) < 0.0001f && Mathf.Abs(ally.stat.DebuffAvoidance) < 0.0001f, "Seren level resistance did not restore after natural expiry.");
                    break;
                case "BeleifWave":
                    Require(Mathf.Abs(ally.coefficient.AttackDamage - attack - (0.15f + tier * 0.03f)) < 0.0001f, "Belief omitted its authored attack milestone.");
                    yield return new WaitForSeconds(5.3f);
                    Require(Mathf.Abs(ally.coefficient.AttackDamage - attack) < 0.0001f, "Belief upgraded attack did not restore after expiry.");
                    break;
                case "Dwarf": case "KingSlime": case "SkeletonArcher": case "DemiGod":
                    int expectedCount = name == "DemiGod" ? 2 : 1;
                    yield return Until(() => UnitLogic.ActiveUnits.Count(unit => unit.Data.OwnerId == player.ID && unit.Data.RoundId == round.RoundId) == expectedCount, "Actual owned boundary summons");
                    var children = UnitLogic.ActiveUnits.Where(unit => unit.Data.OwnerId == player.ID && unit.Data.RoundId == round.RoundId).ToArray();
                    owned = children.Select(unit => unit.ID).ToArray();
                    foreach (var child in children)
                    {
                        HoldAutomaticActions(child);
                        float multiplier = name == "DemiGod" ? 1f : 1f + tier * 0.1f;
                        Require(Mathf.Abs(child.HealthBarHpPoint - (child.objectData.Hp + (child.Level - 1) * child.objectData.LevelUpHpPoint) * multiplier) < 0.5f, "Summoned actual HP did not consume its pre-show level multiplier.");
                        if (name == "KingSlime" || name == "SkeletonArcher")
                        {
                            Require(Mathf.Abs(child.stat.AttackDamage - (child.objectData.AttackDamage + (child.Level - 1) * child.objectData.LevelUpAttPoint) * multiplier) < 0.05f &&
                                Mathf.Abs(child.stat.Armour - (child.objectData.Armour + (child.Level - 1) * child.objectData.LevelUpArmourPoint) * multiplier) < 0.05f, "Summoned actual attack/armour omitted the level multiplier.");
                            Require(Mathf.Abs(child.Data.AttackMultiplier - multiplier) < 0.0001f && Mathf.Abs(child.Data.ArmourMultiplier - multiplier) < 0.0001f, "Zero-valued authored stats lost their typed level multiplier.");
                        }
                    }
                    if (name == "Dwarf")
                    {
                        var dwarf = children.Single();
                        Require(Mathf.Approximately(dwarf.Data.TauntRadius, 2f), "Dwarf focus was not configured before native Show.");
                        enemy.transform.position = dwarf.transform.position + Vector3.right * 0.8f;
                        player.transform.position = enemy.transform.position - Vector3.right * 0.1f;
                        enemy.AttackTarget = player.gameObject;
                        yield return Until(() => enemy.AttackTarget == dwarf.gameObject, "Dwarf actual focus overrides the closer current player target");
                    }
                    if (name == "DemiGod")
                    {
                        float lifetime = 5f + tier * 5f;
                        foreach (var child in children)
                        {
                            float swordFactor = player.Abilities.Loadout.Equipment.Where(gear => gear.name == "LunaEquipmentSkullSword").Aggregate(1f, (value, gear) => value * (1f + gear.coefficient / 100f));
                            Require(swordFactor > 1f && Mathf.Abs(child.Data.BaseAttackDamageOverride.Value - 321f / swordFactor) < 0.05f && Mathf.Approximately(child.Data.Lifetime, lifetime), "Demi clone failed to snapshot owner attack without double-counting SkullSword, or omitted its authored lifetime.");
                        }
                        yield return Until(() => children.All(child => Mathf.Abs(child.stat.AttackDamage - 481.5f) < 0.05f), "Demi clones receive SkullSword once through ordinary party equipment");
                        var clone = children[0];
                        current.observations.Add("Tower FSMs held before cooldown; no old tower projectile remained. Native turret powers: " +
                            string.Join(", ", UnitLogic.ActiveUnits.Where(unit => unit.Data != null && unit.Data.RoundId == round.RoundId)
                                .OfType<TowerLogic>().Select(tower => ((TowerData)tower.Data).Turret + "=" + ((TowerData)tower.Data).TurretDamage)));
                        enemy.transform.position = clone.transform.position + Vector3.right * 0.5f;
                        clone.AttackTarget = enemy.gameObject;
                        float ghostTargetHp = enemy.stat.Hp;
                        clone.BeginAttack();
                        yield return Until(() => enemy.stat.Hp < ghostTargetHp, "Demi clone actual native attack event");
                        Require(Mathf.Abs(ghostTargetHp - enemy.stat.Hp - 481.5f) < 1f, "Demi clone actual attack double-counted SkullSword or remained a zero-damage visual: " + (ghostTargetHp - enemy.stat.Hp));
                        yield return Until(() => owned.All(id => LunaSpawn.Find<UnitLogic>(id) == null), "Demi upgraded natural lifetime", lifetime / 8f + 5f);
                        Require(Time.time - castTime >= lifetime - 0.8f && Time.time - castTime <= lifetime + 2f, "Demi clone expired outside its authored level lifetime: " + (Time.time - castTime));
                        current.observations.Add("Both clones had the exact attack stat; one dealt native damage, and both retired after " + (Time.time - castTime) + " game seconds; expected lifetime=" + lifetime);
                    }
                    break;
            }
            current.observations.Add("Captured level=" + level + "; authored milestone tier=" + tier + "; actual mana paid=" + cost);
            EndCase("Real native ability activation at an authored level boundary produced its quantitative outcome and, where temporary, restored/retired naturally.");
        }
        finally
        {
            round.isStageStop = true;
            foreach (int id in fixtures.Concat(owned)) LunaSpawn.Cancel(id);
            // Gold income intentionally runs the real round for ten seconds.
            // Retire any resulting enemy wave before the next independent skill.
            foreach (var unit in UnitLogic.ActiveUnits.Where(unit => unit.Data != null && unit.Data.RoundId == round.RoundId && unit.UnitSide == eUnitSide.Enemy && unit.hitPriority != eHitPriority.TOWER).ToArray()) unit.SafeHide();
            if (player.Data != null) { player.stat.AttackDamage = oldPlayerAttack; player.transform.position = oldPlayerPosition; }
        }
        yield return Until(() => LunaSpawn.PendingCount == 0, "Boundary fixture native requests settled");
        AssertEntityDataOwnershipBalance("Skill boundary cleanup");
    }
}
#endif

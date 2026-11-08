#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Linq;
using GameFramework;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    // Explicit boundary oracles: these arrays do not call the production tier calculation.
    private static readonly int[] SnapshotLevels = { 4, 5, 9, 10, 14, 15, 19, 20 };
    private static readonly int[] SnapshotTiers = { 0, 1, 1, 2, 2, 3, 3, 4 };
    private static readonly float[] SnapshotGrowth = { 1f, 1.1f, 1.1f, 1.2f, 1.2f, 1.3f, 1.3f, 1.4f };
    private static readonly float[] SnapshotStunAdded = { 0f, 0.5f, 0.5f, 1f, 1f, 1.5f, 1.5f, 2f };
    private static readonly float[] SnapshotDurationAdded = { 0f, 5f, 5f, 10f, 10f, 15f, 15f, 20f };
    private static readonly float[] SnapshotArmour = { 1f, 1.05f, 1.05f, 1.1f, 1.1f, 1.15f, 1.15f, 1.2f };
    private static readonly float[] SnapshotReduction = { 0.5f, 0.6f, 0.6f, 0.7f, 0.7f, 0.8f, 0.8f, 0.9f };
    private static readonly float[] SnapshotBerserk = { 0f, 0.1f, 0.1f, 0.2f, 0.2f, 0.3f, 0.3f, 0.4f };
    private static readonly float[] SnapshotAvoidance = { 25f, 30f, 30f, 35f, 35f, 40f, 40f, 45f };
    private static readonly float[] SnapshotAttack = { 0.15f, 0.18f, 0.18f, 0.21f, 0.21f, 0.24f, 0.24f, 0.27f };
    private static readonly float[] SnapshotGold = { 6f, 7f, 7f, 8f, 8f, 9f, 9f, 10f };

    private static void CheckSkillLevelSnapshot(PlayerSkillSnapshot value, SkillData source, LunaSkillBalanceRow row, int level, int boundary)
    {
        float Base(string field) => (float)row.GetValue(field);
        void Equal(float actual, float expected, string field) => Require(Mathf.Abs(actual - expected) < 0.0002f,
            source.name + " level " + level + " " + field + ": expected " + expected + ", actual " + actual);
        string name = source.name;
        Require(value.name == name && value.level == level && value.UpgradeTier == SnapshotTiers[boundary], "Captured skill level/tier differs from its boundary oracle: " + name);
        Require(value.effectPrefab == source.effectPrefab && value.skillImage == source.skillImage, "Skill growth changed an art reference: " + name);
        Require(value.mana == (int)row.GetValue("mana") && value.maxSummonCount == (int)row.GetValue("maxSummonCount") && value.ableSkill == (bool)row.GetValue("ableSkill"), "Skill growth changed base cost/availability/summon limit: " + name);
        Equal(value.damage, Base("damage") * (name == "FireBall" || name == "Healing" ? SnapshotGrowth[boundary] : 1f), "damage");
        Equal(value.range, Base("range") * (name == "FireBall" || name == "Thunder" ? SnapshotGrowth[boundary] : 1f), "range");
        Equal(value.stunTime, name == "Blizzard" ? (Base("stunTime") > 0f ? Base("stunTime") : 2f) + SnapshotStunAdded[boundary] : Base("stunTime"), "stunTime");
        Equal(value.burfTime, name == "DemiGod" ? (Base("burfTime") > 0f ? Base("burfTime") : 5f) + SnapshotDurationAdded[boundary] : Base("burfTime"), "burfTime");
        Equal(value.coolTime, Base("coolTime"), "coolTime"); Equal(value.probablity, Base("probablity"), "probablity");
        Equal(value.multipleSkillDamageBuff, Base("multipleSkillDamageBuff"), "multipleSkillDamageBuff");
        Equal(value.multipleSkillResistanceBuff, Base("multipleSkillResistanceBuff"), "multipleSkillResistanceBuff");
        Equal(value.coefficient, Base("coefficient"), "coefficient");
        Equal(value.ArmourBonus, name == "LoneBleesing" ? SnapshotArmour[boundary] : 1f, "ArmourBonus");
        Equal(value.DamageReduction, name == "WaterDropBarrier" ? SnapshotReduction[boundary] : 0.5f, "DamageReduction");
        Equal(value.BerserkStatBonus, name == "AngerArmy" ? SnapshotBerserk[boundary] : 0f, "BerserkStatBonus");
        Equal(value.AvoidanceBonus, name == "SerenBlessing" ? SnapshotAvoidance[boundary] : 25f, "AvoidanceBonus");
        Equal(value.AttackBonus, name == "BeleifWave" ? SnapshotAttack[boundary] : 0.15f, "AttackBonus");
        bool threeStats = name == "KingSlime" || name == "SkeletonArcher";
        Equal(value.SummonHealthMultiplier, threeStats || name == "Dwarf" ? SnapshotGrowth[boundary] : 1f, "SummonHealthMultiplier");
        Equal(value.SummonAttackMultiplier, threeStats ? SnapshotGrowth[boundary] : 1f, "SummonAttackMultiplier");
        Equal(value.SummonArmourMultiplier, threeStats ? SnapshotGrowth[boundary] : 1f, "SummonArmourMultiplier");
        Equal(value.GoldBonus, name == "GoldyBlessing" ? SnapshotGold[boundary] : 0f, "GoldBonus");
    }

    private IEnumerator SkillLevelSnapshotContracts()
    {
        yield return EnsureFixtureLobby();
        var table = LunaDataTable.Instance;
        var sources = Resources.LoadAll<SkillData>("ScriptableObjects/LunaSkills").ToDictionary(source => source.name);
        var rows = sources.ToDictionary(pair => pair.Key, pair => LunaSkillBalance.GetRow(pair.Value));
        var originalLevels = table.skiiTable.ToDictionary(pair => pair.Key, pair => pair.Value.level);
        var runtimeJson = table.skiiTable.ToDictionary(pair => pair.Key, pair => JsonUtility.ToJson(pair.Value));
        var sourceJson = sources.ToDictionary(pair => pair.Key, pair => JsonUtility.ToJson(pair.Value));
        var oldSkills = table.playerData.equippedSkill.ToList(); var oldSkin = table.playerData.SkinType;
        string csv = Resources.Load<TextAsset>(LunaSkillBalance.ResourceName).text;
        var view = LunaSession.Current.Players[(int)PlayerData.eLunaSkin.Basic].GetComponent<UnitView>();
        try
        {
            table.playerData.SkinType = PlayerData.eLunaSkin.Basic;
            for (int boundary = 0; boundary < SnapshotLevels.Length; boundary++)
            {
                int level = SnapshotLevels[boundary];
                if (!Includes("skill-levels", "snapshot-boundary/level" + level)) continue;
                BeginCase("skill-levels", "snapshot-boundary/level" + level);
                Require(PlayerSkillNames.Length == 14 && sources.Count == 32, "Skill boundary fixture coverage changed.");
                foreach (var pair in table.skiiTable)
                {
                    pair.Value.level = level;
                    string before = JsonUtility.ToJson(pair.Value);
                    CheckSkillLevelSnapshot(new PlayerSkillSnapshot(pair.Value), sources[pair.Key], rows[pair.Key], level, boundary);
                    Require(JsonUtility.ToJson(pair.Value) == before, "Snapshot construction modified its runtime source: " + pair.Key);
                    foreach (string field in LunaSkillBalanceRow.BalanceFields)
                        Require(Equals(typeof(SkillData).GetField(field).GetValue(pair.Value), rows[pair.Key].GetValue(field)), "Skill growth modified a CSV base field: " + pair.Key + "." + field);
                }
                // Use the real typed request factory, in legal three-slot loadout batches.
                for (int start = 0; start < PlayerSkillNames.Length; start += 3)
                {
                    var names = PlayerSkillNames.Skip(start).Take(3).ToArray();
                    table.playerData.equippedSkill = names.ToList();
                    foreach (string name in names) table.skiiTable[name].level = level;
                    var request = UnitData.CreateFrom(view, Vector3.zero);
                    try
                    {
                        Require(request.PlayerLoadout != null && request.PlayerLoadout.Skills.Count == names.Length, "Typed player request did not capture its complete loadout.");
                        foreach (string name in names) table.skiiTable[name].level = 25;
                        foreach (var snapshot in request.PlayerLoadout.Skills)
                            CheckSkillLevelSnapshot(snapshot, sources[snapshot.name], rows[snapshot.name], level, boundary);
                    }
                    finally
                    {
                        ReferencePool.Release(request);
                        foreach (string name in names) table.skiiTable[name].level = level;
                    }
                }
                if (level == 20)
                    foreach (int cappedLevel in new[] { 24, 25 })
                        foreach (string name in PlayerSkillNames)
                        {
                            table.skiiTable[name].level = cappedLevel;
                            CheckSkillLevelSnapshot(new PlayerSkillSnapshot(table.skiiTable[name]), sources[name], rows[name], cappedLevel, boundary);
                        }
                foreach (var pair in originalLevels) table.skiiTable[pair.Key].level = pair.Value;
                Require(Resources.Load<TextAsset>(LunaSkillBalance.ResourceName).text == csv &&
                    sources.All(pair => JsonUtility.ToJson(pair.Value) == sourceJson[pair.Key]) &&
                    table.skiiTable.All(pair => JsonUtility.ToJson(pair.Value) == runtimeJson[pair.Key]), "Boundary capture changed base CSV, art SO, or restored runtime data.");
                current.observations.Add("14 skill +18 equipment snapshots; expected milestone tier=" + SnapshotTiers[boundary] + "; five pre-Show request batches retained their captured level after global level25 changes." +
                    (level == 20 ? " Levels24/25 also retained exactly the level20 effective cap." : ""));
                EndCase("Explicit boundary scalar/policy oracles matched all skills; gear base values, art references and CSV stayed unchanged; typed request snapshots survived later progression changes.");
            }
        }
        finally
        {
            foreach (var pair in originalLevels) table.skiiTable[pair.Key].level = pair.Value;
            table.playerData.equippedSkill = oldSkills; table.playerData.SkinType = oldSkin;
        }
    }
}
#endif

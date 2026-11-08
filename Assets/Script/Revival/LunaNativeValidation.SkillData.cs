#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using GameFramework;
using GameFramework.DataTable;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    private IEnumerator NativeSkillDataContracts()
    {
        if (!Includes("skills", "native-csv-balance-and-equipment-snapshots")) yield break;
        yield return EnsureFixtureLobby();
        BeginCase("skills", "native-csv-balance-and-equipment-snapshots");
        var sources = Resources.LoadAll<SkillData>("ScriptableObjects/LunaSkills").OrderBy(source => source.name).ToArray();
        var table = LunaDataTable.Instance;
        var native = GameFrameworkEntry.GetModule<IDataTableManager>().GetDataTable<LunaSkillBalanceRow>(LunaSkillBalance.TableName);
        Require(sources.Length == 32 && native != null && ReferenceEquals(native, LunaSkillBalance.Table) && native.Count == sources.Length,
            "Native SkillBalance table does not cover all fourteen skills and eighteen equipment definitions.");
        Require(!LunaSkillBalanceRow.BalanceFields.Contains("level"), "Mutable skill progression was folded into static CSV balance.");
        var sourceJson = sources.ToDictionary(source => source, source => JsonUtility.ToJson(source));
        foreach (var source in sources)
        {
            Require(table.skiiTable.TryGetValue(source.name, out SkillData runtime) && runtime != null && runtime != source,
                "Skill runtime view is missing or aliases its immutable source: " + source.name);
            var row = LunaSkillBalance.GetRow(source);
            Require(ReferenceEquals(native.GetDataRow(row.Id), row), "Skill lookup returned a row outside the native Game Framework table.");
            Require(Resources.Load<SkillData>(row.ResourcePath) == source, "Skill row resource identity differs from the authored source.");
            foreach (string fieldName in LunaSkillBalanceRow.BalanceFields)
            {
                var field = typeof(SkillData).GetField(fieldName);
                Require(field != null && Equals(field.GetValue(runtime), row.GetValue(fieldName)),
                    "Runtime skill balance differs from its native CSV row: " + source.name + "." + fieldName);
            }
            Require(runtime.effectPrefab == source.effectPrefab && runtime.skillImage == source.skillImage,
                "CSV initialization replaced a skill's immutable art references.");
        }
        // Parse the actual production document under a comma-decimal culture. Runtime
        // data must have the same typed values, not merely the same number of rows.
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var parsed = LunaSkillBalanceRow.ParseDocument(Resources.Load<TextAsset>(LunaSkillBalance.ResourceName).text);
            Require(parsed.Count == native.Count, "Skill CSV row coverage depends on process culture.");
            foreach (var row in parsed)
                foreach (string field in LunaSkillBalanceRow.BalanceFields)
                    Require(Equals(row.GetValue(field), native.GetDataRow(row.Id).GetValue(field)), "Skill CSV numeric/enum parsing depends on process culture: " + field);
        }
        finally { CultureInfo.CurrentCulture = culture; }

        var gearSource = sources.Single(source => source.name == "LunaEquipmentWoodSword");
        var gear = table.skiiTable[gearSource.name];
        int originalLevel = gear.level; float originalMultiplier = gear.multipleSkillDamageBuff;
        var originalSkin = table.playerData.SkinType;
        int round = NewFixtureRound();
        try
        {
            table.playerData.SkinType = PlayerData.eLunaSkin.Wood;
            table.SKillLevelUp(gear.name);
            int capturedLevel = gear.level;
            gear.multipleSkillDamageBuff = 2.75f;
            var prefab = LunaSession.Current.Players[(int)PlayerData.eLunaSkin.Wood];
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), new Vector3(0f, -50f));
            data.RoundId = round; data.InitialMode = UnitSpawnMode.Staged;
            var capturedLoadout = data.PlayerLoadout;
            // Change the live upgrade view after request construction. This must not
            // alter the coefficients used on the player's first native OnShow.
            table.SKillLevelUp(gear.name); gear.multipleSkillDamageBuff = 3.5f;
            PlayerLogic player = null;
            yield return ShowFixture<PlayerLogic>(prefab, data, actor => player = actor);
            Require(player.Abilities != null && ReferenceEquals(player.Abilities.Loadout, capturedLoadout), "Equipment owner did not consume its pre-Show request snapshot.");
            var captured = player.Abilities.Loadout.Equipment.Single(skill => skill.name == gearSource.name);
            Require(captured.level == capturedLevel && captured.multipleSkillDamageBuff == 2.75f &&
                Mathf.Abs(player.coefficient.SkillPoint - 2.75f) < 0.001f,
                "Equipment captured authored SO defaults/later global values instead of the requested runtime upgrade and applied multiplier.");
            int savedLevel = gear.level;
            Require(LunaRuntimeData.SaveNow(), "Saving equipment progression failed in the isolated directory.");
            gear.level = 1;
            Require(LunaRuntimeData.Reload() && gear.level == savedLevel, "Equipment upgrade progression did not survive actual save/reload.");
            Require(captured.level == capturedLevel && captured.multipleSkillDamageBuff == 2.75f &&
                Mathf.Abs(player.coefficient.SkillPoint - 2.75f) < 0.001f, "Save reload changed the living player's immutable equipment request.");
            foreach (var source in sources)
                Require(JsonUtility.ToJson(source) == sourceJson[source], "Runtime skill upgrades/snapshots changed the source asset: " + source.name);
            current.observations.Add("Native rows=" + native.Count + "; checked numeric/enum/bool fields=" + LunaSkillBalanceRow.BalanceFields.Length +
                "; de-DE parsing unchanged; requested WoodSword level=" + capturedLevel + "/multiplier2.75; later runtime level=" + savedLevel + "/multiplier3.5; live player coefficient remained2.75.");
        }
        finally
        {
            gear.level = originalLevel; gear.multipleSkillDamageBuff = originalMultiplier;
            table.playerData.SkinType = originalSkin;
            LunaSpawn.CancelRound(round);
            Require(LunaRuntimeData.SaveNow(), "Restoring the isolated equipment progression fixture failed.");
        }
        yield return RetireFixture(round);
        EndCase("All32 native CSV rows configure independent runtime views; culture parsing and art identity held; runtime equipment upgrades were captured before Show, applied to real native player stats, persisted, and never mutated source assets or an already living player.");
    }
}
#endif

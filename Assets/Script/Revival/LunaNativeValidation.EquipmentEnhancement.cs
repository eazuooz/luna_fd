#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GameFramework;
using GameFramework.Setting;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class LunaNativeValidation
{
    private const string EquipmentProgressKey = "Luna.Progress.v1";
    private const string EquipmentNonceKey = "Luna.Validation.EquipmentNonce";
    private const string SkullSwordKey = "LunaEquipmentSkullSword";

    private IEnumerator EquipmentEnhancementContract()
    {
        const int designId = (int)PlayerData.eLunaSkin.SkullAx;
        const string caseName = "equipment-stone-skull-axe";
        var mode = Environment.GetEnvironmentVariable("LUNA_EQUIPMENT_VALIDATION") ?? "";
        Require(mode == "" || mode == "seed" || mode == "reopen", "Unknown isolated equipment validation mode.");
        var table = LunaDataTable.Instance;
        var design = (EquipDesignItemData)table.itemTable[designId];
        var stones = table.itemTable[LunaEquipmentEnhancements.StoneId];
        var settings = GameFrameworkEntry.GetModule<ISettingManager>();
        var rule = LunaEquipmentEnhancements.Find(designId);
        Require(rule != null && rule.Rank == 1 && rule.StoneCost == 2 && rule.EquipmentSkill == SkullSwordKey &&
            rule.PartyAttackBonusPoints == 5, "The authorized Skull Axe enhancement row differs.");
        string noncePath = Path.Combine(Environment.GetEnvironmentVariable("LUNA_VALIDATION_DIRECTORY"), "equipment-nonce.txt");
        if (mode == "reopen")
        {
            BeginCase("ui", caseName);
            Require(File.Exists(noncePath) && settings.GetString(EquipmentNonceKey, "") == File.ReadAllText(noncePath),
                "A separate process did not load the equipment payment generation.");
            Require(design.EnhancementRank == 1 && design.level == 4 && stones.itemCount == 0 &&
                table.playerData.SkinType == PlayerData.eLunaSkin.SkullAx && EquipmentSwordBonus(PlayerLoadoutSnapshot.Capture()) == 15f,
                "The reopened equipment rank, paid stones, 4/4 parts or effective attack differ.");
            yield return UiLobby();
            yield return OpenNativeUi(UI_TYPE.UPGRADE);
            yield return OpenSkullEquipmentForm();
            var reopened = NativeUi(UI_TYPE.EQUIPDESIGN);
            Require(!reopened.transform.Find("LevelUpButton").GetComponent<Button>().interactable &&
                reopened.transform.Find("Weapon/Text").GetComponent<Text>().text.Contains("15%"),
                "The reopened native form offered another purchase or showed the old effect.");
            yield return CloseNativeUi(UI_TYPE.EQUIPDESIGN);
            yield return CloseNativeUi(UI_TYPE.UPGRADE);
            EndCase("A second process read the same paid equipment generation and the native form retained the completed +15% set bonus.");
            yield break;
        }

        int originalStones = stones.itemCount, originalRank = design.EnhancementRank, originalParts = design.level;
        var originalSkin = table.playerData.SkinType;
        int originalSelection = LunaSession.Current.EquipDesignNumber;
        bool keptSeed = false;
        int round = 0;
        try
        {
            yield return UiLobby();
            BeginCase("ui", caseName);
            Require(design.level == 4, "The authored Skull Axe part count differs.");
            table.playerData.SkinType = PlayerData.eLunaSkin.SkullAx;
            design.EnhancementRank = 0;
            stones.itemCount = 1;
            Require(LunaRuntimeData.SaveNow(), "Could not seed the isolated equipment save.");

            string legacy = Regex.Replace(settings.GetString(EquipmentProgressKey), @",""equipmentRank"":\d+", "");
            Require(!legacy.Contains("equipmentRank"), "The fixture must use a legacy payload without equipment ranks.");
            settings.SetString(EquipmentProgressKey, legacy);
            Require(settings.Save(), "Could not write the legacy equipment payload.");
            design.EnhancementRank = 1;
            Require(LunaRuntimeData.Reload() && design.EnhancementRank == 0 && design.level == 4 && stones.itemCount == 1,
                "The legacy snapshot did not default enhancement rank to zero while preserving the four parts and stones.");

            var before = PlayerLoadoutSnapshot.Capture();
            var activeLevels = table.playerData.equippedSkill.Select(key => table.skiiTable[key].level).ToArray();
            var wood = (EquipDesignItemData)table.itemTable[(int)PlayerData.eLunaSkin.Wood];
            table.playerData.SkinType = PlayerData.eLunaSkin.Wood;
            float woodCoefficient = PlayerLoadoutSnapshot.Capture().Equipment.Single(skill => skill.name == "LunaEquipmentWoodSword").multipleSkillDamageBuff;
            table.playerData.SkinType = PlayerData.eLunaSkin.SkullAx;

            yield return OpenNativeUi(UI_TYPE.UPGRADE);
            yield return OpenSkullEquipmentForm();
            var equipment = NativeUi(UI_TYPE.EQUIPDESIGN);
            var button = equipment.transform.Find("LevelUpButton").GetComponent<Button>();
            var caption = button.transform.Find("EquipmentEnhanceCaption").GetComponent<Text>();
            Require(button.gameObject.activeSelf && !button.interactable && caption.text.Contains("+10%") && caption.text.Contains("+15%"),
                "The authored equipment button did not show the two real effects or block insufficient material.");
            button.onClick.Invoke();
            Require(stones.itemCount == 1 && design.EnhancementRank == 0, "An insufficient purchase changed equipment progress.");

            yield return CloseNativeUi(UI_TYPE.EQUIPDESIGN);
            stones.itemCount = 2;
            Require(LunaRuntimeData.SaveNow() && LunaRuntimeData.SaveNow(), "Could not seed both committed save generations.");
            yield return OpenSkullEquipmentForm();
            equipment = NativeUi(UI_TYPE.EQUIPDESIGN);
            button = equipment.transform.Find("LevelUpButton").GetComponent<Button>();
            caption = button.transform.Find("EquipmentEnhanceCaption").GetComponent<Text>();
            Require(button.interactable && !equipment.transform.Find("Melisa").gameObject.activeSelf,
                "The paid Skull Axe button or resource presentation differs.");

            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                string primary = Path.Combine(LunaRuntimeData.SaveDirectory, "LunaSave.dat");
                string backup = primary + ".bak";
                byte[] oldPrimary = File.ReadAllBytes(primary), oldBackup = File.ReadAllBytes(backup);
                string oldPayload = settings.GetString(EquipmentProgressKey);
                int warningsBefore = report.warnings.Count;
                using (var locked = new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    button.onClick.Invoke();
                    Require(button.interactable && caption.text.Contains("취소") && stones.itemCount == 2 &&
                        design.EnhancementRank == 0 && settings.GetString(EquipmentProgressKey) == oldPayload &&
                        File.ReadAllBytes(primary).SequenceEqual(oldPrimary) && File.ReadAllBytes(backup).SequenceEqual(oldBackup),
                        "The blocked physical save did not restore count, rank, cached progress and both committed files.");
                }
                var warnings = report.warnings.Skip(warningsBefore).ToArray();
                Require(warnings.Length == 1 && warnings[0].StartsWith("Could not save Luna progress:", StringComparison.Ordinal),
                    "The intentional write-lock failure did not produce its expected diagnostic.");
                report.expectedWarnings.AddRange(warnings);
            }

            string nonce = mode == "seed" ? Guid.NewGuid().ToString("N") : "";
            if (mode == "seed") settings.SetString(EquipmentNonceKey, nonce);
            button.onClick.Invoke();
            Require(!button.interactable && caption.text.Contains("완료") && equipment.transform.Find("Weapon/Text").GetComponent<Text>().text.Contains("15%") &&
                stones.itemCount == 0 && design.EnhancementRank == 1 && design.level == 4,
                "One real authored button event did not spend exactly two stones and apply exactly one rank.");
            Require(EquipmentSwordBonus(before) == 10f && EquipmentSwordBonus(PlayerLoadoutSnapshot.Capture()) == 15f &&
                table.skiiTable[SkullSwordKey].coefficient == 10f && table.playerData.equippedSkill.Select(key => table.skiiTable[key].level).SequenceEqual(activeLevels),
                "The enhancement changed an immutable old loadout, the authored base or an active skill.");
            button.onClick.Invoke();
            Require(stones.itemCount == 0 && design.EnhancementRank == 1, "A capped/duplicate purchase changed equipment progress.");
            Require(!LunaEconomy.TryEnhanceEquipment(wood, out _) && wood.EnhancementRank == 0 && stones.itemCount == 0,
                "An unsupported kit accepted the enhancement payment.");

            yield return CloseNativeUi(UI_TYPE.EQUIPDESIGN);
            design.EnhancementRank = 0;
            stones.itemCount = 99;
            Require(LunaRuntimeData.Reload() && stones.itemCount == 0 && design.EnhancementRank == 1 && design.level == 4,
                "The saved payment did not survive reload of its physical snapshot.");
            yield return OpenSkullEquipmentForm();
            button = NativeUi(UI_TYPE.EQUIPDESIGN).transform.Find("LevelUpButton").GetComponent<Button>();
            Require(!button.interactable, "Reopening the equipment form enabled a capped purchase.");
            yield return CloseNativeUi(UI_TYPE.EQUIPDESIGN);

            LunaSession.Current.EquipDesignNumber = wood.itemNumber;
            yield return OpenNativeUi(UI_TYPE.EQUIPDESIGN);
            equipment = NativeUi(UI_TYPE.EQUIPDESIGN);
            var equipWood = equipment.transform.Find("EquipButton").GetComponent<Button>();
            Require(!equipment.transform.Find("LevelUpButton").gameObject.activeSelf && !equipment.transform.Find("Keystone").gameObject.activeSelf &&
                !equipment.transform.Find("Melisa").gameObject.activeSelf && equipWood.gameObject.activeSelf && equipWood.interactable,
                "The unsupported kit displayed its mock enhancement controls or hid its original equip button.");
            equipWood.onClick.Invoke();
            Require(table.playerData.SkinType == PlayerData.eLunaSkin.Wood &&
                PlayerLoadoutSnapshot.Capture().Equipment.Single(skill => skill.name == "LunaEquipmentWoodSword").multipleSkillDamageBuff == woodCoefficient,
                "The original Wood equip action or its captured coefficient changed.");
            yield return Until(() => LunaSpawn.PendingCount == 0, "Original Wood equipment preview refresh");
            yield return CloseNativeUi(UI_TYPE.EQUIPDESIGN);
            yield return CloseNativeUi(UI_TYPE.UPGRADE);

            table.playerData.SkinType = PlayerData.eLunaSkin.SkullAx;
            round = NewFixtureRound();
            var allyPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
            var allyData = UnitData.CreateFrom(allyPrefab.GetComponent<UnitView>(), Vector3.zero);
            allyData.RoundId = round; allyData.Side = eUnitSide.Allied; allyData.InitialMode = UnitSpawnMode.Staged;
            UnitLogic ally = null;
            yield return ShowFixture<UnitLogic>(allyPrefab, allyData, value => ally = value);
            HoldAutomaticActions(ally);
            float baseAttack = ally.stat.AttackDamage;
            var ownerPrefab = LunaSession.Current.Players[designId];
            var ownerData = UnitData.CreateFrom(ownerPrefab.GetComponent<UnitView>(), Vector3.left * 3f);
            ownerData.RoundId = round; ownerData.Side = eUnitSide.Allied; ownerData.InitialMode = UnitSpawnMode.Staged;
            PlayerLogic owner = null;
            yield return ShowFixture<PlayerLogic>(ownerPrefab, ownerData, value => owner = value);
            HoldAutomaticActions(owner);
            Require(baseAttack > 0f && EquipmentSwordBonus(owner.Abilities.Loadout) == 15f &&
                Mathf.Abs(ally.stat.AttackDamage - baseAttack * 1.15f) < 0.05f,
                "A real equipped player did not apply the +15% party attack to its own round.");
            yield return RetireFixture(round);
            round = 0;

            if (mode == "seed")
            {
                File.WriteAllText(noncePath, nonce);
                keptSeed = true;
            }
            EndCase("The real equipment form exercised a legacy save, insufficient and capped no-ops, physical save rollback, exact payment, save/reopen, unchanged other skills, and the live +15% party attack.");
        }
        finally
        {
            if (round != 0) LunaSpawn.CancelRound(round);
            LunaSession.Current.EquipDesignNumber = originalSelection;
            if (!keptSeed)
            {
                table.playerData.SkinType = originalSkin;
                stones.itemCount = originalStones;
                design.EnhancementRank = originalRank;
                design.level = originalParts;
                Require(LunaRuntimeData.SaveNow(), "Could not restore the isolated equipment fixture.");
            }
        }
    }

    private IEnumerator OpenSkullEquipmentForm()
    {
        var upgrade = NativeUi(UI_TYPE.UPGRADE);
        var card = LunaUIViews.FindChildObject(upgrade.gameObject, "luna_skull_ax");
        var details = LunaUIViews.FindComponentByChildObject<Button>(card, "DesignButton");
        Require(details != null, "The authored Skull Axe details button is missing.");
        details.onClick.Invoke();
        yield return Until(() => NativeUi(UI_TYPE.EQUIPDESIGN) != null, "Actual Skull Axe design selection");
        yield return null;
    }

    private static float EquipmentSwordBonus(PlayerLoadoutSnapshot snapshot) =>
        snapshot.Equipment.Single(skill => skill.name == SkullSwordKey).coefficient;
}
#endif

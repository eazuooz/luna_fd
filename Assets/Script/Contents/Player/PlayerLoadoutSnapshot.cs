using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Immutable ability values captured before the asynchronous player show request.</summary>
    public sealed class PlayerSkillSnapshot
    {
        public readonly string name;
        public readonly GameObject effectPrefab;
        public readonly Sprite skillImage;
        public readonly int level, mana, maxSummonCount;
        public readonly float damage, range, coolTime, stunTime, burfTime, probablity;
        public readonly float multipleSkillDamageBuff, multipleSkillResistanceBuff, coefficient;
        public readonly bool ableSkill;
        public readonly int UpgradeTier;
        public readonly float ArmourBonus, DamageReduction, BerserkStatBonus, AvoidanceBonus, AttackBonus;
        public readonly float SummonHealthMultiplier, SummonAttackMultiplier, SummonArmourMultiplier, GoldBonus;

        internal PlayerSkillSnapshot(SkillData source)
        {
            name = source.name; effectPrefab = source.effectPrefab; skillImage = source.skillImage;
            level = source.level; mana = source.mana; maxSummonCount = source.maxSummonCount;
            damage = source.damage; range = source.range; coolTime = source.coolTime;
            stunTime = source.stunTime; burfTime = source.burfTime; probablity = source.probablity;
            multipleSkillDamageBuff = source.multipleSkillDamageBuff;
            multipleSkillResistanceBuff = source.multipleSkillResistanceBuff;
            coefficient = source.coefficient; ableSkill = source.ableSkill;
            // Only the four authored milestones grant growth. CSV rows remain the
            // immutable base; the player's request captures the effective values.
            UpgradeTier = Mathf.Clamp(level / 5, 0, 4);
            ArmourBonus = 1f; DamageReduction = 0.5f; AvoidanceBonus = 25f; AttackBonus = 0.15f;
            SummonHealthMultiplier = SummonAttackMultiplier = SummonArmourMultiplier = 1f;
            switch (name)
            {
                case "FireBall": damage *= 1f + UpgradeTier * 0.1f; range *= 1f + UpgradeTier * 0.1f; break;
                case "Blizzard": stunTime = (stunTime > 0f ? stunTime : 2f) + UpgradeTier * 0.5f; break;
                case "Healing": damage *= 1f + UpgradeTier * 0.1f; break;
                case "Thunder": range *= 1f + UpgradeTier * 0.1f; break;
                // Roster gold upgrades are worth three percentage points each.
                // Retain the previous eighteen-point base and grant three per tier.
                case "GoldyBlessing": GoldBonus = 6f + UpgradeTier; break;
                case "LoneBleesing": ArmourBonus += UpgradeTier * 0.05f; break;
                case "WaterDropBarrier": DamageReduction += UpgradeTier * 0.1f; break;
                case "Dwarf": SummonHealthMultiplier += UpgradeTier * 0.1f; break;
                case "AngerArmy": BerserkStatBonus = UpgradeTier * 0.1f; break;
                case "DemiGod": burfTime = (burfTime > 0f ? burfTime : 5f) + UpgradeTier * 5f; break;
                case "SerenBlessing": AvoidanceBonus += UpgradeTier * 5f; break;
                case "BeleifWave": AttackBonus += UpgradeTier * 0.03f; break;
                case "KingSlime": case "SkeletonArcher":
                    SummonHealthMultiplier = SummonAttackMultiplier = SummonArmourMultiplier = 1f + UpgradeTier * 0.1f;
                    break;
            }
        }
    }

    public sealed class PlayerLoadoutSnapshot
    {
        public IReadOnlyList<PlayerSkillSnapshot> Skills { get; }
        public IReadOnlyList<PlayerSkillSnapshot> Equipment { get; }

        private PlayerLoadoutSnapshot(PlayerSkillSnapshot[] skills, PlayerSkillSnapshot[] equipment)
        {
            Skills = Array.AsReadOnly(skills); Equipment = Array.AsReadOnly(equipment);
        }

        public static PlayerLoadoutSnapshot Capture()
        {
            var table = LunaDataTable.Instance;
            var skills = table.playerData.equippedSkill.Select(key =>
            {
                if (!table.skiiTable.TryGetValue(key, out SkillData skill) || skill == null)
                    throw new InvalidOperationException("Player loadout skill is missing: " + key);
                return new PlayerSkillSnapshot(skill);
            }).ToArray();
            var design = table.GetCurrentEquipDesignItemData();
            var equipment = design == null ? Array.Empty<PlayerSkillSnapshot>() :
                design.equipSkills.Where(source => source != null).Select(source =>
                {
                    if (!table.skiiTable.TryGetValue(source.name, out SkillData skill) || skill == null)
                        throw new InvalidOperationException("Player equipment skill is missing: " + source.name);
                    return new PlayerSkillSnapshot(skill);
                }).ToArray();
            return new PlayerLoadoutSnapshot(skills, equipment);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using GameFramework;
using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    public sealed class LunaEquipmentEnhancementRow : DataRowBase
    {
        internal static readonly string[] Header = { "DesignId", "Rank", "StoneCost", "EquipmentSkill", "PartyAttackBonusPoints" };
        private int designId;
        public override int Id => designId;
        public int Rank { get; private set; }
        public int StoneCost { get; private set; }
        public string EquipmentSkill { get; private set; }
        public int PartyAttackBonusPoints { get; private set; }

        public override bool ParseDataRow(string source, object userData)
        {
            var cells = StageCsv.Cells(source, Header.Length);
            designId = StageCsv.Positive(cells[0]);
            Rank = StageCsv.Positive(cells[1]);
            StoneCost = StageCsv.Positive(cells[2]);
            EquipmentSkill = cells[3];
            PartyAttackBonusPoints = StageCsv.Positive(cells[4]);
            if (Rank != 1 || string.IsNullOrWhiteSpace(EquipmentSkill) || PartyAttackBonusPoints > 100)
                throw new InvalidDataException("Invalid equipment enhancement for design " + designId + ".");
            return true;
        }
    }

    public static class LunaEquipmentEnhancements
    {
        public const int StoneId = 1000;
        private const string TableName = "EquipmentEnhancements";
        private static IDataTable<LunaEquipmentEnhancementRow> table;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => table = null;
        internal static void ReleaseRuntimeTable() => Reset();

        internal static void Initialize(LunaDataTable data)
        {
            if (table != null) return;
            var loaded = LunaStageTables.Load<LunaEquipmentEnhancementRow>(TableName, LunaEquipmentEnhancementRow.Header);
            try
            {
                if (!data.itemTable.TryGetValue(StoneId, out var material) || material.type != ItemData.ItemType.MATERIAL || loaded.Count == 0)
                    throw new InvalidDataException("The equipment enhancement catalogue is incomplete.");
                foreach (var row in loaded.GetAllDataRows())
                {
                    if (!data.itemTable.TryGetValue(row.Id, out var item) || !(item is EquipDesignItemData design) ||
                        !design.equipSkills.Any(part => part != null && part.name == row.EquipmentSkill) ||
                        !data.skiiTable.TryGetValue(row.EquipmentSkill, out var skill) || skill.type != SkillData.SkillType.Passive)
                        throw new InvalidDataException("Equipment enhancement references a missing design or passive.");
                }
                table = loaded;
            }
            catch
            {
                GameFrameworkEntry.GetModule<IDataTableManager>().DestroyDataTable(loaded);
                throw;
            }
        }

        public static LunaEquipmentEnhancementRow Find(int designId) =>
            table != null && table.HasDataRow(designId) ? table.GetDataRow(designId) : null;

        internal static int RestoreRank(int designId, int savedRank) =>
            Mathf.Clamp(savedRank, 0, Find(designId)?.Rank ?? 0);
    }
}

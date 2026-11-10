using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GameFramework;
using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    public enum LunaStageKind { Basic, Boss, Endless }
    public sealed class LunaStageRow : DataRowBase
    {
        public static readonly string[] Header = { "RowId", "Key", "Kind", "DisplayName", "Width", "TimeLimit", "StartGold", "Gold", "RandomY", "IsBossStage", "Background", "AllyTower", "EnemyTower", "Boss", "FirstClearGold", "FirstClearStones", "UnlockAfter", "RecommendedLevel", "ObjectiveHealth", "ObjectiveAttack", "Tip", "WaveHealth", "WaveAttack", "StartMana" };
        private int id;
        public override int Id => id;
        public string Key, DisplayName, Background, AllyTower, EnemyTower, Boss;
        public LunaStageKind Kind;
        public float Width, TimeLimit, ObjectiveHealth, ObjectiveAttack, WaveHealth, WaveAttack;
        public int StartGold, StartMana, Gold, FirstClearGold, FirstClearStones, UnlockAfter, RecommendedLevel;
        public string Tip;
        public bool RandomY, IsBossStage;
        public override bool ParseDataRow(string source, object userData)
        {
            var c = StageCsv.Cells(source, Header.Length);
            id = StageCsv.Positive(c[0]); Key = c[1];
            int kind = StageCsv.Integer(c[2]);
            if (!Enum.IsDefined(typeof(LunaStageKind), kind)) throw new InvalidDataException("Unknown stage kind.");
            Kind = (LunaStageKind)kind; DisplayName = c[3];
            Width = StageCsv.Number(c[4]); TimeLimit = StageCsv.Number(c[5]); StartGold = StageCsv.Integer(c[6]);
            Gold = StageCsv.Integer(c[7]); RandomY = StageCsv.Boolean(c[8]); IsBossStage = StageCsv.Boolean(c[9]);
            Background = c[10]; AllyTower = c[11]; EnemyTower = c[12]; Boss = c[13];
            FirstClearGold = StageCsv.Integer(c[14]); FirstClearStones = StageCsv.Integer(c[15]);
            UnlockAfter = StageCsv.Integer(c[16]); RecommendedLevel = StageCsv.Positive(c[17]);
            ObjectiveHealth = StageCsv.Number(c[18]); ObjectiveAttack = StageCsv.Number(c[19]); Tip = c[20];
            WaveHealth = StageCsv.Number(c[21]); WaveAttack = StageCsv.Number(c[22]);
            StartMana = StageCsv.Integer(c[23]);
            if (string.IsNullOrWhiteSpace(Key) || Width <= 0 || TimeLimit < 30 || TimeLimit > 180 ||
                StartGold < 0 || StartGold > 100 || StartMana < 0 || StartMana > 100 ||
                Gold < 0 || Gold > 100000 || FirstClearGold < 0 ||
                FirstClearGold > 100000 || FirstClearStones < 0 || FirstClearStones > 1000 ||
                UnlockAfter < 0 || UnlockAfter > 30 || RecommendedLevel > 20 ||
                ObjectiveHealth <= 0 || ObjectiveAttack <= 0 || WaveHealth <= 0 || WaveAttack <= 0 || string.IsNullOrWhiteSpace(Tip))
                throw new InvalidDataException("Invalid stage settings: " + Key);
            return true;
        }
    }
    public sealed class LunaStageWaveRow : DataRowBase
    {
        public static readonly string[] Header = { "RowId", "StageId", "Phase", "Prefab", "Count", "Level", "Order", "At" };
        private int id;
        public override int Id => id;
        public int StageId, Count, Level, Order;
        public float At;
        public string Phase, Prefab;
        public override bool ParseDataRow(string source, object userData)
        {
            var c = StageCsv.Cells(source, Header.Length);
            id = StageCsv.Positive(c[0]); StageId = StageCsv.Positive(c[1]); Phase = c[2]; Prefab = c[3];
            Count = StageCsv.Positive(c[4]); Level = StageCsv.Integer(c[5]); Order = StageCsv.Integer(c[6]);
            At = StageCsv.Number(c[7]);
            if ((Phase != "Start" && Phase != "Timed" && Phase != "Endless" && Phase != "BossPhase") ||
                Level < 1 || Level > 30 || Order < 0 || Count > 8 || At < 0 || Prefab.Length == 0)
                throw new InvalidDataException("Invalid stage wave: " + id);
            return true;
        }
    }
    public sealed class LunaStageRewardRow : DataRowBase
    {
        public static readonly string[] Header = { "RowId", "StageId", "Trigger", "Item", "Probability" };
        private int id;
        public override int Id => id;
        public int StageId;
        public string Trigger, Item;
        public float Probability;
        public override bool ParseDataRow(string source, object userData)
        {
            var c = StageCsv.Cells(source, Header.Length);
            id = StageCsv.Positive(c[0]); StageId = StageCsv.Positive(c[1]); Trigger = c[2]; Item = c[3]; Probability = StageCsv.Number(c[4]);
            if ((Trigger != "Victory" && Trigger != "FirstClear" && Trigger != "Monster" && Trigger != "EndlessBoss") || Item.Length == 0 || Probability < 0 || Probability > 1)
                throw new InvalidDataException("Invalid stage reward: " + id);
            return true;
        }
    }
    internal static class StageCsv
    {
        internal static string[] Cells(string source, int width)
        {
            var rows = LunaCsv.Read(source);
            if (rows.Count != 1 || rows[0].Length != width) throw new InvalidDataException("Invalid stage CSV record width.");
            return rows[0];
        }
        internal static int Integer(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        internal static int Positive(string value) { int n = Integer(value); if (n <= 0) throw new InvalidDataException("Expected positive row ID/count."); return n; }
        internal static float Number(string value)
        {
            float n = float.Parse(value, CultureInfo.InvariantCulture);
            if (float.IsNaN(n) || float.IsInfinity(n)) throw new InvalidDataException("Non-finite stage value.");
            return n;
        }
        internal static bool Boolean(string value)
        {
            if (value != "0" && value != "1") throw new InvalidDataException("Stage boolean must be 0 or 1.");
            return value == "1";
        }
    }
    /// <summary>Native DataTables are the sole source of stage rules, waves and reward probabilities.</summary>
    public static class LunaStageTables
    {
        public static IDataTable<LunaStageRow> Stages { get; private set; }
        public static IDataTable<LunaStageWaveRow> Waves { get; private set; }
        public static IDataTable<LunaStageRewardRow> Rewards { get; private set; }
        private static readonly Dictionary<string, LunaStageRow> byKey = new Dictionary<string, LunaStageRow>(StringComparer.Ordinal);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Stages = null; Waves = null; Rewards = null; byKey.Clear(); }
        internal static void ReleaseRuntimeTables() => Reset();
        public static void Initialize()
        {
            if (Stages != null) return;
            var manager = GameFrameworkEntry.GetModule<IDataTableManager>();
            IDataTable<LunaStageRow> stages = null;
            IDataTable<LunaStageWaveRow> waves = null;
            IDataTable<LunaStageRewardRow> rewards = null;
            try
            {
                stages = Load<LunaStageRow>("Stages", LunaStageRow.Header);
                waves = Load<LunaStageWaveRow>("StageWaves", LunaStageWaveRow.Header);
                rewards = Load<LunaStageRewardRow>("StageRewards", LunaStageRewardRow.Header);
                foreach (var row in stages.GetAllDataRows())
                {
                    if (byKey.ContainsKey(row.Key)) throw new InvalidDataException("Duplicate stage key " + row.Key);
                    byKey.Add(row.Key, row);
                    RequireAsset<GameObject>(row.Background);
                    if (row.Kind == LunaStageKind.Basic) { RequireAsset<GameObject>(row.AllyTower); RequireAsset<GameObject>(row.EnemyTower); }
                    if (row.Kind == LunaStageKind.Boss) RequireAsset<GameObject>(row.Boss);
                }
                foreach (var row in waves.GetAllDataRows())
                {
                    if (!stages.HasDataRow(row.StageId)) throw new InvalidDataException("Wave references missing stage.");
                }
                foreach (var row in rewards.GetAllDataRows())
                {
                    if (!stages.HasDataRow(row.StageId)) throw new InvalidDataException("Reward references missing stage.");
                    RequireAsset<ItemData>(row.Item);
                }
                foreach (var stage in stages.GetAllDataRows())
                    ValidateWavePlan(stage, Array.FindAll(waves.GetAllDataRows(), wave => wave.StageId == stage.Id));
                Stages = stages; Waves = waves; Rewards = rewards;
            }
            catch
            {
                if (stages != null) manager.DestroyDataTable(stages);
                if (waves != null) manager.DestroyDataTable(waves);
                if (rewards != null) manager.DestroyDataTable(rewards);
                byKey.Clear(); throw;
            }
        }
        public static void ValidateWavePlan(LunaStageRow stage, IEnumerable<LunaStageWaveRow> plan)
        {
            var groups = new Dictionary<float, int>();
            foreach (var wave in plan)
            {
                var view = RequireAsset<GameObject>(wave.Prefab).GetComponent<UnitView>();
                if (view == null || view.Definition == null || view.Definition.UnitSide != eUnitSide.Enemy)
                    throw new InvalidDataException("Wave must spawn an authored enemy: " + wave.Prefab);
                bool valid;
                switch (stage.Kind)
                {
                    case LunaStageKind.Basic:
                        valid = (wave.Phase == "Start" && wave.At == 0) ||
                            (wave.Phase == "Timed" && wave.At > 0 && wave.At < stage.TimeLimit);
                        break;
                    case LunaStageKind.Endless:
                        valid = wave.Phase == "Endless" && wave.At == 0 && wave.Order < 5;
                        break;
                    default:
                        valid = wave.Phase == "BossPhase" && wave.At > 0 && wave.At < 1;
                        break;
                }
                if (!valid) throw new InvalidDataException("Wave timing does not match stage " + stage.Key + ": " + wave.Id);
                float trigger = stage.Kind == LunaStageKind.Endless ? wave.Order : wave.At;
                groups.TryGetValue(trigger, out int count);
                groups[trigger] = count + wave.Count;
                if (groups[trigger] > 8) throw new InvalidDataException("Encounter burst exceeds eight enemies: " + stage.Key);
            }
            if (stage.Kind == LunaStageKind.Basic && (!groups.ContainsKey(0) || groups.Count < 2))
                throw new InvalidDataException("Campaign needs an opener and timed waves: " + stage.Key);
            if (stage.Kind == LunaStageKind.Endless && groups.Count != 5)
                throw new InvalidDataException("Endless needs five authored floor patterns: " + stage.Key);
            if (stage.Kind == LunaStageKind.Boss && groups.Count != 2)
                throw new InvalidDataException("Boss needs two distinct health thresholds: " + stage.Key);
        }
        private static IDataTable<T> Load<T>(string name, string[] header) where T : class, IDataRow, new()
        {
            var csv = Resources.Load<TextAsset>("DataTables/" + name);
            if (csv == null) throw new InvalidDataException("Missing CSV DataTables/" + name);
            var records = LunaCsv.Read(csv.text);
            if (records.Count == 0 || records[0].Length != header.Length) throw new InvalidDataException("Invalid " + name + " header.");
            for (int i = 0; i < header.Length; i++) if (records[0][i] != header[i]) throw new InvalidDataException("Invalid " + name + " column: " + header[i]);
            var manager = GameFrameworkEntry.GetModule<IDataTableManager>();
            var table = manager.CreateDataTable<T>(name);
            try
            {
                for (int i = 1; i < records.Count; i++) if (!table.AddDataRow(LunaCsv.WriteRow(records[i]), null)) throw new InvalidDataException("Rejected " + name + " row " + i);
                return table;
            }
            catch { manager.DestroyDataTable(table); throw; }
        }
        public static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(path) || path.Contains("..") || path.Contains("\\")) throw new InvalidDataException("Invalid resource path: " + path);
            return Resources.Load<T>(path) ?? throw new InvalidDataException("Missing resource: " + path);
        }
        public static LunaStageRow Get(string key)
        {
            Initialize();
            if (byKey.TryGetValue(key, out var row)) return row;
            throw new InvalidDataException("Unknown stage key: " + key);
        }
        public static List<LunaStageWaveRow> GetWaves(int stageId, string phase)
        {
            Initialize(); var result = new List<LunaStageWaveRow>();
            foreach (var row in Waves.GetAllDataRows()) if (row.StageId == stageId && row.Phase == phase) result.Add(row);
            result.Sort((a, b) =>
            {
                int timing = phase == "BossPhase" ? b.At.CompareTo(a.At) : a.At.CompareTo(b.At);
                if (timing != 0) return timing;
                int order = a.Order.CompareTo(b.Order);
                return order != 0 ? order : a.Id.CompareTo(b.Id);
            });
            return result;
        }
        public static bool IsUnlocked(LunaStageRow stage, PlayerData player)
        {
            if (stage.Kind == LunaStageKind.Basic) return stage.Id <= player.UnlockedFloor;
            return stage.UnlockAfter == 0 || player.UnlockedFloor > stage.UnlockAfter ||
                (stage.UnlockAfter == 30 && player.CampaignComplete);
        }
        public static List<LunaStageRewardRow> GetRewards(int stageId, string trigger)
        {
            Initialize(); var result = new List<LunaStageRewardRow>();
            foreach (var row in Rewards.GetAllDataRows()) if (row.StageId == stageId && row.Trigger == trigger) result.Add(row);
            return result;
        }
    }
}

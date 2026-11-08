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
    public sealed class LunaEntityParameterRow : DataRowBase
    {
        private int id;
        public override int Id => id;
        public string Key { get; private set; }
        public string Parameter { get; private set; }
        public float Value { get; private set; }
        public override bool ParseDataRow(string text, object userData)
        {
            var records = LunaCsv.Read(text);
            if (records.Count != 1 || records[0].Length != 4) throw new InvalidDataException("Entity parameter row requires four cells.");
            var cells = records[0];
            id = int.Parse(cells[0], CultureInfo.InvariantCulture);
            Key = cells[1]; Parameter = cells[2]; Value = float.Parse(cells[3], CultureInfo.InvariantCulture);
            if (id <= 0 || string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Parameter) || float.IsNaN(Value) || float.IsInfinity(Value))
                throw new InvalidDataException("Invalid entity parameter.");
            return true;
        }
    }
    public static class LunaEntityParameters
    {
        private static readonly Dictionary<string, float> values = new Dictionary<string, float>();
        private static bool initialized;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { initialized = false; values.Clear(); }
        internal static void ReleaseRuntimeTable() => Reset();
        public static void Initialize()
        {
            if (initialized) return;
            var csv = Resources.Load<TextAsset>("DataTables/EntityParameters");
            if (csv == null) throw new InvalidDataException("Missing entity parameter CSV.");
            var rows = LunaCsv.Read(csv.text);
            if (rows.Count == 0 || string.Join(",", rows[0]) != "RowId,Key,Parameter,Value") throw new InvalidDataException("Entity parameter CSV header differs.");
            var manager = GameFrameworkEntry.GetModule<IDataTableManager>();
            var table = manager.CreateDataTable<LunaEntityParameterRow>("EntityParameters");
            try
            {
                for (int i = 1; i < rows.Count; i++)
                {
                    var parsed = new LunaEntityParameterRow(); parsed.ParseDataRow(LunaCsv.WriteRow(rows[i]), null);
                    string key = parsed.Key + "/" + parsed.Parameter;
                    if (values.ContainsKey(key) || !table.AddDataRow(LunaCsv.WriteRow(rows[i]), null)) throw new InvalidDataException("Duplicate entity parameter " + key);
                    values.Add(key, parsed.Value);
                }
                initialized = true;
            }
            catch { values.Clear(); manager.DestroyDataTable(table); throw; }
        }
        public static float Get(string key, string parameter, float fallback)
        {
            if (!Application.isPlaying) return fallback;
            Initialize();
            return values.TryGetValue(key + "/" + parameter, out float value) ? value : fallback;
        }
    }
}

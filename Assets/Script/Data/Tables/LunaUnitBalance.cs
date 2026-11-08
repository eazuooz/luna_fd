using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using GameFramework.DataTable;
using UnityEngine;

/// <summary>CSV is the balance authority; saves overlay only mutable player progression.</summary>
public static class LunaUnitBalance
{
    public const string ResourceName = "DataTables/UnitBalance";
    public const string CsvAssetPath = "Assets/Resources/DataTables/UnitBalance.csv";
    public const string TableName = "UnitBalance";
    private static readonly Dictionary<ObjectData, LunaUnitBalanceRow> rowsBySource = new Dictionary<ObjectData, LunaUnitBalanceRow>();
    public static IDataTable<LunaUnitBalanceRow> Table { get; private set; }
    public static int Count => Table?.Count ?? 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { rowsBySource.Clear(); Table = null; }
    internal static void ReleaseRuntimeTable() => Reset();

    public static void Initialize()
    {
        if (Table != null) return;
        var csv = Resources.Load<TextAsset>(ResourceName);
        if (csv == null) throw new InvalidDataException("Missing unit balance CSV: " + CsvAssetPath + ". Run the initial exporter first.");
        var records = LunaCsv.Read(csv.text);
        var parsed = LunaUnitBalanceRow.ParseDocument(csv.text); // Validate before creating a partial framework table.
        var sourceRows = new Dictionary<ObjectData, LunaUnitBalanceRow>();
        foreach (var row in parsed)
        {
            var source = Resources.Load<ObjectData>(row.ResourcePath);
            if (source == null) throw new InvalidDataException("Unit balance resource is missing: " + row.ResourcePath);
            if (sourceRows.ContainsKey(source)) throw new InvalidDataException("Multiple unit rows map to " + row.ResourcePath);
            sourceRows.Add(source, row);
        }
        var manager = GameFrameworkEntry.GetModule<IDataTableManager>();
        var table = manager.CreateDataTable<LunaUnitBalanceRow>(TableName);
        try
        {
            for (int i = 1; i < records.Count; i++)
                if (!table.AddDataRow(LunaCsv.WriteRow(records[i]), null))
                    throw new InvalidDataException("Game Framework rejected unit balance row " + (i + 1));
            rowsBySource.Clear();
            foreach (var pair in sourceRows) rowsBySource.Add(pair.Key, table.GetDataRow(pair.Value.Id));
            Table = table;
        }
        catch
        {
            manager.DestroyDataTable(table);
            rowsBySource.Clear();
            throw;
        }
    }

    public static LunaUnitBalanceRow GetRow(ObjectData source)
    {
        Initialize();
        if (source != null && rowsBySource.TryGetValue(source, out var row)) return row;
        throw new InvalidDataException("ObjectData has no CSV balance row: " + (source != null ? source.name : "null"));
    }

    public static void Apply(ObjectData source, ObjectData runtimeView)
    {
        GetRow(source).ApplyTo(runtimeView);
    }
}

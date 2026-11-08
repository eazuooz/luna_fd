using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using GameFramework.DataTable;
using UnityEngine;

/// <summary>Native CSV authority for active skills and equipment. Saved level progression is applied separately.</summary>
public static class LunaSkillBalance
{
    public const string ResourceName = "DataTables/SkillBalance";
    public const string CsvAssetPath = "Assets/Resources/DataTables/SkillBalance.csv";
    public const string TableName = "SkillBalance";
    private static readonly Dictionary<SkillData, LunaSkillBalanceRow> rowsBySource = new Dictionary<SkillData, LunaSkillBalanceRow>();
    public static IDataTable<LunaSkillBalanceRow> Table { get; private set; }
    public static int Count => Table?.Count ?? 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { rowsBySource.Clear(); Table = null; }
    internal static void ReleaseRuntimeTable() => Reset();

    public static void Initialize()
    {
        if (Table != null) return;
        var csv = Resources.Load<TextAsset>(ResourceName);
        if (csv == null) throw new InvalidDataException("Missing skill balance CSV: " + CsvAssetPath + ". Run the initial exporter first.");
        var records = LunaCsv.Read(csv.text);
        var parsed = LunaSkillBalanceRow.ParseDocument(csv.text); // Validate before creating a partial framework table.
        var sourceRows = new Dictionary<SkillData, LunaSkillBalanceRow>();
        foreach (var row in parsed)
        {
            var source = Resources.Load<SkillData>(row.ResourcePath);
            if (source == null) throw new InvalidDataException("Skill balance resource is missing: " + row.ResourcePath);
            if (sourceRows.ContainsKey(source)) throw new InvalidDataException("Multiple skill rows map to " + row.ResourcePath);
            sourceRows.Add(source, row);
        }
        var allSources = Resources.LoadAll<SkillData>("ScriptableObjects/LunaSkills");
        if (allSources.Length != sourceRows.Count)
            throw new InvalidDataException("Skill balance coverage differs from source assets.");
        foreach (var source in allSources)
            if (!sourceRows.ContainsKey(source)) throw new InvalidDataException("Skill source has no CSV balance row: " + source.name);
        var manager = GameFrameworkEntry.GetModule<IDataTableManager>();
        var table = manager.CreateDataTable<LunaSkillBalanceRow>(TableName);
        try
        {
            for (int i = 1; i < records.Count; i++)
                if (!table.AddDataRow(LunaCsv.WriteRow(records[i]), null))
                    throw new InvalidDataException("Game Framework rejected skill balance row " + (i + 1));
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

    public static LunaSkillBalanceRow GetRow(SkillData source)
    {
        Initialize();
        if (source != null && rowsBySource.TryGetValue(source, out var row)) return row;
        throw new InvalidDataException("SkillData has no CSV balance row: " + (source != null ? source.name : "null"));
    }

    public static void Apply(SkillData source, SkillData runtimeView)
    {
        GetRow(source).ApplyTo(runtimeView);
    }
}

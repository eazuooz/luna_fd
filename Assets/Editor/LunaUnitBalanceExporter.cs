using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class LunaUnitBalanceExporter
{
    [MenuItem("Luna/Data/Export Initial Unit Balance CSV")]
    public static void ExportInitialTable()
    {
        EnsureEditMode();
        if (File.Exists(LunaUnitBalance.CsvAssetPath))
            throw new InvalidOperationException("UnitBalance.csv is already the balance authority. Initial export refuses to overwrite it. Use Sync New Unit Sources to add assets without replacing edited balance.");
        var sources = Sources();
        var records = new List<string[]> { LunaUnitBalanceRow.Header };
        int id = 1;
        foreach (var source in sources)
            records.Add(LunaUnitBalanceRow.Capture(id++, source.Key, ResourcePath(source.Value), AssetDatabase.LoadAssetAtPath<ObjectData>(source.Value)));
        Write(records);
        ValidateTable();
        Debug.Log("Exported " + sources.Count + " ObjectData assets and " + LunaUnitBalanceRow.BalanceFields.Length + " balance fields to the Game Framework unit CSV.");
    }

    [MenuItem("Luna/Data/Sync New Unit Sources and Paths")]
    public static void SyncNewUnitsAndPaths()
    {
        EnsureEditMode();
        var rows = LunaUnitBalanceRow.ParseDocument(File.ReadAllText(LunaUnitBalance.CsvAssetPath));
        var records = LunaCsv.Read(File.ReadAllText(LunaUnitBalance.CsvAssetPath));
        var sources = Sources();
        var existing = new HashSet<string>(StringComparer.Ordinal);
        int nextId = rows.Max(row => row.Id) + 1;
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!sources.TryGetValue(row.Key, out string path))
                throw new InvalidDataException("CSV source asset was deleted; resolve explicitly before syncing: " + row.Key + " / " + row.ResourcePath);
            records[i + 1][2] = ResourcePath(path); // Only the location changes. Existing numeric values remain intact.
            existing.Add(row.Key);
        }
        foreach (var source in sources)
            if (!existing.Contains(source.Key))
                records.Add(LunaUnitBalanceRow.Capture(nextId++, source.Key, ResourcePath(source.Value), AssetDatabase.LoadAssetAtPath<ObjectData>(source.Value)));
        Write(records);
        ValidateTable();
        Debug.Log("Synchronized unit source mappings without overwriting existing CSV balance.");
    }

    [MenuItem("Luna/Data/Validate Unit Balance CSV")]
    public static void ValidateTable()
    {
        var rows = LunaUnitBalanceRow.ParseDocument(File.ReadAllText(LunaUnitBalance.CsvAssetPath));
        var sources = Sources();
        if (rows.Count != sources.Count) throw new InvalidDataException("Unit CSV/source coverage differs: " + rows.Count + " rows / " + sources.Count + " ObjectData assets.");
        foreach (var row in rows)
        {
            if (!sources.TryGetValue(row.Key, out string path)) throw new InvalidDataException("Unknown CSV source GUID: " + row.Key);
            if (row.ResourcePath != ResourcePath(path)) throw new InvalidDataException("CSV resource path moved; run Sync New Unit Sources and Paths: " + row.ResourcePath);
            if (Resources.Load<ObjectData>(row.ResourcePath) != AssetDatabase.LoadAssetAtPath<ObjectData>(path))
                throw new InvalidDataException("Ambiguous or missing Resources mapping: " + row.ResourcePath);
        }
        Debug.Log("Validated complete unit CSV coverage: " + rows.Count + " stable source GUIDs.");
    }

    private static SortedDictionary<string, string> Sources()
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string guid in AssetDatabase.FindAssets("t:ObjectData", new[] { "Assets/Resources" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.LoadAssetAtPath<ObjectData>(path) != null) result.Add(guid, path);
        }
        if (result.Count == 0) throw new InvalidDataException("No source ObjectData assets found.");
        return result;
    }

    private static string ResourcePath(string path)
    {
        const string prefix = "Assets/Resources/";
        if (!path.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidDataException("ObjectData source is outside Resources: " + path);
        return path.Substring(prefix.Length, path.Length - prefix.Length - Path.GetExtension(path).Length);
    }

    private static void Write(List<string[]> records)
    {
        string text = string.Join("\n", records.Select(LunaCsv.WriteRow)) + "\n";
        LunaUnitBalanceRow.ParseDocument(text); // Verify complete CSV before touching its authoritative copy.
        string path = Path.GetFullPath(LunaUnitBalance.CsvAssetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, text, new UTF8Encoding(false));
        if (File.Exists(path))
        {
            string backup = Path.GetFullPath("Logs/Revival/BalanceBackups/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N") + ".csv");
            Directory.CreateDirectory(Path.GetDirectoryName(backup));
            File.Replace(temporary, path, backup);
        }
        else File.Move(temporary, path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void EnsureEditMode()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Export unit data only outside Play Mode.");
    }
}

[CustomEditor(typeof(ObjectData))]
public sealed class LunaObjectDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool csvExists = File.Exists(LunaUnitBalance.CsvAssetPath);
        if (csvExists)
        {
            EditorGUILayout.HelpBox("게임 밸런스 수치와 분류는 UnitBalance.csv가 원본입니다. 이 에셋은 이미지·프리팹·설명 참조를 유지합니다. 저장된 진행도는 실행 시 별도로 적용됩니다.", MessageType.Info);
            if (GUILayout.Button("Open UnitBalance.csv")) AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(LunaUnitBalance.CsvAssetPath));
        }
        serializedObject.Update();
        var property = serializedObject.GetIterator();
        bool enter = true;
        while (property.NextVisible(enter))
        {
            enter = false;
            bool locked = property.name == "m_Script" || (csvExists && Array.IndexOf(LunaUnitBalanceRow.BalanceFields, property.name) >= 0);
            using (new EditorGUI.DisabledScope(locked)) EditorGUILayout.PropertyField(property, true);
        }
        serializedObject.ApplyModifiedProperties();
    }
}

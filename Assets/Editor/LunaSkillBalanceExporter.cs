using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class LunaSkillBalanceExporter
{
    [MenuItem("Luna/Data/Export Initial Skill Balance CSV")]
    public static void ExportInitialTable()
    {
        EnsureEditMode();
        if (File.Exists(LunaSkillBalance.CsvAssetPath))
            throw new InvalidOperationException("SkillBalance.csv is already the balance authority. Initial export refuses to overwrite it. Use Sync New Skill Sources to add assets without replacing edited balance.");
        var sources = Sources();
        var records = new List<string[]> { LunaSkillBalanceRow.Header };
        int id = 1;
        foreach (var source in sources)
            records.Add(LunaSkillBalanceRow.Capture(id++, source.Key, ResourcePath(source.Value), AssetDatabase.LoadAssetAtPath<SkillData>(source.Value)));
        Write(records);
        ValidateTable();
        Debug.Log("Exported " + sources.Count + " SkillData assets and " + LunaSkillBalanceRow.BalanceFields.Length + " balance fields to the Game Framework skill CSV.");
    }

    [MenuItem("Luna/Data/Sync New Skill Sources and Paths")]
    public static void SyncNewSkillsAndPaths()
    {
        EnsureEditMode();
        var rows = LunaSkillBalanceRow.ParseDocument(File.ReadAllText(LunaSkillBalance.CsvAssetPath));
        var records = LunaCsv.Read(File.ReadAllText(LunaSkillBalance.CsvAssetPath));
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
                records.Add(LunaSkillBalanceRow.Capture(nextId++, source.Key, ResourcePath(source.Value), AssetDatabase.LoadAssetAtPath<SkillData>(source.Value)));
        Write(records);
        ValidateTable();
        Debug.Log("Synchronized skill source mappings without overwriting existing CSV balance.");
    }

    [MenuItem("Luna/Data/Validate Skill Balance CSV")]
    public static void ValidateTable()
    {
        var rows = LunaSkillBalanceRow.ParseDocument(File.ReadAllText(LunaSkillBalance.CsvAssetPath));
        var sources = Sources();
        if (rows.Count != sources.Count) throw new InvalidDataException("Skill CSV/source coverage differs: " + rows.Count + " rows / " + sources.Count + " SkillData assets.");
        foreach (var row in rows)
        {
            if (!sources.TryGetValue(row.Key, out string path)) throw new InvalidDataException("Unknown CSV source GUID: " + row.Key);
            if (row.ResourcePath != ResourcePath(path)) throw new InvalidDataException("CSV resource path moved; run Sync New Skill Sources and Paths: " + row.ResourcePath);
            if (Resources.Load<SkillData>(row.ResourcePath) != AssetDatabase.LoadAssetAtPath<SkillData>(path))
                throw new InvalidDataException("Ambiguous or missing Resources mapping: " + row.ResourcePath);
        }
        Debug.Log("Validated complete skill CSV coverage: " + rows.Count + " stable source GUIDs.");
    }

    private static SortedDictionary<string, string> Sources()
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string guid in AssetDatabase.FindAssets("t:SkillData", new[] { "Assets/Resources/ScriptableObjects/LunaSkills" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.LoadAssetAtPath<SkillData>(path) != null) result.Add(guid, path);
        }
        if (result.Count == 0) throw new InvalidDataException("No source SkillData assets found.");
        return result;
    }

    private static string ResourcePath(string path)
    {
        const string prefix = "Assets/Resources/";
        if (!path.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidDataException("SkillData source is outside Resources: " + path);
        return path.Substring(prefix.Length, path.Length - prefix.Length - Path.GetExtension(path).Length);
    }

    private static void Write(List<string[]> records)
    {
        string text = string.Join("\n", records.Select(LunaCsv.WriteRow)) + "\n";
        LunaSkillBalanceRow.ParseDocument(text); // Verify complete CSV before touching its authoritative copy.
        string path = Path.GetFullPath(LunaSkillBalance.CsvAssetPath);
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
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Export skill data only outside Play Mode.");
    }
}

[CustomEditor(typeof(SkillData))]
public sealed class LunaSkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool csvExists = File.Exists(LunaSkillBalance.CsvAssetPath);
        if (csvExists)
        {
            EditorGUILayout.HelpBox("Skill and equipment combat settings are authored in SkillBalance.csv. This asset retains art/text references and the initial skill level; saved levels are applied separately at runtime.", MessageType.Info);
            if (GUILayout.Button("Open SkillBalance.csv")) AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(LunaSkillBalance.CsvAssetPath));
        }
        serializedObject.Update();
        var property = serializedObject.GetIterator();
        bool enter = true;
        while (property.NextVisible(enter))
        {
            enter = false;
            bool locked = property.name == "m_Script" || (csvExists && Array.IndexOf(LunaSkillBalanceRow.BalanceFields, property.name) >= 0);
            using (new EditorGUI.DisabledScope(locked)) EditorGUILayout.PropertyField(property, true);
        }
        serializedObject.ApplyModifiedProperties();
    }
}

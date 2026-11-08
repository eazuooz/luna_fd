using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luna.Gameplay;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework.DataTable;

/// <summary>Native asset contracts: data-only prefab views, typed EntityLogic catalog, direct native UI and Scene addresses.</summary>
public static class LunaAssetRepair
{
    public static void RepairAndAudit() { LunaMigrationSetup.Prepare(); LunaValidation.Audit(); }

    [MenuItem("Luna/Validation/Validate Native Asset Contracts")]
    public static void ValidateNativeAssets()
    {
        var checks = new List<string>(); Validate(checks); Debug.Log(string.Join("\n", checks));
    }
    public static void Validate(List<string> checks)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) throw new InvalidDataException("Native Addressables settings are missing.");
        var entries = new List<AddressableAssetEntry>();
        foreach (var group in settings.groups) if (group != null) entries.AddRange(group.entries);
        int forms = 0;
        foreach (var form in LunaUI.Definitions)
        {
            var entry = Resolve(entries, form.AssetName);
            if (!LunaMigrationSetup.FormGuids.TryGetValue(form.Type, out string guid) || entry.guid != guid)
                throw new InvalidDataException("UI address resolves to a different authored prefab: " + form.AssetName);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null || prefab.GetComponent<UILuna>() == null) throw new InvalidDataException("UI form is not native UIFormLogic: " + form.AssetName);
            RequireNoMissingScripts(prefab); forms++;
        }
        if (forms != 26) throw new InvalidDataException("Expected the 26 authored UI registrations.");
        checks.Add("26 native UI load keys resolve to the original 24 GUID-preserved prefabs (account/PVP popup aliases included).");
        foreach (string scene in LunaMigrationSetup.NativeScenes)
            if (AssetDatabase.GUIDToAssetPath(Resolve(entries, scene).guid) != scene) throw new InvalidDataException("Native scene address mismatch: " + scene);
        var enabled = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (enabled.Length != 1 || enabled[0] != LunaMigrationSetup.LoadingScene) throw new InvalidDataException("Loading must be the only build-scene entry; game scenes load through native Scene/Addressables.");
        checks.Add("Loading is the sole build entry; Lobby/Game/Dungeon use native scene addresses.");
        var catalog = AssetDatabase.LoadAssetAtPath<LunaEntityCatalog>("Assets/Resources/Luna/EntityCatalog.asset");
        if (catalog == null || catalog.Entries.Length == 0) throw new InvalidDataException("Native entity catalog is empty.");
        var locations = LunaResourcesProvider.CreateLocations(catalog);
        var unique = new HashSet<GameObject>();
        foreach (var row in catalog.Entries)
        {
            if (row.Prefab == null || !unique.Add(row.Prefab)) throw new InvalidDataException("Entity catalog contains a missing or duplicate prefab.");
            Type type = Type.GetType(row.LogicType + ", Assembly-CSharp") ?? Type.GetType(row.LogicType);
            if (type == null || type.IsAbstract || !typeof(EntityLogicBase).IsAssignableFrom(type)) throw new InvalidDataException("Invalid native EntityLogic type: " + row.LogicType);
            if (row.Address != AssetDatabase.GetAssetPath(row.Prefab) || !locations.Locate(row.Address, typeof(GameObject), out var resolved)
                || resolved.Count != 1 || resolved[0].InternalId != LunaResourcesProvider.ResourcePath(row.Address))
                throw new InvalidDataException("Native Resources location mismatch: " + row.Address);
            if (settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(row.Address)) != null)
                throw new InvalidDataException("Resources prefab is also marked for an Addressables bundle: " + row.Address);
            RequireNoMissingScripts(row.Prefab);
            if (row.Prefab.GetComponentsInChildren<EntityLogic>(true).Length != 0) throw new InvalidDataException("EntityLogic must be added by the native Entity module, not baked into " + row.Prefab.name);
            if (typeof(UnitLogic).IsAssignableFrom(type))
            {
                var view = row.Prefab.GetComponent<UnitView>();
                if (view == null || view.Definition == null) throw new InvalidDataException("Unit asset has no authored view/definition: " + row.Prefab.name);
            }
        }
        checks.Add(catalog.Entries.Length + " entity catalog bindings use concrete native EntityLogic classes and data-only unit views.");
        var bootstrap = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LunaBootstrap.prefab")?.GetComponent<LunaBootstrap>();
        if (bootstrap == null || bootstrap.playerPrefabs == null || bootstrap.playerPrefabs.Length != 7 ||
            bootstrap.playerPrefabs.Any(prefab => prefab == null) || bootstrap.playerPrefabs.Distinct().Count() != 7)
            throw new InvalidDataException("Bootstrap must contain seven distinct playable skin prefabs.");
        foreach (var prefab in bootstrap.playerPrefabs)
        {
            var entry = catalog.Entries.SingleOrDefault(value => value.Prefab == prefab);
            var type = entry != null ? Type.GetType(entry.LogicType + ", Assembly-CSharp") : null;
            if (type == null || !typeof(PlayerLogic).IsAssignableFrom(type))
                throw new InvalidDataException("Playable skin is not bound to native PlayerLogic: " + prefab.name);
        }
        checks.Add("All seven authored player skins bind to native PlayerLogic and its player FSM.");
        // Edit-mode audit must not construct runtime modules whose helpers are installed in Start.
        // Parse the exact production IDataRow classes and validate all links without global state.
        var stages = ReadRows<LunaStageRow>("Stages", LunaStageRow.Header);
        var waves = ReadRows<LunaStageWaveRow>("StageWaves", LunaStageWaveRow.Header);
        var rewards = ReadRows<LunaStageRewardRow>("StageRewards", LunaStageRewardRow.Header);
        var stageKeys = new Dictionary<string, LunaStageRow>(StringComparer.Ordinal);
        var stageIds = new HashSet<int>(stages.Select(row => row.Id));
        foreach (var row in stages)
        {
            if (stageKeys.ContainsKey(row.Key)) throw new InvalidDataException("Duplicate stage key " + row.Key);
            stageKeys.Add(row.Key, row);
            LunaStageTables.RequireAsset<GameObject>(row.Background);
            if (row.Kind == LunaStageKind.Basic)
            {
                LunaStageTables.RequireAsset<GameObject>(row.AllyTower);
                LunaStageTables.RequireAsset<GameObject>(row.EnemyTower);
                if (!waves.Any(w => w.StageId == row.Id && w.Phase == "Repeat")) throw new InvalidDataException("Campaign stage lacks a repeating wave: " + row.Key);
            }
            if (row.Kind == LunaStageKind.Boss) LunaStageTables.RequireAsset<GameObject>(row.Boss);
        }
        foreach (var row in waves)
        {
            if (!stageIds.Contains(row.StageId)) throw new InvalidDataException("Wave references a missing stage: " + row.Id);
            LunaStageTables.RequireAsset<GameObject>(row.Prefab);
        }
        foreach (var row in rewards)
        {
            if (!stageIds.Contains(row.StageId)) throw new InvalidDataException("Reward references a missing stage: " + row.Id);
            LunaStageTables.RequireAsset<ItemData>(row.Item);
        }
        var definitionKeys = new HashSet<string>(StringComparer.Ordinal);
        int stageCount = 0;
        foreach (string path in new[] { "Assets/Resources/Prefabs/10Stage", "Assets/Resources/Prefabs/11Dungeons" })
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { path }))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab == null) continue;
                var definition = prefab.GetComponent<LunaStageDefinition>();
                if (definition == null) throw new InvalidDataException("Stage still lacks its data-only definition: " + prefab.name);
                string key = string.IsNullOrEmpty(definition.TableKey) ? prefab.name : definition.TableKey;
                if (!stageKeys.ContainsKey(key) || !definitionKeys.Add(key)) throw new InvalidDataException("Stage has a missing or duplicate native table binding: " + prefab.name);
                RequireNoMissingScripts(prefab); stageCount++;
            }
        if (stageCount != stages.Count) throw new InvalidDataException("Stage definitions and CSV table rows differ.");
        checks.Add(stageCount + " stage definitions, " + waves.Count + " wave rows and " + rewards.Count + " reward rows passed production IDataRow parsers, uniqueness and asset/reference checks without starting runtime modules.");
        var unitCsv = Resources.Load<TextAsset>(LunaUnitBalance.ResourceName);
        if (unitCsv == null) throw new InvalidDataException("Unit balance CSV is missing.");
        var unitRows = LunaUnitBalanceRow.ParseDocument(unitCsv.text);
        var unitSources = new HashSet<ObjectData>();
        foreach (var row in unitRows)
        {
            var source = Resources.Load<ObjectData>(row.ResourcePath);
            if (source == null || !unitSources.Add(source) || AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)) != row.Key)
                throw new InvalidDataException("Unit balance row has a missing, duplicate or changed source GUID: " + row.ResourcePath);
        }
        var parameters = ReadRows<LunaEntityParameterRow>("EntityParameters", new[] { "RowId", "Key", "Parameter", "Value" });
        var parameterKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in parameters)
            if (!parameterKeys.Add(row.Key + "/" + row.Parameter)) throw new InvalidDataException("Duplicate entity parameter: " + row.Key + "/" + row.Parameter);
        checks.Add(unitRows.Count + " unit balance rows and " + parameters.Count + " entity parameter rows passed production parsers and identity/key validation.");
        LunaSkillBalanceExporter.ValidateTable();
        var skillRows = LunaSkillBalanceRow.ParseDocument(File.ReadAllText(LunaSkillBalance.CsvAssetPath));
        checks.Add(skillRows.Count + " active-skill/equipment balance rows passed the production parser and complete source GUID/Resources coverage; art references and saved levels remain separate.");
        var framework = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Luna/GameFramework.prefab");
        if (framework == null) throw new InvalidDataException("Supplied GameFramework prefab is missing.");
        RequireNoMissingScripts(framework);
        var procedure = framework.GetComponentInChildren<ProcedureComponent>(true);
        var serialized = new SerializedObject(procedure);
        if (!procedure.enabled || serialized.FindProperty("mEntranceProcedureTypeName").stringValue != typeof(LunaLaunchProcedure).FullName)
            throw new InvalidDataException("Native ProcedureComponent must launch LunaLaunchProcedure.");
        checks.Add("Supplied native ProcedureComponent is enabled with the Luna entrance procedure.");
        LunaLinkerValidation.Validate(checks, catalog, framework);
    }
    private static List<T> ReadRows<T>(string name, string[] header) where T : IDataRow, new()
    {
        var source = Resources.Load<TextAsset>("DataTables/" + name);
        if (source == null) throw new InvalidDataException("Missing CSV: " + name);
        var records = LunaCsv.Read(source.text);
        if (records.Count == 0 || !records[0].SequenceEqual(header)) throw new InvalidDataException("Invalid CSV header: " + name);
        var rows = new List<T>(); var ids = new HashSet<int>();
        for (int i = 1; i < records.Count; i++)
        {
            var row = new T();
            if (!row.ParseDataRow(LunaCsv.WriteRow(records[i]), null) || !ids.Add(row.Id)) throw new InvalidDataException("Rejected or duplicate " + name + " row " + i);
            rows.Add(row);
        }
        return rows;
    }
    private static AddressableAssetEntry Resolve(List<AddressableAssetEntry> entries, string address)
    {
        var matches = entries.Where(e => e.address == address || e.labels.Contains(address)).Distinct().ToArray();
        if (matches.Length != 1) throw new InvalidDataException("Expected exactly one asset for native load key " + address + "; found " + matches.Length);
        return matches[0];
    }
    private static void RequireNoMissingScripts(GameObject prefab)
    {
        foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
                throw new InvalidDataException("Missing script in " + AssetDatabase.GetAssetPath(prefab) + " / " + transform.name);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Luna.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>Checks dynamic linker roots without creating native modules or changing build settings.</summary>
public static class LunaLinkerValidation
{
    public static void Validate(List<string> checks, LunaEntityCatalog catalog, GameObject framework)
    {
        var assemblies = new[] { typeof(LunaEntityCatalog).Assembly, typeof(EntityComponent).Assembly,
            typeof(GameFramework.GameFrameworkEntry).Assembly }.ToDictionary(value => value.GetName().Name);
        var document = XDocument.Load("Assets/link.xml");
        if (document.Root?.Name != "linker") throw new InvalidDataException("Invalid Luna link.xml root.");
        var roots = new Dictionary<Type, XElement>();
        var assemblyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assemblyNode in document.Root.Elements())
        {
            string name = (string)assemblyNode.Attribute("fullname");
            if (assemblyNode.Name != "assembly" || name == null || !assemblyNames.Add(name) ||
                !assemblies.TryGetValue(name, out var assembly) || assemblyNode.Attribute("preserve") != null)
                throw new InvalidDataException("Unexpected or broad assembly preservation in Luna link.xml: " + name);
            foreach (var typeNode in assemblyNode.Elements())
            {
                string fullName = (string)typeNode.Attribute("fullname");
                var type = fullName == null ? null : assembly.GetType(fullName.Replace('/', '+'), false);
                string preservation = (string)typeNode.Attribute("preserve");
                if (typeNode.Name != "type" || type == null || roots.ContainsKey(type) ||
                    (preservation != "all" && preservation != "nothing"))
                    throw new InvalidDataException("Unresolved, duplicate or unsupported linker type: " + fullName);
                roots.Add(type, typeNode);
                var fieldNames = new HashSet<string>(StringComparer.Ordinal);
                foreach (var fieldNode in typeNode.Elements())
                {
                    string field = (string)fieldNode.Attribute("name");
                    if (fieldNode.Name != "field" || field == null || !fieldNames.Add(field) ||
                        type.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) == null)
                        throw new InvalidDataException("Unresolved or duplicate linker field: " + fullName + "." + field);
                }
            }
        }

        void RequireType(Type type)
        {
            if (type == null || !roots.TryGetValue(type, out var node) || (string)node.Attribute("preserve") != "all")
                throw new InvalidDataException("Dynamic type is missing full linker preservation: " + type?.FullName);
        }
        void RequireFields(Type type, IEnumerable<string> expected)
        {
            if (!roots.TryGetValue(type, out var node)) throw new InvalidDataException("CSV field owner is missing linker preservation: " + type.FullName);
            var fields = new HashSet<string>(node.Elements("field").Select(field => (string)field.Attribute("name")), StringComparer.Ordinal);
            foreach (string field in expected)
                if (!fields.Contains(field)) throw new InvalidDataException("CSV-reflected field is missing linker preservation: " + type.FullName + "." + field);
        }

        var game = typeof(LunaEntityCatalog).Assembly;
        foreach (var row in catalog.Entries) RequireType(game.GetType(row.LogicType, false));
        int procedures = 0, helpers = 0, modules = 0;
        var native = typeof(EntityComponent).Assembly;
        var core = typeof(GameFramework.GameFrameworkEntry).Assembly;
        foreach (var component in framework.GetComponentsInChildren<GameFrameworkComponent>(true))
        {
            using (var serialized = new SerializedObject(component))
            {
                var property = serialized.GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.String && property.name.EndsWith("HelperTypeName", StringComparison.Ordinal) &&
                        !string.IsNullOrEmpty(property.stringValue))
                    { RequireType(native.GetType(property.stringValue, false)); helpers++; }
                if (component is ProcedureComponent)
                {
                    var names = serialized.FindProperty("mAvailableProcedureTypeNames");
                    for (int i = 0; i < names.arraySize; i++) { RequireType(game.GetType(names.GetArrayElementAtIndex(i).stringValue, false)); procedures++; }
                }
            }
            // These three supplied components do not request a Core I*Manager module.
            if (component is BaseComponent || component is ResourceComponent || component is ReferencePoolComponent) continue;
            string prefix = component.GetType().Name.Replace("Component", "");
            RequireType(core.GetType("GameFramework." + prefix + "." + prefix + "Manager", false));
            modules++;
        }
        RequireType(core.GetType("GameFramework.ObjectPool.ObjectPoolManager+ObjectPool`1", false));
        RequireType(core.GetType("GameFramework.DataTable.DataTableManager+DataTable`1", false));
        RequireFields(typeof(ObjectData), LunaUnitBalanceRow.BalanceFields);
        RequireFields(typeof(SkillData), LunaSkillBalanceRow.BalanceFields);
        checks.Add("Linker roots resolve and cover " + catalog.Entries.Select(row => row.LogicType).Distinct().Count() +
            " catalog types, " + procedures + " procedures, " + helpers + " configured helpers, " + modules +
            " Core modules, both reflected generic implementations and all CSV-reflected fields; this is an authoring check, not an IL2CPP runtime test.");
    }
}

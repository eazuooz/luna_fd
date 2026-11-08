using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityGameFramework.Runtime;

/// <summary>Actual Game Framework row; ObjectData is only the legacy gameplay view.</summary>
public sealed class LunaUnitBalanceRow : DataRowBase
{
    // This explicit schema deliberately excludes Sprite/Prefab references and localized descriptions.
    public static readonly string[] BalanceFields =
    {
        "ID", "Level", "moveSpeedLevel", "cardCount", "ultState", "cardLevel", "hitPriority",
        "UnitSide", "UnitType", "UnitAttackType", "TypeLunaPlayer", "TypeSuperArmour", "Price",
        "Hp", "Mp", "Stamina", "MoveSpeed", "Armour", "AttackDamage", "AttackOnceBySecondValue",
        "SkillResistance", "SkillDamage", "AttackRange", "SpawnDelayTime", "CriticalMultiples",
        "CriticalProbability", "LevelUpHpPoint", "LevelUpAttPoint", "LevelUpArmourPoint",
        "LevelUpSkillDamage", "LevelUpMoveSpeed", "EffectScale"
    };
    public static readonly string[] Header = BuildHeader();
    private static readonly FieldInfo[] Fields = BuildFields();
    private object[] values;
    private int id;
    public override int Id => id;
    public string Key { get; private set; }
    public string ResourcePath { get; private set; }

    public override bool ParseDataRow(string dataRowString, object userData)
    {
        var rows = LunaCsv.Read(dataRowString);
        if (rows.Count != 1) throw new InvalidDataException("Expected one unit balance CSV record.");
        ParseCells(rows[0]);
        return true;
    }

    public void ParseCells(string[] cells)
    {
        if (cells.Length != Header.Length) throw new InvalidDataException("Unit balance row has " + cells.Length + " columns; expected " + Header.Length + ".");
        id = int.Parse(cells[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (id <= 0) throw new InvalidDataException("Unit balance RowId must be positive.");
        Key = cells[1];
        if (Key.Length != 32 || !Guid.TryParseExact(Key, "N", out _)) throw new InvalidDataException("Unit balance Key must be the source asset GUID.");
        ResourcePath = cells[2];
        if (string.IsNullOrWhiteSpace(ResourcePath) || ResourcePath.Contains("\\") || ResourcePath.StartsWith("/") || ResourcePath.Contains(".."))
            throw new InvalidDataException("Invalid unit resource path: " + ResourcePath);
        values = new object[Fields.Length];
        for (int i = 0; i < Fields.Length; i++)
        {
            try { values[i] = ParseValue(Fields[i].FieldType, cells[i + 3]); }
            catch (Exception exception) { throw new InvalidDataException("Unit " + Key + " has invalid " + Fields[i].Name + ": " + cells[i + 3], exception); }
        }
    }

    public void ApplyTo(ObjectData target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        for (int i = 0; i < Fields.Length; i++) Fields[i].SetValue(target, values[i]);
    }

    public object GetValue(string field)
    {
        int index = Array.IndexOf(BalanceFields, field);
        if (index < 0) throw new ArgumentException("Unknown balance field: " + field);
        return values[index];
    }

    public static string[] Capture(int rowId, string key, string resourcePath, ObjectData source)
    {
        var cells = new string[Header.Length];
        cells[0] = rowId.ToString(CultureInfo.InvariantCulture); cells[1] = key; cells[2] = resourcePath;
        for (int i = 0; i < Fields.Length; i++)
        {
            object value = Fields[i].GetValue(source);
            if (value is float number) cells[i + 3] = number.ToString("R", CultureInfo.InvariantCulture);
            else if (value is bool boolean) cells[i + 3] = boolean ? "true" : "false";
            else if (Fields[i].FieldType.IsEnum) cells[i + 3] = Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            else cells[i + 3] = Convert.ToString(value, CultureInfo.InvariantCulture);
        }
        return cells;
    }

    public static List<LunaUnitBalanceRow> ParseDocument(string text)
    {
        var records = LunaCsv.Read(text);
        if (records.Count < 2) throw new InvalidDataException("Unit balance CSV requires a header and at least one row.");
        if (records[0].Length != Header.Length) throw new InvalidDataException("Unit balance CSV schema width differs.");
        for (int i = 0; i < Header.Length; i++)
            if (records[0][i] != Header[i]) throw new InvalidDataException("Unit balance column " + i + " must be " + Header[i] + ".");
        var rows = new List<LunaUnitBalanceRow>();
        var ids = new HashSet<int>(); var keys = new HashSet<string>(StringComparer.Ordinal); var paths = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 1; i < records.Count; i++)
        {
            var row = new LunaUnitBalanceRow(); row.ParseCells(records[i]);
            if (!ids.Add(row.Id) || !keys.Add(row.Key) || !paths.Add(row.ResourcePath))
                throw new InvalidDataException("Duplicate unit RowId, Key or ResourcePath at CSV row " + (i + 1) + ".");
            rows.Add(row);
        }
        return rows;
    }

    private static object ParseValue(Type type, string text)
    {
        if (type == typeof(float))
        {
            float value = float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
            if (float.IsNaN(value) || float.IsInfinity(value)) throw new FormatException("Non-finite balance value.");
            return value;
        }
        if (type == typeof(int)) return int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (type == typeof(bool)) return bool.Parse(text);
        if (type == typeof(char))
        {
            if (text.Length != 1) throw new FormatException("Rarity must be a single character.");
            return text[0];
        }
        if (type.IsEnum)
        {
            int value = int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (!Enum.IsDefined(type, value)) throw new FormatException("Undefined enum value.");
            return Enum.ToObject(type, value);
        }
        throw new InvalidOperationException("Unsupported balance schema type " + type);
    }

    private static string[] BuildHeader()
    {
        var header = new string[BalanceFields.Length + 3];
        header[0] = "RowId"; header[1] = "Key"; header[2] = "ResourcePath";
        Array.Copy(BalanceFields, 0, header, 3, BalanceFields.Length);
        return header;
    }

    private static FieldInfo[] BuildFields()
    {
        var fields = new FieldInfo[BalanceFields.Length];
        for (int i = 0; i < fields.Length; i++)
            fields[i] = typeof(ObjectData).GetField(BalanceFields[i], BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException("ObjectData balance field removed: " + BalanceFields[i]);
        return fields;
    }
}

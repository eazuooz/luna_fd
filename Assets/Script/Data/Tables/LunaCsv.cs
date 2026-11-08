using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>Small RFC 4180 reader: escaped quotes, commas, CRLF and quoted newlines.</summary>
public static class LunaCsv
{
    public static List<string[]> Read(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        var rows = new List<string[]>();
        var cells = new List<string>();
        var cell = new StringBuilder();
        bool quoted = false, closedQuote = false, rowStarted = false;
        int start = text.Length > 0 && text[0] == '\uFEFF' ? 1 : 0;
        for (int i = start; i < text.Length; i++)
        {
            char character = text[i];
            if (quoted)
            {
                if (character == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else { quoted = false; closedQuote = true; }
                }
                else cell.Append(character);
                continue;
            }
            if (character == ',' || character == '\r' || character == '\n')
            {
                cells.Add(cell.ToString()); cell.Clear(); closedQuote = false;
                if (character == ',') { rowStarted = true; continue; }
                if (character == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                if (rowStarted || cells.Count > 1 || cells[0].Length > 0) rows.Add(cells.ToArray());
                cells.Clear(); rowStarted = false;
                continue;
            }
            if (closedQuote) throw new InvalidDataException("Unexpected text after a closing CSV quote at character " + i + ".");
            if (character == '"')
            {
                if (cell.Length != 0) throw new InvalidDataException("Quote inside an unquoted CSV field at character " + i + ".");
                quoted = true;
            }
            else cell.Append(character);
            rowStarted = true;
        }
        if (quoted) throw new InvalidDataException("Unterminated quoted CSV field.");
        if (rowStarted || cells.Count > 0 || cell.Length > 0 || closedQuote)
        {
            cells.Add(cell.ToString()); rows.Add(cells.ToArray());
        }
        return rows;
    }

    public static string WriteRow(IEnumerable<string> cells)
    {
        var result = new StringBuilder();
        bool first = true;
        foreach (string cellValue in cells)
        {
            if (!first) result.Append(',');
            first = false;
            string cell = cellValue ?? string.Empty;
            if (cell.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
                result.Append('"').Append(cell.Replace("\"", "\"\"")).Append('"');
            else result.Append(cell);
        }
        return result.ToString();
    }
}

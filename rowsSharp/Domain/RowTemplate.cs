using RowsSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RowsSharp.Domain;

internal static class RowTemplate
{
    internal static IEnumerable<string?[]> Generate(int count, IList<string> headers, IEnumerable<ColumnStyle> columnStyles)
    {
        for (int i = 0; i < count; i++)
        {
            var row = new string?[headers.Count];

            row = ApplyTemplate(row, i, count, headers, columnStyles);

            yield return row;
        }
    }

    private static string?[] ApplyTemplate(string?[] row, int rowIndex, int count, IList<string> headers, IEnumerable<ColumnStyle> columnStyles)
    {
        foreach (var columnStyle in columnStyles)
        {
            int index = headers.IndexOf(columnStyle.Column);
            if (index == -1) { continue; }

            row[index] = Expand(columnStyle.Template, rowIndex, count);
        }
        return row;
    }

    private static string ExpandOriginalNotation(string value)
    {
        // These will be deprecated soon.
        return value.Replace("<D>", "<yyyyMMdd>")
                    .Replace("<d>", "<yyyy-MM-dd>")
                    .Replace("<T>", "<HHmmss>")
                    .Replace("<t>", "<HH:mm:ss>");
    }

    private readonly static Regex column = new("(?<=<)(.+?)(?=>)");

    private static string ExpandDateTime(string value)
    {
        var matches = column.Matches(value).OfType<Match>();
        var now = DateTime.Now;

        foreach (var match in matches)
        {
            try
            {
                value = value.Replace("<" + match + ">", now.ToString(match.Value));
            }
            catch (FormatException) { }
        }

        return value;
    }

    private static string ExpandIndexer(string value, int rowIndex, int count)
    {
        return value.Replace("<#>", rowIndex.ToString())
                    .Replace("<!#>", (count - rowIndex - 1).ToString());
    }

    private static string? Expand(string? value, int rowIndex, int count)
    {
        if (value is null) { return null; }

        value = ExpandIndexer(value, rowIndex, count);
        value = ExpandOriginalNotation(value);
        value = ExpandDateTime(value);

        return value;        
    }
}

using rowsSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace rowsSharp.Domain;

internal static class RowTemplate
{
    internal static IEnumerable<string?[]> Generate(int count, IList<string> headers, IDictionary<string, ColumnStyle>? template)
    {
        for (int i = 0; i < count; i++)
        {
            var row = new string?[headers.Count];

            if (template is not null)
            {
                row = ApplyTemplate(row, i, count, headers, template);
            }

            yield return row;
        }
    }

    private static string?[] ApplyTemplate(string?[] row, int rowIndex, int count, IList<string> headers, IDictionary<string, ColumnStyle> template)
    {
        foreach (var (column, style) in template)
        {
            int index = headers.IndexOf(column);
            if (index == -1) { continue; }

            row[index] = Expand(style.Template, rowIndex, count);
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

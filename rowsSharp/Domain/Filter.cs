using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RowsSharp.Domain;

internal class Filter
{
    private static readonly Regex splitBySpace = new(@"\s+(?=(?:\""[^\""]*\""|[^\""])*$)");
    private static readonly Regex splitByColon = new(@"(?<!\\):");

    private readonly List<KeyValuePair<int, string>> filter = new();

    internal IList<string> Headers { get; set; } = new List<string>();
    internal string FilterText { get; set; } = "";

    internal bool UseRegex { get; set; }

    internal Predicate<object> Invoke()
    {
        try
        {
            SplitInput();
            if (UseRegex)
            {
                ValidateRegex();
            }
            return Predicate;
        }
        catch { }

        return (object obj) => false;
    }

    private void SplitInput()
    {
        filter.Clear();
        if (string.IsNullOrEmpty(FilterText)) { return; }

        var filters = splitBySpace.Split(FilterText);

        foreach (string criterion in filters)
        {
            string[] keyvalue = splitByColon.Split(criterion.Replace("\"", ""));

            // Value only
            if (keyvalue.Length == 1)
            {
                ParseValue(keyvalue[0]);
                continue;
            }

            // Key:Value
            ParseKeyValue(keyvalue);
        }
    }

    private void ParseValue(string criterion)
    {
        filter.Add(new(-1, criterion));
    }

    private void ParseKeyValue(string[] criterion)
    {
        int column = Headers.IndexOf(criterion[0]);

        if (column == -1)
        {
            throw new IndexOutOfRangeException($"Invalid column {criterion[0]}");
        }

        filter.Add(new(column, criterion[1]));
    }

    private void ValidateRegex()
    {
        foreach ((var _, string criterion) in filter)
        {
            // Throws exception on invalid regex
            Regex.IsMatch("", criterion);
        }
    }

    private static string ToCsvString(IList<string?> row)
    {
        return '"' + string.Join("\",\"", row) + '"';
    }

    private bool Predicate(object obj)
    {
        var row = (IList<string?>)obj;
        foreach ((int column, string pattern) in filter)
        {
            string input = (column == -1 ? ToCsvString(row) : row[column]) ?? "";

            if (
                (UseRegex && Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                || (!UseRegex && input.Contains(pattern, StringComparison.InvariantCultureIgnoreCase))
            )
            { continue; }
            return false;
        }
        return true;
    }
}

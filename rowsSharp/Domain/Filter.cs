using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace rowsSharp.Domain;

internal class Filter
{
    private static readonly Regex splitBySpace = new(@"\s+(?=(?:\""[^\""]*\""|[^\""])*$)");
    private static readonly Regex splitByColon = new(@"(?<!\\):");

    private readonly List<KeyValuePair<int, string>> filter = new();

    internal IList<string> Headers { get; set; } = new List<string>();
    internal string FilterText { get; set; } = "";
    internal ICollectionView CollectionView { get; private set; }

    internal bool UseRegex { get; set; }

    internal Filter(ICollectionView collectionView)
    {
        CollectionView = collectionView;
    }

    internal ICollectionView Invoke()
    {
        try
        {
            SplitInput();
            if (UseRegex)
            {
                ValidateRegex();
            }
        }
        catch
        {
            CollectionView.Filter = (object obj) => false;
            return CollectionView;
        }

        CollectionView.Filter = Predicate;
        return CollectionView;
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
        foreach ((int column, string criterion) in filter)
        {
            if (column == -1)
            {
                continue;
            }

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

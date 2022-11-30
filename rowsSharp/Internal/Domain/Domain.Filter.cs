using rowsSharp.Model;
using rowsSharp.ViewModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace rowsSharp.Domain;

internal class Filter
{
    private readonly Status status;
    private readonly DataStore.Config config;
    private readonly DataStore.Csv csv;
    private readonly RecordsView recordsView;

    internal Filter(Status inputStatus, DataStore.Config inputConfig, DataStore.Csv inputCsv, RecordsView inputRecordsView)
    {
        status = inputStatus;
        config = inputConfig;
        csv = inputCsv;
        recordsView = inputRecordsView;
    }

    internal void FocusFilter()
    {
        // Force update ViewModel by refocusing
        status.IsFilterFocused = false;
        status.IsFilterFocused = true;
    }

    private List<KeyValuePair<int, string>> criteria = new();

    private List<KeyValuePair<int, string>> ParseInput()
    {
        List<KeyValuePair<int, string>> output = new();

        string[] splitFilterText = Regex.Split(status.FilterText, "\\s+(?=(?:\"[^\"]*\"|[^\"])*$)");
        foreach (string criterion in splitFilterText)
        {
            string[] keyvalue = Regex.Split(criterion, ":(?=(?:\"[^\"]*\"|[^\"])*$)");
            // Handle value-only criterion (default)
            int column = -1;
            string value = keyvalue[^1].Trim('"');

            // Extra handling for Key:Value
            if (keyvalue.Length == 2)
            {
                string header = keyvalue[0].Trim('"');
                column = csv.Headers.IndexOf(header);

                if (column == -1)
                {
                    throw new InvalidFilterCriteriaException($"Invalid column {header}");
                }

                // Input alias
                value = keyvalue[1].Trim('"');
                if (config.UseInputAlias)
                {
                    Dictionary<string, string> columnAlias = config.Style.Alias.GetValueOrDefault(header, new());
                    foreach ((string raw, string aliased) in columnAlias)
                    {
                        value = value.Replace(aliased, raw);
                    }
                }
            }

            // Validate regular expression
            if (config.UseRegexFilter)
            {
                try
                {
                    Regex.IsMatch("", value);
                }
                catch
                {
                    throw new InvalidFilterCriteriaException($"Invalid regex {value}");
                }
            }
            output.Add(new(column, value));
        }
        return output;
    }

    private ICollectionView OutputAlias()
    {
        List<Record> tempRecords = new();
        foreach (Record record in recordsView.CollectionView)
        {
            Record thisRecord = record.DeepCopy(csv.Headers.Count);
            for (int i = 0; i < csv.Headers.Count; i++)
            {
                Dictionary<string, string> thisAlias = config.Style.Alias.GetValueOrDefault(csv.Headers[i], new());
                foreach ((string raw, string aliased) in thisAlias)
                {
                    thisRecord.SetField(
                        i,
                        thisRecord.GetField(i).Replace(raw, aliased)
                    );
                }
            }
            tempRecords.Add(thisRecord);
        }
        return CollectionViewSource.GetDefaultView(tempRecords);
    }

    private bool RecordsViewFilter(object obj)
    {
        var row = (Record)obj;
        foreach ((int column, string pattern) in criteria)
        {
            string input = column == -1
                ? row.ConcatenateFields(csv.Headers.Count)
                : row.GetField(column);

            if (
                (config.UseRegexFilter && Regex.IsMatch(input.ToLower(), pattern))
                || (!config.UseRegexFilter && input.ToLower().Contains(pattern.ToLower()))
            )
            { continue; }
            return false;
        }
        return true;
    }

    internal void DoFilter()
    {
        App.Logger.Info("Filtering CSV, ({filter}, IOAlias: {IAlias}, {OAlias})",
            status.FilterText,
            config.UseInputAlias,
            config.UseOutputAlias
        );

        // Parse input
        try
        {
            criteria = ParseInput();
        }
        catch (InvalidFilterCriteriaException ex)
        {
            App.Logger.Warn(ex.Message);
            recordsView.CollectionView.Filter = (object obj) => false;
            return;
        }

        // Filtering
        recordsView.CollectionView = CollectionViewSource.GetDefaultView(csv.Records);
        recordsView.CollectionView.Filter = RecordsViewFilter;

        // Output alias
        if (config.UseOutputAlias)
        {
            recordsView.CollectionView = OutputAlias();
        }

        criteria = new();
        status.PreviewBitmap = new();
        status.SelectedIndex = -1;
        App.Logger.Debug("Filtering CSV completed");
    }
}

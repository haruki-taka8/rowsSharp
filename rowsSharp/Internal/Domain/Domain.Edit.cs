using rowsSharp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace rowsSharp.Domain;
internal class Edit : INPC
{
    private readonly ViewModel.Status status;
    private readonly DataStore.Config config;
    private readonly DataStore.Csv csv;
    private readonly CsvRowHelper csvRowHelper;
    private readonly History history;

    internal Edit (ViewModel.Status status, DataStore.Config config, DataStore.Csv csv, CsvRowHelper csvRowHelper, History history)
    {
        this.status = status;
        this.config = config;
        this.csv = csv;
        this.csvRowHelper = csvRowHelper;
        this.history = history;
    }

    internal void OutputAliasEditing()
    {
        // Make ReadWrite FALSE when OutputAlias is TRUE
        // Revert OriginalCanEdit when OutputAlias is FALSE
        if (config.UseOutputAlias) { config.OriginalCanEdit = config.CanEdit; }
        config.CanEdit = !config.UseOutputAlias && config.OriginalCanEdit;
    }

    internal void BeginEdit() { status.IsEditing = true; }

    internal void EndEdit(DataGridCellEditEndingEventArgs e)
    {
        var record = (Record)e.Row.Item;
        int columnIndex = csv.Headers.IndexOf(e.Column.Header.ToString()!);
        string oldString = CsvRowHelper.GetField(record, columnIndex);

        if (((TextBox)e.EditingElement).Text == oldString) { return; }
        
        history.AddOperation(
            OperationType.Inline,
            csv.Records.IndexOf(record),
            csvRowHelper.DeepCopy(record)
        );
        history.CommitOperation();
    }

    internal bool CanInsertTopOrBottom() =>
        config.CanEdit &&
        (
            (config.InsertSelectedCount && status.SelectedItems.Any()) ||
            (!config.InsertSelectedCount)
        );

    internal bool IsAnyRowSelected() =>
        config.CanEdit && status.SelectedIndex != -1;

    internal void Remove()
    {
        App.Logger.Info("Removing rows (x{Count})", status.SelectedItems.Count);
        foreach (Record item in status.SelectedItems)
        {
            history.AddOperation(
                OperationType.Remove,
                csv.Records.IndexOf(item),
                item
            );
            csv.Records.Remove(item);
        }
        history.CommitOperation();
    }

    internal void Insert(int at)
    {
        if (at == -1) { at = csv.Records.Count; }

        int count = config.InsertSelectedCount
            ? status.SelectedItems.Count
            : config.InsertCount;

        App.Logger.Info("Inserting CSV (@{At} x{Count}, Template: {Template})", at, count, config.UseInsertTemplate);
        status.IsInsertExpanded = false;

        DateTime now = DateTime.Now;
        Record templatedRow = new();

        // Templating. Expand static <[DdTt]> fields beforehand.
        if (config.UseInsertTemplate)
        {
            foreach (KeyValuePair<string, string> keyValuePair in config.Style.Template)
            {
                int columnIndex = csv.Headers.IndexOf(keyValuePair.Key);
                CsvRowHelper.SetField(
                    templatedRow,
                    columnIndex,
                    keyValuePair.Value
                        .Replace("<D>", now.ToString("yyyyMMdd"))
                        .Replace("<d>", now.ToString("yyyy-MM-dd"))
                        .Replace("<T>", now.ToString("HHmmss"))
                        .Replace("<t>", now.ToString("HH:mm:ss"))
                );
            }
        }

        // Insert
        for (int i = 0; i < count; i++)
        {
            Record thisRow = csvRowHelper.DeepCopy(templatedRow);
            for (int j = 0; j < csv.Headers.Count; j++)
            {
                CsvRowHelper.SetField(
                    thisRow,
                    j,
                    CsvRowHelper.GetField(thisRow, j)
                        .Replace("<#>", i.ToString())
                        .Replace("<!#>", (count - i - 1).ToString())
                );
            }

            csv.Records.Insert(at + i, thisRow);
            history.AddOperation(OperationType.Insert, at + i, thisRow);
        }
        history.CommitOperation();
        status.SelectedIndex = at;
    }

    internal void Save()
    {
        App.Logger.Info("Saving");
        using StreamWriter writer = new(config.CsvPath);

        string fullHeader = string.Join(
            ",",
            csv.Headers.Select(m => "\"" + m.Replace("\"", "\"\"") + "\"")
        );
        writer.WriteLine(fullHeader);

        foreach (Record record in csv.Records)
        {
            string toOutput = csvRowHelper.ConcatenateFields(record);
            writer.WriteLine(toOutput);
        }

        status.IsDirtyEditor = false;
    }
}

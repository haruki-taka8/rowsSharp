using ObservableTable.Core;
using rowsSharp.Domain;
using rowsSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using rowsSharp.View;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace rowsSharp.ViewModel.Editor;

public class Edit : NotifyPropertyChanged, IDisposable
{
    private readonly RootVM rootVM;
    private Preferences Preferences => rootVM.Preferences;
    private ObservableTable<string> Table => rootVM.Table;

    private readonly UniqueColumn column = new();
    private readonly Timer timer;

    public Edit(RootVM rootViewModel)
    {
        rootVM = rootViewModel;

        Table.TableModified += Table_TableModified;

        timer = new(Preferences.AutosavePeriod * 1000);
        timer.Enabled = true;
        timer.AutoReset = true;
        timer.Elapsed += Timer_Elapsed;
    }

    public void Dispose()
    {
        timer.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!Preferences.UseAutosave || !Preferences.CanEdit || !isEditorDirty) { return; }

        CsvFile.Export(Preferences.CsvPath, Table, Preferences.HasHeader);
        IsEditorDirty = false;
    }

    private void Table_TableModified(object? sender, EventArgs e)
    {
        IsEditorDirty = true;
    }

    private bool isEditorDirty;
    public bool IsEditorDirty
    {
        get => isEditorDirty;
        private set => SetField(ref isEditorDirty, value);
    }


    private bool DataGridEditableAndSelected()
    {
        return Preferences.CanEdit && selectedCells.Count > 0;
    }

    public DelegateCommand Undo => new(
        () => Table.Undo(),
        () => Table.UndoCount > 0
    );

    public DelegateCommand Redo => new(
        () => Table.Redo(),
        () => Table.RedoCount > 0
    );

    private enum InsertionMode
    {
        Prepend,
        Before,
        After,
        Append
    }

    private void InsertRow(InsertionMode insertionMode)
    {
        int count = selectedCells.Count > 0
            ? selectedCells.Rows().Count()
            : 1;

        int index = insertionMode switch
        {
            InsertionMode.Prepend => 0,
            InsertionMode.Before => selectedCells.RowIndices(Table.Records).Min(),
            InsertionMode.After => selectedCells.RowIndices(Table.Records).Max() + 1,
            _ => Table.Records.Count
        };

        var template = Preferences.UseInsertTemplate ? Preferences.ColumnStyle?.Template : null;

        var toAdd = RowTemplate.Generate(count, Table.Headers, template);
        Table.InsertRow(index, toAdd);
    }

    public DelegateCommand InsertRowAbove => new(
        () => InsertRow(InsertionMode.Before),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertRowBelow => new(
        () => InsertRow(InsertionMode.After),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertRowFirst => new(
        () => InsertRow(InsertionMode.Prepend),
        () => Preferences.CanEdit
    );

    public DelegateCommand InsertRowLast => new(
        () => InsertRow(InsertionMode.Append),
        () => Preferences.CanEdit
    );

    public DelegateCommand NewTable => new(
        () =>
        {
            InsertColumn(InsertionMode.Append);
            InsertRow(InsertionMode.Append);
        },
        () => Preferences.CanEdit
    );

    private void InsertColumn(InsertionMode insertionMode)
    {
        int count = selectedCells.Count > 0
            ? selectedCells.Columns().Count()
            : 1;

        int index = insertionMode switch
        {
            InsertionMode.Prepend => 0,
            InsertionMode.Before => selectedCells.ColumnIndices(Table.Headers).Min(),
            InsertionMode.After => selectedCells.ColumnIndices(Table.Headers).Max() + 1,
            _ => Table.Headers.Count
        };

        Table.InsertColumn(index, column.Next(count));
    }

    public DelegateCommand InsertColumnLeft => new(
        () => InsertColumn(InsertionMode.Before),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertColumnRight => new(
        () => InsertColumn(InsertionMode.After),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertColumnFirst => new(
        () => InsertColumn(InsertionMode.Prepend),
        () => Preferences.CanEdit
    );

    public DelegateCommand InsertColumnLast => new(
        () => InsertColumn(InsertionMode.Append),
        () => Preferences.CanEdit
    );

    public DelegateCommand RemoveRows => new(
        () => Table.RemoveRow(selectedCells.Rows()),
        () => DataGridEditableAndSelected()
     );

    public DelegateCommand RemoveColumns => new(
        () => Table.RemoveColumn(selectedCells.Columns()),
        () => DataGridEditableAndSelected()
     );

    private IList<DataGridCellInfo> selectedCells = new List<DataGridCellInfo>();
    public IList<DataGridCellInfo> SelectedCells
    {
        get => selectedCells;
        set
        {
            SetField(ref selectedCells, value);
            OnPropertyChanged(nameof(SelectedRows));
            OnPropertyChanged(nameof(SelectedColumns));
        }
    }

    public DelegateCommand<IList<DataGridCellInfo>> ChangeSelectedCells => new(
        (cells) => { SelectedCells = cells; }
     );

    public int SelectedRows => selectedCells.Rows().Count();
    public int SelectedColumns => selectedCells.Columns().Count();

    public DelegateCommand Save => new(
        () =>
        {
            CsvFile.Export(Preferences.CsvPath, Table, Preferences.HasHeader);
            IsEditorDirty = false;
        },
        () => Preferences.CanEdit && isEditorDirty
    );

    public DelegateCommand RenameColumn => new(
        () =>
        {
            RenameColumn dialog = new();
            dialog.ShowDialog();
            string? newHeader = dialog.NewName;

            if (newHeader is null) { return; }

            int index = Table.Headers.IndexOf((string)selectedCells[0].Column.Header);
            Table.RenameColumn(index, newHeader);
        },
        () => DataGridEditableAndSelected() && Preferences.HasHeader
    );

    private IEnumerable<Cell<string>> EmptiedSelectedCells()
    {
        foreach (var cell in selectedCells)
        {
            int row = cell.RowIndex(Table.Records);
            int column = cell.ColumnIndex(Table.Headers);

            yield return new(row, column, null);
        }
    }

    public DelegateCommand ClearCells => new(
        () => Table.SetCell(EmptiedSelectedCells()),
        () => DataGridEditableAndSelected()
    );

    private IEnumerable<Cell<string>> EmptiedSelectedRows()
    {
        foreach (int row in selectedCells.RowIndices(Table.Records))
        {
            for (int i = 0; i < Table.Headers.Count; i++)
            {
                yield return new(row, i, null);
            }
        }
    }

    public DelegateCommand ClearRows => new(
        () => Table.SetCell(EmptiedSelectedRows()),
        () => DataGridEditableAndSelected()
    );

    private IEnumerable<Cell<string>> EmptiedSelectedColumns()
    {
        foreach (int column in selectedCells.ColumnIndices(Table.Headers))
        {
            for (int i = 0; i < Table.Records.Count; i++)
            {
                yield return new(i, column, null);
            }
        }
    }

    public DelegateCommand ClearColumns => new(
        () => Table.SetCell(EmptiedSelectedColumns()),
        () => DataGridEditableAndSelected()
    );

    private static IEnumerable<string[]> ParseClipboard()
    {
        string clipboard = Clipboard.GetText().ReplaceLineEndings().Trim();
        string[] lines = clipboard.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            yield return line.Split('\t');
        }
    }

    private IEnumerable<Cell<string>> YieldPasteCells(IList<string[]> toPaste, int rowOffset, int columnOffset)
    {
        for (int row = 0; row < toPaste.Count; row++)
        {
            if (rowOffset + row > Table.Records.Count - 1) { continue; }
            for (int column = 0; column < toPaste[row].Length; column++)
            {
                if (columnOffset + column > Table.Headers.Count - 1) { continue; }
                yield return new(rowOffset + row, columnOffset + column, toPaste[row][column]);
            }
        }
    }

    public DelegateCommand Paste => new(
        () =>
        {
            int rowOffset = selectedCells.RowIndices(Table.Records).Min();
            int columnOffset = selectedCells.ColumnIndices(Table.Headers).Min();

            var toPaste = ParseClipboard().ToList();
            Table.SetCell(YieldPasteCells(toPaste, rowOffset, columnOffset));
        },
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand<DataGridColumnEventArgs> ReorderColumn => new(
        (e) =>
        {
            int oldIndex = Table.Headers.IndexOf((string)e.Column.Header);
            int newIndex = e.Column.DisplayIndex;

            Table.ReorderColumn(oldIndex, newIndex);
        },
        (e) => Preferences.CanEdit
    );
}

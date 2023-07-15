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

namespace rowsSharp.ViewModel.Editor;

public class Edit : NotifyPropertyChanged
{
    private readonly RootVM rootVM;
    private Preferences Preferences => rootVM.Preferences;
    private ObservableTable<string> Table => rootVM.Table;

    private readonly UniqueColumn column = new();
    private readonly Autosave autosave;

    private bool isAutosaveEnabled;
    public bool IsAutosaveEnabled
    {
        get => isAutosaveEnabled;
        set
        {
            autosave.Enabled = value;
            SetField(ref isAutosaveEnabled, value);
        }
    }

    private double autosaveInterval;
    public double AutosaveInterval
    {
        get => autosaveInterval;
        set
        {
            autosave.Interval = value;
            SetField(ref autosaveInterval, value);
        }
    }

    public Edit(RootVM rootViewModel)
    {
        rootVM = rootViewModel;

        Table.TableModified += Table_TableModified;

        autosave = new(Autosave_Elapsed);
        AutosaveInterval = Preferences.Editor.AutosaveInterval;
        IsAutosaveEnabled = Preferences.Editor.IsAutosaveEnabled;
    }

    private void Autosave_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!Preferences.Editor.IsAutosaveEnabled || !Preferences.Editor.CanEdit || !isEditorDirty) { return; }
    
        CsvFile.Export(Preferences.Csv.Path, Table, Preferences.Csv.HasHeader);
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
        return Preferences.Editor.CanEdit && selectedCells.Count > 0;
    }

    public DelegateCommand Undo => new(
        () => Table.Undo(),
        () => Preferences.Editor.CanEdit && Table.UndoCount > 0
    );

    public DelegateCommand Redo => new(
        () => Table.Redo(),
        () => Preferences.Editor.CanEdit && Table.RedoCount > 0
    );

    private enum Insertion
    {
        Prepend,
        Before,
        After,
        Append
    }

    private void InsertRow(Insertion insertionMode)
    {
        int count = selectedCells.Count > 0
            ? selectedCells.Rows().Count()
            : 1;

        int index = insertionMode switch
        {
            Insertion.Prepend => 0,
            Insertion.Before => selectedCells.RowIndices(Table.Records).Min(),
            Insertion.After => selectedCells.RowIndices(Table.Records).Max() + 1,
            _ => Table.Records.Count
        };

        var template = Preferences.Editor.IsTemplateEnabled ? Preferences.Editor.ColumnStyles : null;
        
        var toAdd = RowTemplate.Generate(count, Table.Headers, template);
        Table.InsertRow(index, toAdd);
    }

    public DelegateCommand InsertRowAbove => new(
        () => InsertRow(Insertion.Before),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertRowBelow => new(
        () => InsertRow(Insertion.After),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertRowFirst => new(
        () => InsertRow(Insertion.Prepend),
        () => Preferences.Editor.CanEdit
    );

    public DelegateCommand InsertRowLast => new(
        () => InsertRow(Insertion.Append),
        () => Preferences.Editor.CanEdit
    );

    public DelegateCommand NewTable => new(
        () =>
        {
            InsertColumn(Insertion.Append);
            InsertRow(Insertion.Append);
        },
        () => Preferences.Editor.CanEdit
    );

    private void InsertColumn(Insertion insertionMode)
    {
        int count = selectedCells.Count > 0
            ? selectedCells.Columns().Count()
            : 1;

        int index = insertionMode switch
        {
            Insertion.Prepend => 0,
            Insertion.Before => selectedCells.ColumnIndices(Table.Headers).Min(),
            Insertion.After => selectedCells.ColumnIndices(Table.Headers).Max() + 1,
            _ => Table.Headers.Count
        };

        Table.InsertColumn(index, column.Next(count));
    }

    public DelegateCommand InsertColumnLeft => new(
        () => InsertColumn(Insertion.Before),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertColumnRight => new(
        () => InsertColumn(Insertion.After),
        () => DataGridEditableAndSelected()
    );

    public DelegateCommand InsertColumnFirst => new(
        () => InsertColumn(Insertion.Prepend),
        () => Preferences.Editor.CanEdit
    );

    public DelegateCommand InsertColumnLast => new(
        () => InsertColumn(Insertion.Append),
        () => Preferences.Editor.CanEdit
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
            CsvFile.Export(Preferences.Csv.Path, Table, Preferences.Csv.HasHeader);
            IsEditorDirty = false;
        },
        () => Preferences.Editor.CanEdit && isEditorDirty
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
        () => DataGridEditableAndSelected() && Preferences.Csv.HasHeader
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
        (e) => Preferences.Editor.CanEdit
    );
}

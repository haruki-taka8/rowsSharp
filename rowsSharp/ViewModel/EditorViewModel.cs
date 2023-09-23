using ObservableTable.Core;
using RowsSharp.Domain;
using RowsSharp.Model;
using RowsSharp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RowsSharp.ViewModel;

public enum Position
{
    Prepend,
    Before,
    After,
    Append
}

public enum Container
{
    Row,
    Column,
    Cell
}

public class EditorViewModel : NotifyPropertyChanged
{
    private readonly CommonViewModel commonViewModel;
    public ObservableTable<string> Table => commonViewModel.Table;
    public Preferences Preferences => commonViewModel.Preferences;

    public bool IsEditorDirty
    {
        get => commonViewModel.IsEditorDirty;
        set => commonViewModel.IsEditorDirty = value;
    }

    public ICollectionView CollectionView { get; }

    public IEnumerable<DataGridColumn> DataGridColumns
    {
        get
        {
            ObservableCollection<DataGridColumn> columns = new();
    
            for (int i = 0; i < Table.Headers.Count; i++)
            {
                string thisHeader = Table.Headers[i];

                DataGridTextColumn column = new()
                {
                    Header = thisHeader,
                    Binding = new Binding("[" + i + "]"),
                    EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(Preferences.Editor.CanInsertNewline)
                };

                var style = Preferences.Editor.ColumnStyles.FirstOrDefault(x => x.Column == thisHeader);

                column.Width = style!.Width > 0 ? style.Width : (DataGridLength)DependencyProperty.UnsetValue;
                column.CellStyle = ColumnStyleHelper.GetConditionalFormatting(i, style.ConditionalFormatting);

                columns.Add(column);
            }
            return columns;
        }
    }

    private void UpdatePreview()
    {
        if (!selectedCells.Any())
        {
            Preview = null;
            return;
        }

        string path = ColumnNotation.Expand(Preferences.Preview.Path, Table.Headers, (IEnumerable<string?>)selectedCells[0].Item);
        Preview = PreviewHelper.FromPath(path);
    }

    private BitmapImage? preview;
    public BitmapImage? Preview
    {
        get => preview;
        internal set => SetField(ref preview, value);
    }

    private IList<DataGridCellInfo> selectedCells = new List<DataGridCellInfo>();

    public int SelectedRowsCount => selectedCells.Rows().Count();
    public int SelectedColumnsCount => selectedCells.Columns().Count();

    private string filterText = "";
    public string FilterText
    {
        get => filterText;
        set => SetField(ref filterText, value);
    }

    private bool isFilterFocused;
    public bool IsFilterFocused
    {
        get => isFilterFocused;
        set
        {
            if (value)
            {
                // Force WPF to refocus onto textbox
                SetField(ref isFilterFocused, false);
            }
            SetField(ref isFilterFocused, value);
        }
    }

    private bool isSorted;
    public bool IsSorted
    {
        get => isSorted;
        set => SetField(ref isSorted, value);
    }

    public string CsvName => Preferences.Csv.Path.Split('/', '\\')[^1];

    public EditorViewModel(CommonViewModel commonViewModel)
    {
        this.commonViewModel = commonViewModel;
        CollectionView = CollectionViewSource.GetDefaultView(Table.Records);

        ((INotifyCollectionChanged)Table.Headers).CollectionChanged += Headers_Changed;
        Table.TableModified += Table_TableModified;

        _ = new BackgroundTimer()
        {
            ElapsedAction = Autosave,
            Interval = Preferences.Editor.AutosaveInterval,
            Enabled = Preferences.Editor.IsAutosaveEnabled
        };
    }

    private void Table_TableModified(object? sender, EventArgs e)
    {
        IsEditorDirty = true;
    }

    private void Headers_Changed(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(DataGridColumns));
    }

    private void Autosave()
    {
        if (Preferences.Editor.IsAutosaveEnabled && Preferences.Editor.CanEdit && IsEditorDirty)
        {
            Save.Execute(null);
        }
    }

    // Commands
    // View
    public DelegateCommand<IList<DataGridCellInfo>> ChangeSelectedCells => new(
        (e) =>
        {
            selectedCells = e;
            OnPropertyChanged(nameof(SelectedRowsCount));
            OnPropertyChanged(nameof(SelectedColumnsCount));
            UpdatePreview();
        }
    );

    public DelegateCommand FocusFilter => new(
        () => IsFilterFocused = true
    );

    public DelegateCommand InvokeFilter => new(
        () =>
        {
            Domain.Filter filter = new()
            {
                UseRegex = Preferences.Filter.IsRegexEnabled,
                Headers = Table.Headers,
                FilterText = FilterText
            };
            CollectionView.Filter = filter.Invoke();
        }
    );

    public DelegateCommand CopyPreview => new(
        () => ClipboardHelper.SetImage(Preview!),
        () => Preview is not null
    );

    public DelegateCommand SortColumn => new(
        () => IsSorted = true
    );

    public DelegateCommand<IEnumerable<DataGridColumn>> ResetSorting => new(
        (e) => {
            CollectionView.SortDescriptions.Clear();
            foreach (DataGridColumn column in e)
            {
                column.SortDirection = null;
            }
            IsSorted = false;
        },
        (e) => IsSorted
    );

    // Editing
    private readonly UniqueColumn uniqueColumn = new();

    public DelegateCommand Save => new(
        () =>
        {
            CsvFile.Export(Preferences.Csv.Path, Table, Preferences.Csv.HasHeader);
            IsEditorDirty = false;
        },
        () => Preferences.Editor.CanEdit && IsEditorDirty
    );

    public DelegateCommand RenameColumn => new(
        () =>
        {
            RenameColumn dialog = new();
            dialog.ShowDialog();
            string? newHeader = dialog.NewName;

            if (newHeader is null) { return; }

            int index = selectedCells[0].ColumnIndex(Table.Headers);
            Table.RenameColumn(index, newHeader);
        },
        () => Preferences.Editor.CanEdit && selectedCells.Any() && Preferences.Csv.HasHeader
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

    public DelegateCommand Paste => new(
        () =>
        {
            int rowOffset = selectedCells.RowIndices(Table.Records).Min();
            int columnOffset = selectedCells.ColumnIndices(Table.Headers).Min();

            var toPaste = ClipboardHelper.SplitTo2DArray();
            Table.FillGrid(toPaste, rowOffset, columnOffset);
        },
        () => Preferences.Editor.CanEdit && selectedCells.Any()
    );

    private void InsertRow_Private(Position position)
    {
        int count = Math.Max(1, SelectedRowsCount);
        var rows = RowTemplate.Generate(count, Table.Headers, Preferences.Editor.ColumnStyles);

        int index = position switch
        {
            _ when !selectedCells.Any() => 0,

            Position.Prepend => 0,
            Position.Before => selectedCells.RowIndices(Table.Records).Min(),
            Position.After => selectedCells.RowIndices(Table.Records).Max() + 1,
            _ => Table.Records.Count,
        };

        Table.InsertRow(index, rows);
    }

    private void InsertColumn_Private(Position position)
    {
        int count = Math.Max(1, SelectedColumnsCount);
        var columns = uniqueColumn.Next(count);

        int index = position switch
        {
            _ when !selectedCells.Any() => 0,

            Position.Prepend => 0,
            Position.Before => selectedCells.ColumnIndices(Table.Headers).Min(),
            Position.After => selectedCells.ColumnIndices(Table.Headers).Max() + 1,
            _ => Table.Headers.Count,
        };

        Table.InsertColumn(index, columns);
    }


    public DelegateCommand<Position> InsertRow => new(
        (position) => InsertRow_Private(position),
        (position) => Preferences.Editor.CanEdit
    );

    public DelegateCommand<Position> InsertColumn => new(
        (position) => InsertColumn_Private(position),
        (position) => Preferences.Editor.CanEdit
    );

    public DelegateCommand RemoveSelectedRows => new(
        () => Table.RemoveRow(selectedCells.Rows()),
        () => Preferences.Editor.CanEdit
    );

    public DelegateCommand RemoveSelectedColumns => new(
        () => Table.RemoveColumn(selectedCells.Columns()),
        () => Preferences.Editor.CanEdit
    );

    public DelegateCommand Undo => new(
        () => Table.Undo(),
        () => Table.UndoCount > 0
    );

    public DelegateCommand Redo => new(
        () => Table.Redo(),
        () => Table.RedoCount > 0
    );

    private void Clear_Private(Container container)
    {
        switch (container)
        {
            case Container.Row:
                Table.FillRow(null, selectedCells.RowIndices(Table.Records));
                break;

            case Container.Column:
                Table.FillColumn(null, selectedCells.ColumnIndices(Table.Headers));
                break;

            case Container.Cell:
                Table.FillCell(null, selectedCells.RowColumnPairs(Table.Records, Table.Headers));
                break;
        }
    }

    public DelegateCommand<Container> Clear => new(
        (container) => Clear_Private(container),
        (container) => Preferences.Editor.CanEdit && selectedCells.Any()
    );

    // Others
    public DelegateCommand OpenPreferences => new(() =>
        commonViewModel.CurrentSection = Section.Settings
    );

    public DelegateCommand OpenHome => new(() =>
    {
        CancelEventArgs e = new();
        commonViewModel.Exit.Execute(e);

        if (e.Cancel) { return; }

        commonViewModel.CurrentSection = Section.Welcome;
    });
}

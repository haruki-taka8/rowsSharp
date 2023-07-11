using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ObservableTable.Core;
using rowsSharp.Domain;
using rowsSharp.Model;
using rowsSharp.ViewModel.Editor;

namespace rowsSharp.ViewModel;

public class EditorVM : NotifyPropertyChanged
{
    private readonly RootVM rootVM;
    private Preferences Preferences => rootVM.Preferences;
    private ObservableTable<string> Table => rootVM.Table;

    public string CsvName => System.IO.Path.GetFileName(Preferences.Csv.Path);

    public Edit Edit { get; set; }
    public Editor.Filter Filter { get; set; }
    public Editor.Preview Preview { get; set; }

    public EditorVM(RootVM rootViewModel)
    {
        rootVM = rootViewModel;
        collectionView = CollectionViewSource.GetDefaultView(Table.Records);

        Edit = new(rootVM);
        Filter = new(rootVM, CollectionView);
        Preview = new(rootVM);

        ((INotifyCollectionChanged)Table.Headers).CollectionChanged += HeadersChanged;
        HeadersChanged(null, new(NotifyCollectionChangedAction.Reset));
    }

    private void HeadersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DataGridColumns.Clear();

        for (int i = 0; i < Table.Headers.Count; i++)
        {
            string header = Table.Headers[i];

            DataGridTextColumn column = new()
            {
                Header = header,
                Binding = new Binding("[" + i + "]"),
                EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(Preferences.Editor.CanInsertNewline)
            };

            if (Preferences.Editor.ColumnStyles.TryGetValue(header, out var style))
            {
                column.Width = style.Width > 0 ? style.Width : column.Width;
                column.CellStyle = ColumnStyleHelper.GetConditionalFormatting(i, style.ConditionalFormatting);
            }

            DataGridColumns.Add(column);
        }
    }

    private ICollectionView collectionView;
    public ICollectionView CollectionView
    {
        get => collectionView;
        set => SetField(ref collectionView, value);
    }

    public ObservableCollection<DataGridColumn> DataGridColumns { get; } = new();
}

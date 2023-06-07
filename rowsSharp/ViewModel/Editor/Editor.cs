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

        ((INotifyCollectionChanged)Table.Headers).CollectionChanged += Headers_CollectionChanged;
    }

    private void Headers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(DataGridColumns));
    }

    private ICollectionView collectionView;
    public ICollectionView CollectionView
    {
        get => collectionView;
        set => SetField(ref collectionView, value);
    }

    private const int DefaultColumnWidth = 128;

    public ObservableCollection<DataGridColumn> DataGridColumns =>
        new(
            Table.Headers.Select((header, index) => new DataGridTextColumn()
            {
                Header = header,
                Binding = new Binding("[" + index + "]"),
                Width = Preferences.ColumnStyle.Width.GetValueOrDefault(header, DefaultColumnWidth),
                
                CellStyle = Preferences.ColumnStyle.Color.GetConditionalFormatting(header, index),
                EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(Preferences.AllowMultiline)
            })
        );
}

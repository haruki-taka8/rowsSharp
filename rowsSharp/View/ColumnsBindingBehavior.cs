using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;

namespace RowsSharp.View;

/* Code:    [ColumnsBindingBehavior](https://stackoverflow.com/a/40935553)
 * Creator: [Paul Gibson](https://stackoverflow.com/users/4024800/paul-gibson)
 * License: [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/)
 * Modifications:
 *   - Code formatting
 *   - Removing unnecessary bits
 */

public class ColumnsBindingBehavior : Behavior<DataGrid>
{
    public ObservableCollection<DataGridColumn> Columns
    {
        get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns",
        typeof(ObservableCollection<DataGridColumn>), typeof(ColumnsBindingBehavior),
            new(OnDataGridColumnsPropertyChanged));

    private static void OnDataGridColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
        var context = (ColumnsBindingBehavior)source;

        if (e.OldValue is ObservableCollection<DataGridColumn> oldItems)
        {
            RemoveColumns(context, oldItems);
        }

        if (e.NewValue is ObservableCollection<DataGridColumn> newItems)
        {
            AddColumns(context, newItems);
        }
    }

    private static void RemoveColumns(ColumnsBindingBehavior context, ObservableCollection<DataGridColumn> columns)
    {
        foreach (var column in columns)
        {
            context._datagridColumns.Remove(column);
        }
        columns.CollectionChanged -= context.CollectionChanged;
    }

    private static void AddColumns(ColumnsBindingBehavior context, ObservableCollection<DataGridColumn> columns)
    {
        foreach (var column in columns)
        {
            context._datagridColumns.Add(column);
        }
        columns.CollectionChanged += context.CollectionChanged;
    }

    private ObservableCollection<DataGridColumn> _datagridColumns = new();

    protected override void OnAttached()
    {
        base.OnAttached();
        _datagridColumns = AssociatedObject.Columns;
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                    foreach (DataGridColumn one in e.NewItems)
                        _datagridColumns.Insert(e.NewStartingIndex, one);
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                    _datagridColumns.RemoveAt(e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Move:
                _datagridColumns.Move(e.OldStartingIndex, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                _datagridColumns.Clear();
                if (e.NewItems != null)
                    foreach (DataGridColumn one in e.NewItems)
                        _datagridColumns.Add(one);
                break;
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace rowsSharp.Domain;

internal static class Cell
{
    private static IEnumerable<DataGridCellInfo> WhereValid(this IList<DataGridCellInfo> cells)
    {
        return cells.Where(x => x.IsValid);
    }

    // Rows
    internal static IEnumerable<ObservableCollection<string?>> Rows(this IList<DataGridCellInfo> cells)
    {
        return cells.WhereValid()
                    .Select(x => (ObservableCollection<string?>)x.Item)
                    .Distinct();
    }

    internal static IEnumerable<int> RowIndices(this IList<DataGridCellInfo> cells, IList<ObservableCollection<string?>> records)
    {
        return cells.Rows()
                    .Select(x => records.IndexOf(x));
    }

    // Columns
    internal static IEnumerable<string> Columns(this IList<DataGridCellInfo> cells)
    {
        return cells.WhereValid()
                    .Select(x => (string)x.Column.Header)
                    .Distinct();
    }

    internal static IEnumerable<int> ColumnIndices(this IList<DataGridCellInfo> cells, IList<string> headers)
    {
        return cells.Columns()
                    .Select(x => headers.IndexOf(x));
    }

    // Cell
    internal static int RowIndex(this DataGridCellInfo cell, IList<ObservableCollection<string?>> records)
    {
        return records.IndexOf((ObservableCollection<string?>)cell.Item);
    }

    internal static int ColumnIndex(this DataGridCellInfo cell, IList<string> headers)
    {
        return headers.IndexOf((string)cell.Column.Header);
    }
}

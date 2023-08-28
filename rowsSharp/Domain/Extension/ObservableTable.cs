using ObservableTable.Core;
using System.Collections.Generic;
using System;

namespace RowsSharp.Domain;

internal static class ObservableTableExtensions
{
    internal static void FillRow<T>(this ObservableTable<T> table, T? fill, IEnumerable<int> rows)
    {
        List<Cell<T>> cells = new();

        foreach (int row in rows)
        {
            for (int column = 0; column < table.Headers.Count; column++)
            {
                cells.Add(new(row, column, fill));
            }
        }

        table.SetCell(cells);
    }

    internal static void FillColumn<T>(this ObservableTable<T> table, T? fill, IEnumerable<int> columns)
    {
        List<Cell<T>> cells = new();

        foreach (int column in columns)
        {
            for (int row = 0; row < table.Records.Count; row++)
            {
                cells.Add(new(row, column, fill));
            }
        }

        table.SetCell(cells);
    }

    internal static void FillCell<T>(this ObservableTable<T> table, T? fill, IEnumerable<(int, int)> rowColumnPairs)
    {
        List<Cell<T>> cells = new();

        foreach ((int row, int column) in rowColumnPairs)
        {
            cells.Add(new(row, column, fill));
        }

        table.SetCell(cells);
    }

    internal static void FillGrid<T>(this ObservableTable<T> table, T?[,] fill, int rowOffset, int columnOffset)
    {
        List<Cell<T>> cells = new();

        int width = fill.GetLength(1);
        int height = fill.GetLength(0);
        int maxWidth = Math.Min(width, table.Headers.Count - columnOffset);
        int maxHeight = Math.Min(height, table.Records.Count - rowOffset);

        for (int row = 0; row < maxHeight; row++)
        {
            for (int column = 0; column < maxWidth; column++)
            {
                cells.Add(new(rowOffset + row, columnOffset + column, fill[row, column]));
            }
        }

        table.SetCell(cells);
    }
}

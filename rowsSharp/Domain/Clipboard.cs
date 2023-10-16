using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RowsSharp.Domain;

public static class ClipboardHelper
{
    public static void SetImage(BitmapImage bitmapImage)
    {
        Clipboard.SetImage(bitmapImage);
    }

    public static string[,] SplitTo2DArray()
    {
        string clipboard = Clipboard.GetText()
                                    .ReplaceLineEndings()
                                    .Trim();

        string[] allRows = clipboard.Split(Environment.NewLine);
        var table = allRows.Select(x => x.Split('\t')).ToList();

        int rows = table.Count;
        int columns = table.Select(x => x.Length).Max();

        string[,] results = new string[rows, columns];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < table[y].Length; x++)
            {
                results[y, x] = table[y][x];
            }
        }

        return results;            
    }
}

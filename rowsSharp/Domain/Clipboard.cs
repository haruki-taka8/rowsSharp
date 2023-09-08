using System;
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

        string[] fields = clipboard.Split(new string[] { Environment.NewLine, "\t" }, StringSplitOptions.None);

        int rowCount = clipboard.Split(Environment.NewLine).Length;
        int columnCount = fields.Length / rowCount;

        string[,] result = new string[rowCount, columnCount];

        for (int y = 0; y < rowCount; y++)
        {
            for (int x = 0; x < columnCount; x++)
            {
                result[y, x] = fields[y * columnCount + x];
            }
        }

        return result;            
    }
}

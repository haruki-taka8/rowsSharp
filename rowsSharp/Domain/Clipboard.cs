using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;

namespace RowsSharp.Domain;

public static class ClipboardHelper
{
    public static void SetFileFromPath(string path)
    {
        StringCollection list = new() { path };

        Clipboard.SetFileDropList(list);
    }

    public static string[,] SplitTable()
    {
        string clipboard = Clipboard.GetText()
                                    .ReplaceLineEndings();

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

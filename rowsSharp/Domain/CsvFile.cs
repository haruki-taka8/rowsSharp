using Microsoft.Win32;
using ObservableTable.Core;
using ObservableTable.IO;
using System.IO;

namespace rowsSharp.Domain;

internal class CsvFile
{
    internal static ObservableTable<string> Import(string path, bool hasHeader)
    {
        if (!File.Exists(path)) { return new(); }

        return Importer.FromFilePath(path, hasHeader);
    }

    private static string RequestFilePath()
    {
        SaveFileDialog dialog = new()
        {
            Filter = "Comma-seperated values (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = "csv"
        };

        dialog.ShowDialog();

        return dialog.FileName;
    }

    internal static void Export(string path, ObservableTable<string> table, bool hasHeader)
    {
        if (!File.Exists(path))
        {
            path = RequestFilePath();
        }

        Exporter.ToFile(path, table, hasHeader);
    }
}

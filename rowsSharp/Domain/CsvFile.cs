using ObservableTable.Core;
using ObservableTable.IO;
using System.IO;

namespace RowsSharp.Domain;

internal class CsvFile
{
    internal static ObservableTable<string> Import(string path, bool hasHeader)
    {
        if (!File.Exists(path)) { return new(); }

        return Importer.FromFilePath(path, hasHeader);
    }

    internal static void Export(string path, ObservableTable<string> table, bool hasHeader)
    {
        while (true)
        {
            try
            {
                table.ToFile(path, hasHeader);
                return;
            }
            catch (IOException)
            { 
                path = FileDialogHelper.RequestWritePath();
            }
        }
    }
}

using Microsoft.Win32;

namespace RowsSharp.Domain;

public static class FileDialogHelper
{
    private const string Filter = "Comma-separated values (*.csv)|*.csv|All files (*.*)|*.*";
    private const string CsvExtension = "csv";

    private static string RequestPath(FileDialog dialog)
    {
        dialog.Filter = Filter;
        dialog.DefaultExt = CsvExtension;

        dialog.ShowDialog();

        return dialog.FileName;
    }

    public static string RequestReadPath()
    {
        return RequestPath(new OpenFileDialog());
    }

    public static string RequestWritePath()
    {
        return RequestPath(new SaveFileDialog());
    }
}

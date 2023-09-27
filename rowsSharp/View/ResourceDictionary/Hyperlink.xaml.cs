using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace RowsSharp.View;

public partial class Hyperlink
{        
    public void Hyperlink_Clicked(object sender, RoutedEventArgs e)
    {
        var file = ((System.Windows.Documents.Hyperlink)e.OriginalSource)
            .NavigateUri
            .ToString();

        StartShell(file);
    }

    private static void StartShell(string file)
    {
        ProcessStartInfo info = new()
        {
            FileName = file,
            UseShellExecute = true,
        };

        try
        {
            Process.Start(info);
        }
        catch (Win32Exception)
        {
            // Invalid ProcessStartInfo.FileName
        }
    }
}

using System;
using System.Windows;

namespace rowsSharp;

public partial class App : Application
{
    internal static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private App()
    {
        AppDomain.CurrentDomain.UnhandledException += new(Handler);
    }

    private void Handler(object sender, UnhandledExceptionEventArgs args)
    {
        var exception = (Exception)args.ExceptionObject;

        MessageBox.Show(
           "RowsSharp will close due to the following exception:\n\n" + exception.Message + "\n\n" + exception.StackTrace,
           "RowsSharp",
           MessageBoxButton.OK,
           MessageBoxImage.Error
       );
    }
}

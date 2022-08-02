using System;
using System.Windows;

namespace rowsSharp;

public partial class App : Application
{
    public App()
    {
        AppDomain domain = AppDomain.CurrentDomain;
        domain.UnhandledException += new UnhandledExceptionEventHandler(Handler);
    }

    static void Handler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception inner = ((Exception)args.ExceptionObject).InnerException ?? new();

        MessageBox.Show(
           "RowsSharp will close due to the following exception:\n\n" + inner.Message + "\n\n" + inner.StackTrace,
           "RowsSharp",
           MessageBoxButton.OK,
           MessageBoxImage.Error
       );
    }
}

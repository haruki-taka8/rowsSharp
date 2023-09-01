using System;
using System.Windows;

namespace RowsSharp;

public partial class App : Application
{
    internal static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

    private App()
    {
        AppDomain.CurrentDomain.UnhandledException += Handler;
    }

    private void Handler(object sender, UnhandledExceptionEventArgs args)
    {
        MessageBox.Show(
           "RowsSharp will close due to the following error:\n\n" + args.ExceptionObject,
           "RowsSharp",
           MessageBoxButton.OK,
           MessageBoxImage.Error
       );
    }
}

using RowsSharp.ViewModel;
using System.Windows;

namespace RowsSharp.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new CommonViewModel();
        InitializeComponent();
        App.Logger.Info("Okay, it's happening! Everybody stay calm!");
    }
}

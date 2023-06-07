using rowsSharp.ViewModel;
using System.Windows;

namespace rowsSharp.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new RootVM();
        InitializeComponent();
        App.Logger.Info("Okay, it's happening! Everybody stay calm!");
    }
}

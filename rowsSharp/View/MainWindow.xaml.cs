using RowsSharp.Domain;
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
        CommonViewModel viewModel = new();
        viewModel.InitializeAsync();
        DataContext = viewModel;

        ResourceDictionary theme = PreferencesReader.GetTheme(viewModel.Preferences.UserInterface.ThemePath);
        Application.Current.Resources.MergedDictionaries.Add(theme);

        InitializeComponent();
        App.Logger.Info("Okay, it's happening! Everybody stay calm!");
    }
}

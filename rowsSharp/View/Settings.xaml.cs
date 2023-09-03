using RowsSharp.ViewModel;
using System.Windows.Controls;

namespace RowsSharp.View;

/// <summary>
/// Interaction logic for Settings.xaml
/// </summary>
public partial class Settings : UserControl
{
    public Settings(CommonViewModel commonViewModel)
    {
        DataContext = new SettingsViewModel(commonViewModel);
        InitializeComponent();
    }
}

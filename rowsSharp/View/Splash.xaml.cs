using RowsSharp.ViewModel;
using System.Windows.Controls;

namespace RowsSharp.View;

/// <summary>
/// Interaction logic for Splash.xaml
/// </summary>
public partial class Splash : UserControl
{
    public Splash(CommonViewModel commonViewModel)
    {
        DataContext = commonViewModel;
        InitializeComponent();
    }
}

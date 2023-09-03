using System.Windows.Controls;
using RowsSharp.ViewModel;

namespace RowsSharp.View;
/// <summary>
/// Interaction logic for Welcome.xaml
/// </summary>
public partial class Welcome : UserControl
{
    public Welcome(CommonViewModel commonViewModel)
    {
        DataContext = new WelcomeViewModel(commonViewModel);
        InitializeComponent();
    }
}

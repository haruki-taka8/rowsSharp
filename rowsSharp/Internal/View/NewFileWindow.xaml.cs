using rowsSharp.Model;
using rowsSharp.ViewModel;
using System.Windows;

namespace rowsSharp.View;

public partial class NewFileWindow : Window
{
    public NewFileWindow(Config config)
    {
        DataContext = new NewFileWindowVM(config);
        InitializeComponent();
    }

    // No, not writing a 100-line DepenencyProperty for one line of code.
    private void Button_Click(object sender, RoutedEventArgs e) { Close(); }
}

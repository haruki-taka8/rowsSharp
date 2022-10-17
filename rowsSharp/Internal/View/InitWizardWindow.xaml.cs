using System.Windows;

namespace rowsSharp;

public partial class InitWizardWindow : Window
{
    public InitWizardWindow(DataStore.Config config)
    {
        DataContext = new Domain.InitWizard(config);
        InitializeComponent();
        ShowDialog();
    }

    // No, not writing a 100-line DepenencyProperty for one line of code.
    private void Button_Click(object sender, RoutedEventArgs e) { Close(); }
}

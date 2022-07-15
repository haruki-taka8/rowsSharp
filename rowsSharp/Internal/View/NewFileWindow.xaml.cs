using System.Windows;
using rowsSharp.ViewModel;

namespace rowsSharp.View
{
    public partial class NewFileWindow : Window
    {
        public NewFileWindow(RowsVM inViewModel)
        {
            NewFileWindowVM viewModel = new(inViewModel);
            DataContext = viewModel;
            InitializeComponent();
        }

        // No, not writing a 100-line DepenencyProperty for one line of code.
        private void Button_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}

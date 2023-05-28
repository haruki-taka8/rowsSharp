using System.Windows;

namespace rowsSharp.View
{
    // MVVM is an overkill here, hence not implemented.

    public partial class RenameColumn : Window
    {
        public RenameColumn()
        {
            InitializeComponent();
        }

        public string? NewName { get; set; }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            NewName = ColumnName.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

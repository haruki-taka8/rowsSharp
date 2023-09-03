using RowsSharp.ViewModel;
using System.Windows.Controls;

namespace RowsSharp.View;

/// <summary>
/// Interaction logic for Home.xaml
/// </summary>
public partial class Editor : UserControl
{
    public Editor(CommonViewModel commonViewModel)
    {
        // Initialize the ViewModel after the View,
        // so that ColumnsBindingBehavior receives the ObservableCollection<DataGridColumn>
        InitializeComponent();
        DataContext = new EditorViewModel(commonViewModel);
    }
}

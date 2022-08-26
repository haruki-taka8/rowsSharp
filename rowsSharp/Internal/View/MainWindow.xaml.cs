using rowsSharp.Model;
using rowsSharp.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace rowsSharp.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly RowsVM viewModel;

    public MainWindow()
    {
        viewModel = new RowsVM();
        DataContext = viewModel;
        InitializeComponent();
        editingStyle = DefineEditingStyle();
        viewModel.Logger.Info("Okay, it's happening! Everybody stay calm!");
    }

    // Sorry, no MVVM
    private readonly Style editingStyle;

    private Style DefineEditingStyle()
    {
        Style style = new(
            typeof(TextBox),
            (Style)Application.Current.FindResource(typeof(TextBox))
        );

        style.Setters.Add(new Setter(System.Windows.Controls.Primitives.TextBoxBase.AcceptsReturnProperty, viewModel.Config.AllowMultiline));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
        return style;
    }

    private void Grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        int columnIndex = ((DataGrid)sender).Columns.Count;
        if (columnIndex >= viewModel.Csv.Headers.Count)
        {
            e.Cancel = true;
            return;
        }

        DataGridTextColumn column = (DataGridTextColumn)e.Column;

        string columnName = viewModel.Csv.Headers[columnIndex];
        column.Header = columnName;
        column.EditingElementStyle = editingStyle;

        // Column width
        if (viewModel.Config.Style.Width.ContainsKey(columnName))
        {
            column.Width = viewModel.Config.Style.Width[columnName];
        }

        // Conditional formatting
        if (!viewModel.Config.Style.Color.ContainsKey(columnName)) { return; }
        column.CellStyle = new();

        foreach (KeyValuePair<string, string> colorPair in viewModel.Config.Style.Color[columnName])
        {
            DataTrigger trigger = new()
            {
                Binding = new Binding("Column" + columnIndex),
                Value = colorPair.Key
            };

            trigger.Setters.Add(
                new Setter()
                {
                    Property = BackgroundProperty,
                    Value = new BrushConverter().ConvertFromString(colorPair.Value)
                }
            );
            column.CellStyle.Triggers.Add(trigger);
        }
    }

    private void Grid_CurrentCellChanged(object sender, System.EventArgs e)
    {
        ((DataGrid)sender).CommitEdit();
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DataGrid dataGrid = (DataGrid)sender;
        List<Record> selected = dataGrid.SelectedItems.Cast<Record>().ToList();

        viewModel.Edit.SelectedItems = selected;
        if (selected.Any()) { dataGrid.ScrollIntoView(selected[0]); }
    }
}

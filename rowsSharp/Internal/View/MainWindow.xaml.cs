using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace rowsSharp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // private readonly RowsVM viewModel;
    private readonly DataContext dataContext = new();

    public MainWindow()
    {
        DataContext = dataContext;
        InitializeComponent();
        editingStyle = DefineEditingStyle();
        App.Logger.Info("Okay, it's happening! Everybody stay calm!");
    }

    // Sorry, no MVVM
    private readonly Style editingStyle;

    private Style DefineEditingStyle()
    {
        Style style = new(
            typeof(TextBox),
            (Style)Application.Current.FindResource(typeof(TextBox))
        );

        style.Setters.Add(new Setter(System.Windows.Controls.Primitives.TextBoxBase.AcceptsReturnProperty, dataContext.Config.AllowMultiline));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
        return style;
    }

    private void Grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        int columnIndex = ((DataGrid)sender).Columns.Count;
        if (columnIndex >= dataContext.Csv.Headers.Count)
        {
            e.Cancel = true;
            return;
        }

        DataGridTextColumn column = (DataGridTextColumn)e.Column;

        string columnName = dataContext.Csv.Headers[columnIndex];
        column.Header = columnName;
        column.EditingElementStyle = editingStyle;

        // Column width
        if (dataContext.Config.Style.Width.ContainsKey(columnName))
        {
            column.Width = dataContext.Config.Style.Width[columnName];
        }

        // Conditional formatting
        if (!dataContext.Config.Style.Color.ContainsKey(columnName)) { return; }
        column.CellStyle = new();

        foreach (KeyValuePair<string, string> colorPair in dataContext.Config.Style.Color[columnName])
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
        List<Model.Record> selected = dataGrid.SelectedItems.Cast<Model.Record>().ToList();

        dataContext.Status.SelectedItems = selected;
        dataContext.Preview.UpdatePreview();
        if (selected.Any()) { dataGrid.ScrollIntoView(selected[0]); }
    }
}

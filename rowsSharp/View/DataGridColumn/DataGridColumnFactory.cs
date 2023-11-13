using RowsSharp.Model;
using System.Windows.Controls;
using System.Windows.Data;

namespace RowsSharp.View;

internal static class DataGridColumnFactory
{
    internal static DataGridColumn CreateColumn(int columnIndex, ColumnStyle style, bool canInsertNewline)
    {
        Binding binding = new("[" + columnIndex + "]");

        DataGridBoundColumn column;

        switch (style.ColumnType)
        {
            case ColumnType.Hyperlink:
                binding.Converter = new StringToUriConverter();

                column = new DataGridHyperlinkColumn()
                {
                    EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(canInsertNewline)
                };
                break;

            case ColumnType.CheckBox:
                binding.Converter = new StringToBooleanConverter();
                binding.ConverterParameter = (style.CheckBoxTrueValue, style.CheckBoxFalseValue);

                column = new DataGridCheckBoxColumn()
                {
                    ElementStyle = ColumnStyleHelper.GetDefaultStyle(typeof(CheckBox))
                };
                break;

            default:
                column = new DataGridTextColumn()
                {
                    EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(canInsertNewline)
                };
                break;
        }

        column.Binding = binding;
        column.Header = style.Column;
        column.Width = style.Width > 0 ? style.Width : column.Width;
        if (style.MinWidth > 0) { column.MinWidth = style.MinWidth; }
        if (style.MaxWidth > 0) { column.MaxWidth = style.MaxWidth; }
        column.CellStyle = ColumnStyleHelper.GetConditionalFormatting(style.ConditionalFormatting);

        return column;
    }
}

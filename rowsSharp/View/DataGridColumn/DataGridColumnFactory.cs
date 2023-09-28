using RowsSharp.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RowsSharp.View;

internal static class DataGridColumnFactory
{
    internal static DataGridColumn CreateColumn(int columnIndex, string header, ColumnStyle style, bool canInsertNewline)
    {
        Binding binding = new("[" + columnIndex + "]");

        DataGridColumn column;

        switch (style.ColumnType)
        {
            case ColumnType.Text:
                column = new DataGridTextColumn()
                {
                    Binding = binding,
                    EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(canInsertNewline)
                };
                break;

            case ColumnType.Hyperlink:
                binding.Converter = new StringToUriConverter();

                column = new DataGridHyperlinkColumn()
                {
                    Binding = binding,
                    EditingElementStyle = ColumnStyleHelper.GetEditingElementStyle(canInsertNewline),
                };
                break;

            case ColumnType.CheckBox:
                binding.Converter = new StringToBooleanConverter();
                binding.ConverterParameter = (style.CheckBoxTrueValue, style.CheckBoxFalseValue);

                column = new DataGridCheckBoxColumn()
                {
                    Binding = binding,
                    ElementStyle = ColumnStyleHelper.GetDefaultStyle(typeof(CheckBox))
                };
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(style.ColumnType));
        }

        column.Header = header;
        column.Width = style.Width > 0 ? style.Width : DataGridLength.Auto;
        column.CellStyle = ColumnStyleHelper.GetConditionalFormatting(binding, style.ConditionalFormatting);

        return column;
    }
}

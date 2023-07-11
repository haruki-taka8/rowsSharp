using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace rowsSharp.Domain;

internal static class ColumnStyleHelper
{
    private static Style GetDefaultStyle(Type type)
    {
        return new(
            type,
            (Style)Application.Current.FindResource(type)
        );
    }


    internal static Style GetConditionalFormatting(int column, IDictionary<string, string> rules)
    {
        Style style = GetDefaultStyle(typeof(DataGridCell));

        foreach (var (key, value) in rules)
        {
            style.Triggers.Add(GetDataTrigger(column, key, value));
        }

        return style;
    }

    private static DataTrigger GetDataTrigger(int column, string match, string color)
    {
        DataTrigger dataTrigger = new()
        {
            Binding = new Binding("[" + column + "]"),
            Value = match
        };

        dataTrigger.Setters.Add(new Setter()
        {
            Property = Control.BackgroundProperty,
            Value = new BrushConverter().ConvertFrom(color)
        });

        return dataTrigger;
    }

    private static Style? editingElementStyle;
    private static Style SetEditingElementStyle(bool allowMultiline)
    {
        Style style = GetDefaultStyle(typeof(TextBoxBase));

        List<Setter> setters = new()
        {
            new(TextBoxBase.AcceptsReturnProperty, allowMultiline),
            new(TextBoxBase.PaddingProperty, new Thickness(1, 1, 0, 0)),
            new(TextBoxBase.MarginProperty, new Thickness(-2))
        };

        style.Setters.AddRange(setters);
        return style;
    }

    internal static Style GetEditingElementStyle(bool allowMultiline)
        => editingElementStyle ??= SetEditingElementStyle(allowMultiline);
}

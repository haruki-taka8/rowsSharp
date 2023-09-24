using RowsSharp.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace RowsSharp.View;

internal static class ColumnStyleHelper
{
    internal static Style GetDefaultStyle(Type type)
    {
        return new(
            type,
            (Style)Application.Current.FindResource(type)
        );
    }

    internal static Style GetConditionalFormatting(Binding binding, IEnumerable<ConditionalFormatting> conditionalFormattings)
    {
        Style style = GetDefaultStyle(typeof(DataGridCell));

        foreach (var conditionalFormatting in conditionalFormattings)
        {
            style.Triggers.Add(GetDataTrigger(binding, conditionalFormatting));
        }

        return style;
    }

    private static DataTrigger GetDataTrigger(Binding binding, ConditionalFormatting conditionalFormatting)
    {
        DataTrigger dataTrigger = new()
        {
            Binding = binding,
            Value = conditionalFormatting.Match
        };

        dataTrigger.Setters.Add(new Setter()
        {
            Property = Control.BackgroundProperty,
            Value = new BrushConverter().ConvertFrom(conditionalFormatting.Background)
        });

        return dataTrigger;
    }

    private static Style? editingElementStyle;
    private static Style SetEditingElementStyle(bool allowMultiline)
    {
        Style style = GetDefaultStyle(typeof(TextBoxBase));

        var setters = new Setter[]
        {
            new(TextBoxBase.AcceptsReturnProperty, allowMultiline),
            new(TextBoxBase.PaddingProperty, new Thickness(1, 1, 0, 0)),
            new(TextBoxBase.MarginProperty, new Thickness(-2))
        };

        foreach (var setter in setters)
        {
            style.Setters.Add(setter);
        }
        return style;
    }

    internal static Style GetEditingElementStyle(bool allowMultiline)
        => editingElementStyle ??= SetEditingElementStyle(allowMultiline);
}

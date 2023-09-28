using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RowsSharp.View;

internal class StringToBooleanConverter : IValueConverter
{
    /// <param name="value">string, to be converted to a bool</param>
    /// <param name="parameter">ValueTuple[string, string], first item representing a true value, second item representing a false value</param>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = (string)value;
        var trueFalse = (ValueTuple<string, string>)parameter;

        if (text == trueFalse.Item1)
        {
            return true;
        }

        if (text == trueFalse.Item2)
        {
            return false;
        }

        return DependencyProperty.UnsetValue;
    }

    /// <param name="value">bool, to be converted to a string</param>
    /// <param name="parameter">ValueTuple[string, string], first item representing a true value, second item representing a false value</param>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolean = (bool)value;
        var trueFalse = (ValueTuple<string, string>)parameter;

        return boolean ? trueFalse.Item1 : trueFalse.Item2;
    }
}

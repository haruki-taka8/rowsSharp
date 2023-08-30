using System;
using System.Globalization;
using System.Windows.Data;

namespace RowsSharp.View;

/// <summary>
/// Convert a value from milliseconds to seconds, and round the result.
/// </summary>

public class MsecToSecConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Convert.ToInt32(value) / 1000;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

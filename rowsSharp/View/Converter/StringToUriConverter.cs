using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace RowsSharp.View;

internal class StringToUriConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return DependencyProperty.UnsetValue;
        }
        
        if (value is not string text)
        {
            throw new ArgumentException(null, nameof(value));
        }
        
        bool isValidUri = Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out Uri? uri);
        
        return isValidUri ? uri! : text;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string or Uri)
        {
            throw new ArgumentException(null, nameof(value));
        }

        return value;
    }
}

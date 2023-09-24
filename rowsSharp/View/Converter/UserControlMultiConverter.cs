using RowsSharp.ViewModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace RowsSharp.View;

public class UserControlMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2
            || values[0] is not Section section
            || values[1] is not CommonViewModel commonViewModel
        )
        {
            throw new ArgumentException(null, nameof(values));
        }

        return section switch
        {
            Section.Splash => new Splash(commonViewModel),
            Section.Editor => new Editor(commonViewModel),
            Section.Welcome => new Welcome(commonViewModel),
            Section.Settings => new Settings(commonViewModel),

            _ => throw new InvalidEnumArgumentException(nameof(section))
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

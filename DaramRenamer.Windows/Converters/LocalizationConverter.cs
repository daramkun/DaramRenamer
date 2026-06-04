using System;
using System.Globalization;
using System.Windows.Data;
using DaramRenamer.Registry;

namespace DaramRenamer.Converters;

internal class LocalizationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var descriptor = value == null ? null : DaramRenamerRegistry.GetDescriptor(value);
        return descriptor == null ? value?.GetType().Name : Strings.Instance[descriptor.LocalizationKey];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

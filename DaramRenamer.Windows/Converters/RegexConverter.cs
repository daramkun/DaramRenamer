using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DaramRenamer.Converters
{
	class RegexConverter : IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value as Regex)?.ToString ();
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Regex (value as string);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Daramkun.DaramRenamer.Converters
{
	class RegexConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ( value as Regex )?.ToString ();
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return new Regex ( value as string );
		}
	}
}

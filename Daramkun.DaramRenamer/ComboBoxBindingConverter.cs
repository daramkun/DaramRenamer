using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Daramkun.DaramRenamer
{
	public class ComboBoxBindingConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string [] arr = ( value as string ).Split ( ';' );
			for ( int i = 0; i < arr.Length; ++i )
				arr [ i ] = Encoding.UTF8.GetString ( System.Convert.FromBase64String ( arr [ i ] ) );
			return new ObservableCollection<string> ( arr );
		}

		public object ConvertBack ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string [] arr = ( value as ObservableCollection<string> ).ToArray ();
			for ( int i = 0; i < arr.Length; ++i )
				arr [ i ] = System.Convert.ToBase64String ( Encoding.UTF8.GetBytes ( arr [ i ] ) );
			return string.Join ( ";", arr );
		}
	}
}

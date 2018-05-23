using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Daramkun.DaramRenamer.Converters
{
	public class PathToIconConverter : IValueConverter
	{
		static Dictionary<string, BitmapSource> cached = new Dictionary<string, BitmapSource> ();

		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			var filename = value as string;
			if ( !File.GetAttributes ( filename ).HasFlag ( FileAttributes.Directory ) )
			{
				var ext = Path.GetExtension ( filename );
				if ( cached.ContainsKey ( ext ) )
					return cached [ ext ];
				using ( System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon ( filename ) )
				{
					var icon = Imaging.CreateBitmapSourceFromHIcon ( sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight ( 16, 16 ) );
					if ( icon.CanFreeze )
						icon.Freeze ();
					return icon;
				}
			}

			return null;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}

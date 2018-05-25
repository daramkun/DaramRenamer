using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
			string key = null;
			bool doNotCache = false;
			if ( File.GetAttributes ( filename ).HasFlag ( FileAttributes.Directory ) )
			{
				if ( cached.ContainsKey ( "?" ) )
					return cached [ "?" ];
				key = "?";
			}
			else
			{
				var ext = Path.GetExtension ( filename );

				if ( ext == ".exe" || ext == ".ico" )
				{
					doNotCache = true;
				}
				else
				{
					if ( cached.ContainsKey ( ext ) )
						return cached [ ext ];
					key = ext;
				}
			}
			IntPtr hIcon = File.GetAttributes ( filename ).HasFlag ( FileAttributes.Directory )
				? GetFolderIcon ( filename )
				: GetFileIcon ( filename );
			var icon = Imaging.CreateBitmapSourceFromHIcon ( hIcon, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight ( 16, 16 ) );
			DestroyIcon ( hIcon );
			if ( icon.CanFreeze )
				icon.Freeze ();

			if ( !doNotCache )
				cached.Add ( key, icon );

			return icon;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ();
		}

		[StructLayout ( LayoutKind.Sequential )]
		struct SHFILEINFO
		{
			public const int NAMESIZE = 80;
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs ( UnmanagedType.ByValTStr, SizeConst = 260 )]
			public string szDisplayName;
			[MarshalAs ( UnmanagedType.ByValTStr, SizeConst = 80 )]
			public string szTypeName;
		};

		[DllImport ( "Shell32.dll" )]
		static extern IntPtr SHGetFileInfo ( string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags );

		const uint SHGFI_ICON = 0x000000100;
		const uint SHGFI_SMALLICON = 0x000000001;
		const uint SHGFI_OPENICON = 0x000000002;
		const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

		const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
		const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

		[DllImport ( "User32.dll" )]
		static extern int DestroyIcon ( IntPtr hIcon );

		static IntPtr GetFileIcon ( string name )
		{
			SHGetFileInfo ( name, FILE_ATTRIBUTE_NORMAL, out SHFILEINFO shfi,
				( uint ) Marshal.SizeOf ( typeof ( SHFILEINFO ) ),
				SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON );
			
			return shfi.hIcon;
		}
		
		static IntPtr GetFolderIcon ( string name )
		{
			SHGetFileInfo ( name, FILE_ATTRIBUTE_DIRECTORY, out SHFILEINFO shfi,
				( uint ) Marshal.SizeOf ( typeof ( SHFILEINFO ) ),
				SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_OPENICON | SHGFI_SMALLICON );
			
			return shfi.hIcon;
		}
	}
}

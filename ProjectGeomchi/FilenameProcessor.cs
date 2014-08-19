using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GroupRenamer
{
	public static class FilenameProcessor
	{
		#region Utility
		private static string GetFilenameWithoutExtension ( string filename )
		{
			int lastDot = filename.LastIndexOf ( '.' );
			return filename.Substring ( 0, lastDot );
		}

		private static string GetExtensionWithoutFilename ( string filename )
		{
			int lastDot = filename.LastIndexOf ( '.' );
			return filename.Substring ( lastDot, filename.Length - lastDot );
		}

		private static string GetReverseString ( string str )
		{
			StringBuilder sb = new StringBuilder ();
			foreach ( char ch in str.Reverse () )
				sb.Append ( ch );
			return sb.ToString ();
		}
		#endregion

		public static string Replace ( string filename, string original, string replace, bool isExcludeExtension = false )
		{
			return ( isExcludeExtension ) ? (
					   GetFilenameWithoutExtension ( filename ).Replace ( original, replace )
						   + GetExtensionWithoutFilename ( filename ) )
						   : filename.Replace ( original, replace );
		}

		public static string Prestring ( string filename, string adder )
		{
			return string.Format ( "{0}{1}{2}", adder, GetFilenameWithoutExtension ( filename ),
				GetExtensionWithoutFilename ( filename ) );
		}

		public static string Poststring ( string filename, string adder )
		{
			return string.Format ( "{1}{0}{2}", adder, GetFilenameWithoutExtension ( filename ),
				GetExtensionWithoutFilename ( filename ) );
		}

		public static string DeleteName ( string filename )
		{
			return GetExtensionWithoutFilename ( filename );
		}

		public static string DeleteEnclosed ( string filename, string pre, string post, bool isDeleteAllEnclosed )
		{
			int first, last;
			while ( ( first = filename.IndexOf ( pre ) ) != -1 )
			{
				last = filename.IndexOf ( post, first + 1 );
				if ( last == -1 ) break;
				filename = filename.Remove ( first, last - first + post.Length );
				if ( !isDeleteAllEnclosed ) break;
			}
			return filename;
		}

		public static string DeleteWithoutNumber ( string filename )
		{
			StringBuilder sb = new StringBuilder ();
			foreach ( char ch in GetFilenameWithoutExtension ( filename ) )
				if ( ch >= '0' && ch <= '9' )
					sb.Append ( ch );
			return sb.ToString () + GetExtensionWithoutFilename ( filename );
		}

		public static string SameNumberOfDigit ( string filename, int digitCount, bool isOffsetFromBack )
		{
			string fn = GetFilenameWithoutExtension ( filename );

			bool meetTheNumber = false;
			int offset = 0, count = 0, size = 0;
			foreach ( char ch in !isOffsetFromBack ? fn : fn.Reverse () )
			{
				if ( ( ch >= '0' && ch <= '9' ) )
				{
					if ( !meetTheNumber )
					{
						offset = count;
						meetTheNumber = true;
					}
					++size;
				}
				else
				{
					if ( meetTheNumber )
					{
						if ( isOffsetFromBack )
							offset = fn.Length - ( offset + size );
						break;
					}
				}
				++count;
			}

			if ( !meetTheNumber || size >= digitCount ) return filename;

			StringBuilder sb = new StringBuilder ();
			sb.Append ( fn );
			size = digitCount - size;
			while ( size > 0 )
			{
				sb.Insert ( offset, '0' );
				--size;
			}
			sb.Append ( GetExtensionWithoutFilename ( filename ) );

			return sb.ToString ();
		}

		public static string AddNumber ( string filename, int a )
		{
			return GetFilenameWithoutExtension ( filename ) + a + GetExtensionWithoutFilename ( filename );
		}

		public static string NumberIncrese ( string filename, int increaseValue, bool isOffsetFromBack )
		{
			string fn = GetFilenameWithoutExtension ( filename );

			bool meetTheNumber = false;
			int offset = 0, count = 0, size = 0;
			foreach ( char ch in !isOffsetFromBack ? fn : fn.Reverse () )
			{
				if ( ( ch >= '0' && ch <= '9' ) )
				{
					if ( !meetTheNumber )
					{
						offset = count;
						meetTheNumber = true;
					}
					++size;
				}
				else
				{
					if ( meetTheNumber )
					{
						if ( isOffsetFromBack )
							offset = fn.Length - ( offset + size );
						break;
					}
				}
				++count;
			}

			if ( !meetTheNumber ) return filename;

			string origin = fn.Substring ( offset, size );
			int number = int.Parse ( origin ) + increaseValue;

			StringBuilder sb = new StringBuilder ();
			sb.Append ( number );
			int nsize = origin.Length - origin.Length;
			while ( nsize > 0 )
			{
				sb.Insert ( offset, '0' );
				--nsize;
			}
			fn = fn.Remove ( offset, size ).Insert ( offset, sb.ToString () );

			return fn + GetExtensionWithoutFilename ( filename );
		}

		public static string RemoveExtension ( string filename )
		{
			return GetFilenameWithoutExtension ( filename );
		}

		public static string AddExtension ( string filename, string ext )
		{
			return string.Format ( "{0}.{1}", filename, ext );
		}

		public static string ChangeExtension ( string filename, string ext )
		{
			string originalExt =  GetExtensionWithoutFilename ( filename );
			return string.Format ( "{0}.{1}", filename.Substring ( 0, filename.LastIndexOf ( '.' ) ), ext );
		}

		public static string ExtensionToUpper ( string filename )
		{
			return string.Format ( "{0}{1}", GetFilenameWithoutExtension ( filename ),
					GetExtensionWithoutFilename ( filename ).ToUpper () );
		}

		public static string ExtensionToLower ( string filename )
		{
			return string.Format ( "{0}{1}", GetFilenameWithoutExtension ( filename ),
					GetExtensionWithoutFilename ( filename ).ToLower () );
		}

		public static string RegularExpression ( string filename, Regex exp, string formatString )
		{
			try
			{
				Match match = exp.Match ( filename );
				GroupCollection group = match.Groups;
				object [] groupArr = new object [ group.Count ];
				for ( int i = 0; i < groupArr.Length; i++ )
					groupArr [ i ] = group [ i ].Value.Trim ();
				return string.Format ( formatString, groupArr );
			}
			catch { return filename; }
		}
	}
}

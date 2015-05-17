using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public static class FilenameProcessor
	{
		public static void Replace ( FileInfo fileInfo, string original, string replace, bool isExcludeExtension = false )
		{
			if ( original.Length == 0 ) return;

			fileInfo.ChangedName = ( isExcludeExtension ) ?
				( Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).Replace ( original, replace ) + Path.GetExtension ( fileInfo.ChangedName ) ) :
				fileInfo.ChangedName.Replace ( original, replace );
		}

		public static void Prestring ( FileInfo fileInfo, string adder )
		{
			if ( adder.Length == 0 ) return;
			fileInfo.ChangedName = string.Format ( "{0}{1}{2}", adder, Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ),
				Path.GetExtension ( fileInfo.ChangedName ) );
		}

		public static void Poststring ( FileInfo fileInfo, string adder )
		{
			if ( adder.Length == 0 ) return;
			fileInfo.ChangedName = string.Format ( "{1}{0}{2}", adder, Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ),
				Path.GetExtension ( fileInfo.ChangedName ) );
		}

		public static void Trimming ( FileInfo fileInfo, bool includeExtension = true, bool? lastCharactersTrimming = null )
		{
			string ext = Path.GetExtension ( fileInfo.ChangedName );
			if ( includeExtension )
				ext = "." + ext.Substring ( 1, ext.Length - 1 ).Trim ();
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName );
			if ( lastCharactersTrimming == null )
				fileInfo.ChangedName = fileInfo.ChangedName.Trim () + ext;
			else
			{
				if ( lastCharactersTrimming.Value )
					fileInfo.ChangedName = fileInfo.ChangedName.TrimEnd () + ext;
				else
					fileInfo.ChangedName = fileInfo.ChangedName.TrimStart () + ext;
			}
		}

		public static void DeleteEnclosed ( FileInfo fileInfo, string pre, string post, bool isDeleteAllEnclosed )
		{
			if ( pre.Length == 0 || post.Length == 0 ) return;

			int first, last;
			while ( ( first = fileInfo.ChangedName.IndexOf ( pre ) ) != -1 )
			{
				last = fileInfo.ChangedName.IndexOf ( post, first + 1 );
				if ( last == -1 ) break;
				fileInfo.ChangedName = fileInfo.ChangedName.Remove ( first, last - first + post.Length );
				if ( !isDeleteAllEnclosed ) break;
			}
		}

		public static void DeleteName ( FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			fileInfo.ChangedName = Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void NameToUpper ( FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).ToUpper () +
				Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void NameToLower ( FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).ToLower () +
				Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void NameToUpperFirstLetterOnly ( FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			string [] split = fileInfo.ChangedName.Split ( ' ' );
			for ( int i = 0; i < split.Length; ++i )
			{
				char [] arr = split [ i ].ToArray ();
				arr [ 0 ] = char.ToUpper ( arr [ 0 ] );
				split [ i ] = new string ( arr );
			}
			fileInfo.ChangedName = string.Join ( " ", split );
		}

		public static void DeleteWithoutNumber ( FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			StringBuilder sb = new StringBuilder ();
			foreach ( char ch in Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) )
				if ( ch >= '0' && ch <= '9' )
					sb.Append ( ch );
			fileInfo.ChangedName = sb.ToString () + Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void DeleteWithoutNumberWordly ( FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			List<string> split = new List<string> ( Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).Split ( ' ' ) );
			for ( int i = 0; i < split.Count; ++i )
				for ( int j = 0; j < split[i].Length;++i )
					if ( !( split [ i ] [ j ] >= '0' && split [ i ] [ j ] <= '9' ) )
					{
						split.RemoveAt ( i );
						--i;
					}
			fileInfo.ChangedName = string.Join ( " ", split.ToArray () ) + Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void SameNumberOfDigit ( FileInfo fileInfo, int digitCount, bool isOffsetFromBack )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			string fn = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName );

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

			if ( !meetTheNumber || size >= digitCount ) return;

			StringBuilder sb = new StringBuilder ();
			sb.Append ( fn );
			size = digitCount - size;
			while ( size > 0 )
			{
				sb.Insert ( offset, '0' );
				--size;
			}
			sb.Append ( Path.GetExtension ( fileInfo.ChangedName ) );

			fileInfo.ChangedName = sb.ToString ();
		}

		public static void AddNumber ( FileInfo fileInfo, int a, bool isOffsetFromBack )
		{
			fileInfo.ChangedName = string.Format ( isOffsetFromBack ? "{0}{1}{2}" : "{1}{0}{2}",
				Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ), a, Path.GetExtension ( fileInfo.ChangedName ) );
		}

		public static void NumberIncrese ( FileInfo fileInfo, int increaseValue, bool isOffsetFromBack )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			string fn = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName );

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

			if ( !meetTheNumber ) return;

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

			fileInfo.ChangedName = fn + Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void RemoveExtension ( FileInfo fileInfo )
		{
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName );
		}

		public static void AddExtension ( FileInfo fileInfo, string ext )
		{
			fileInfo.ChangedName = string.Format ( "{0}.{1}", fileInfo.ChangedName, ext );
		}

		public static void ChangeExtension ( FileInfo fileInfo, string ext )
		{
			string originalExt = Path.GetExtension ( fileInfo.ChangedName );
			fileInfo.ChangedName = string.Format ( "{0}.{1}", fileInfo.ChangedName.Substring ( 0, fileInfo.ChangedName.LastIndexOf ( '.' ) ), ext );
		}

		public static void ExtensionToUpper ( FileInfo fileInfo )
		{
			fileInfo.ChangedName = string.Format ( "{0}{1}", Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ),
					Path.GetExtension ( fileInfo.ChangedName ).ToUpper () );
		}

		public static void ExtensionToLower ( FileInfo fileInfo )
		{
			fileInfo.ChangedName = string.Format ( "{0}{1}", Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ),
					Path.GetExtension ( fileInfo.ChangedName ).ToLower () );
		}

		public static void ChangePath ( FileInfo fileInfo, string path )
		{
			fileInfo.ChangedPath = path;
		}

		public static void RegularExpression ( FileInfo fileInfo, Regex exp, string formatString )
		{
			try
			{
				string ext = Path.GetExtension ( fileInfo.ChangedName );
				Match match = exp.Match ( Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) );
				GroupCollection group = match.Groups;
				object [] groupArr = new object [ group.Count ];
				for ( int i = 0; i < groupArr.Length; i++ )
					groupArr [ i ] = group [ i ].Value.Trim ();
				fileInfo.ChangedName = string.Format ( formatString, groupArr ) + ext;
			}
			catch { }
		}

		public static void AddCreationDate ( FileInfo fileInfo, bool lastLocationAdd, string formatString = "yyyyMMdd" )
		{
			DateTime dateTime = File.GetCreationTime ( fileInfo.OriginalFullName );
			if ( !lastLocationAdd ) fileInfo.ChangedName = dateTime.ToString ( formatString ) + fileInfo.ChangedName;
			else fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) + dateTime.ToString ( formatString ) +
				Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void AddLastWriteDate ( FileInfo fileInfo, bool lastLocationAdd, string formatString = "yyyyMMdd" )
		{
			DateTime dateTime = File.GetLastWriteTime ( fileInfo.OriginalFullName );
			if ( !lastLocationAdd ) fileInfo.ChangedName = dateTime.ToString ( formatString ) + fileInfo.ChangedName;
			else fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) + dateTime.ToString ( formatString ) +
				Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void AddLastAccessDate ( FileInfo fileInfo, bool lastLocationAdd, string formatString = "yyyyMMdd" )
		{
			DateTime dateTime = File.GetLastAccessTime ( fileInfo.OriginalFullName );
			if ( !lastLocationAdd ) fileInfo.ChangedName = dateTime.ToString ( formatString ) + fileInfo.ChangedName;
			else fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) + dateTime.ToString ( formatString ) + 
				Path.GetExtension ( fileInfo.ChangedName );
		}
	}
}

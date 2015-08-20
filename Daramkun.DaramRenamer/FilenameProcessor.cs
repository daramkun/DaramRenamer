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
		public static void Replace ( this FileInfo fileInfo, string original, string replace, bool isExcludeExtension = false )
		{
			if ( original.Length == 0 ) return;

			fileInfo.ChangedName = ( isExcludeExtension ) ?
				( Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).Replace ( original, replace ) + Path.GetExtension ( fileInfo.ChangedName ) ) :
				fileInfo.ChangedName.Replace ( original, replace );
		}

		public static void Concat ( this FileInfo fileInfo, string adder, bool isPrestring = true )
		{
			if ( adder.Length == 0 ) return;
			fileInfo.ChangedName = string.Format ( isPrestring ? "{0}{1}{2}" : "{1}{0}{2}", adder, Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ),
				Path.GetExtension ( fileInfo.ChangedName ) );
		}

		public static void Trimming ( this FileInfo fileInfo, bool includeExtension = true, bool? lastCharactersTrimming = null )
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

		public static void DeleteEnclosed ( this FileInfo fileInfo, string pre, string post, bool isDeleteAllEnclosed = false )
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

		public static void DeleteName ( this FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			fileInfo.ChangedName = Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void NameToUpper ( this FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).ToUpper () +
				Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void NameToLower ( this FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ).ToLower () +
				Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void NameToUpperFirstLetterOnly ( this FileInfo fileInfo )
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

		public static void SubstringName ( this FileInfo fileInfo, int startIndex, int length )
		{
			if ( length <= 0 )
				fileInfo.ChangedName = fileInfo.ChangedName.Substring ( startIndex );
			else
				fileInfo.ChangedName = fileInfo.ChangedName.Substring ( startIndex, length );
		}

		public static void DeleteWithoutNumber ( this FileInfo fileInfo )
		{
			if ( fileInfo.ChangedName.Length == 0 ) return;
			StringBuilder sb = new StringBuilder ();
			foreach ( char ch in Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) )
				if ( ch >= '0' && ch <= '9' )
					sb.Append ( ch );
			fileInfo.ChangedName = sb.ToString () + Path.GetExtension ( fileInfo.ChangedName );
		}

		public static void DeleteWithoutNumberWordly ( this FileInfo fileInfo )
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

		public static void SameNumberOfDigit ( this FileInfo fileInfo, int digitCount, bool isOffsetFromBack = true )
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

		public static void AddNumber ( this FileInfo fileInfo, int a, bool isOffsetFromBack = true )
		{
			fileInfo.ChangedName = string.Format ( isOffsetFromBack ? "{0}{1}{2}" : "{1}{0}{2}",
				Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ), a, Path.GetExtension ( fileInfo.ChangedName ) );
		}

		public static void NumberIncrese ( this FileInfo fileInfo, int increaseValue, bool isOffsetFromBack = true )
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

		public static void AddExtension ( this FileInfo fileInfo, string ext )
		{
			fileInfo.ChangedName = $"{fileInfo.ChangedName}.{ext}";
		}

		public static void DeleteExtension ( this FileInfo fileInfo )
		{
			fileInfo.ChangedName = Path.GetFileNameWithoutExtension ( fileInfo.ChangedName );
		}

		public static void ChangeExtension ( this FileInfo fileInfo, string ext )
		{
			string originalExt = Path.GetExtension ( fileInfo.ChangedName );
			fileInfo.ChangedName = $"{fileInfo.ChangedName.Substring ( 0, fileInfo.ChangedName.LastIndexOf ( '.' ) )}.{ext}";
		}

		public static void ExtensionToUpper ( this FileInfo fileInfo )
		{
			fileInfo.ChangedName = $"{Path.GetFileNameWithoutExtension ( fileInfo.ChangedName )}{Path.GetExtension ( fileInfo.ChangedName ).ToUpper ()}";
		}

		public static void ExtensionToLower ( this FileInfo fileInfo )
		{
			fileInfo.ChangedName = $"{Path.GetFileNameWithoutExtension ( fileInfo.ChangedName )}{Path.GetExtension ( fileInfo.ChangedName ).ToLower ()}";
		}

		public static void AddCreationDate ( this FileInfo fileInfo, bool lastLocationAdd = true, string formatString = "yyyyMMdd" )
		{
			var dateTime = File.GetCreationTime ( fileInfo.OriginalFullName ).ToString ( formatString );
			if ( !lastLocationAdd ) fileInfo.ChangedName = $"{dateTime}{fileInfo.ChangedName}";
			else fileInfo.ChangedName = $"{Path.GetFileNameWithoutExtension ( fileInfo.ChangedName )}{dateTime}{Path.GetExtension ( fileInfo.ChangedName )}";
		}

		public static void AddLastWriteDate ( this FileInfo fileInfo, bool lastLocationAdd = true, string formatString = "yyyyMMdd" )
		{
			var dateTime = File.GetLastWriteTime ( fileInfo.OriginalFullName ).ToString ( formatString );
			if ( !lastLocationAdd ) fileInfo.ChangedName = $"{dateTime}{fileInfo.ChangedName}";
			else fileInfo.ChangedName = $"{Path.GetFileNameWithoutExtension ( fileInfo.ChangedName )}{dateTime}{Path.GetExtension ( fileInfo.ChangedName )}";
		}

		public static void AddLastAccessDate ( this FileInfo fileInfo, bool lastLocationAdd = true, string formatString = "yyyyMMdd" )
		{
			var dateTime = File.GetLastAccessTime ( fileInfo.OriginalFullName ).ToString ( formatString );
			if ( !lastLocationAdd ) fileInfo.ChangedName = $"{dateTime}{fileInfo.ChangedName}";
			else fileInfo.ChangedName = $"{Path.GetFileNameWithoutExtension ( fileInfo.ChangedName )}{dateTime}{Path.GetExtension ( fileInfo.ChangedName )}";
		}

		public static void ChangePath ( this FileInfo fileInfo, string path )
		{
			fileInfo.ChangedPath = path;
		}

		public static void RegularExpression ( this FileInfo fileInfo, Regex exp, string formatString )
		{
			try
			{
				string ext = Path.GetExtension ( fileInfo.ChangedName );
				Match match = exp.Match ( Path.GetFileNameWithoutExtension ( fileInfo.ChangedName ) );
				GroupCollection group = match.Groups;
				object [] groupArr = new object [ group.Count ];
				for ( int i = 0; i < groupArr.Length; i++ )
					groupArr [ i ] = group [ i ].Value.Trim ();
				fileInfo.ChangedName = $"{string.Format ( formatString, groupArr )}{ext}";
			}
			catch { }
		}

		public static void BatchScript ( this FileInfo fileInfo, string script, string condition = null )
		{
			if ( script == null || script.Trim () == "" ) return;
			if ( condition != null && condition != ".*" )
			{
				bool found = false;
				string ext = System.IO.Path.GetExtension ( fileInfo.OriginalFullName );
				foreach ( var filter in condition.Split ( '|' ) )
				{
					if ( filter == ext )
					{
						found = true;
						break;
					}
				}

				if ( !found ) return;
			}
			fileInfo.BatchProcess ( script );
		}
	}
}

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
		/// <summary>
		/// 문자열 교체
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <param name="original">원본 문자열</param>
		/// <param name="replace">바꿀 문자열</param>
		/// <param name="isExcludeExtension">확장자를 포함하여 교체하는가</param>
		/// <returns>문자열 교체된 파일명</returns>
		public static string Replace ( string filename, string original, string replace, bool isExcludeExtension = false )
		{
			if ( original.Length == 0 ) return filename;
			return ( isExcludeExtension ) ?
				// 확장자 미포함
				( Path.GetFileNameWithoutExtension ( filename ).Replace ( original, replace ) + Path.GetExtension ( filename ) ) :
				// 확장자 포함
				filename.Replace ( original, replace );
		}

		/// <summary>
		/// 맨 앞에 문자열 추가
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <param name="adder">추가할 문자열</param>
		/// <returns>문자열 추가된 파일명</returns>
		public static string Prestring ( string filename, string adder )
		{
			if ( adder.Length == 0 ) return filename;
			return string.Format ( "{0}{1}{2}", adder, Path.GetFileNameWithoutExtension ( filename ),
				Path.GetExtension ( filename ) );
		}

		/// <summary>
		/// 맨 뒤에 문자열 추가
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <param name="adder">추가할 문자열</param>
		/// <returns>문자열 추가된 파일명</returns>
		public static string Poststring ( string filename, string adder )
		{
			if ( adder.Length == 0 ) return filename;
			return string.Format ( "{1}{0}{2}", adder, Path.GetFileNameWithoutExtension ( filename ),
				Path.GetExtension ( filename ) );
		}

		/// <summary>
		/// 트리밍
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <param name="includeExtension">확장자도 포함하는가 (포함하면 확장자는 무조건 양 옆 모두 트리밍)</param>
		/// <param name="lastCharactersTrimming">맨 뒤만 트리밍 하는가 (null이면 모두 트리밍)</param>
		/// <returns>트리밍 된 파일명</returns>
		public static string Trimming ( string filename, bool includeExtension = true, bool? lastCharactersTrimming = null )
		{
			string ext = Path.GetExtension ( filename );
			if ( includeExtension )
				ext = "." + ext.Substring ( 1, ext.Length - 1 ).Trim ();
			filename = Path.GetFileNameWithoutExtension ( filename );
			if ( lastCharactersTrimming == null ) return filename.Trim () + ext;
			else
			{
				if ( lastCharactersTrimming.Value )
					return filename.TrimEnd () + ext;
				else return filename.TrimStart () + ext;
			}
		}

		/// <summary>
		/// 묶인 문자열 제거
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <param name="pre">묶인 문자열 앞 문자열</param>
		/// <param name="post">묶인 문자열 뒤 문자열</param>
		/// <param name="isDeleteAllEnclosed">모든 묶인 문자열 제거</param>
		/// <returns>묶인 문자열 제거된 파일명</returns>
		public static string DeleteEnclosed ( string filename, string pre, string post, bool isDeleteAllEnclosed )
		{
			if ( pre.Length == 0 || post.Length == 0 ) return filename;

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

		/// <summary>
		/// 파일 이름 제거
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <returns>확장자만 남은 파일명</returns>
		public static string DeleteName ( string filename )
		{
			if ( filename.Length == 0 ) return filename;
			return Path.GetExtension ( filename );
		}

		public static string NameToUpper ( string filename )
		{
			if ( filename.Length == 0 ) return filename;
			return Path.GetFileNameWithoutExtension ( filename ).ToUpper () + Path.GetExtension ( filename );
		}

		public static string NameToLower ( string filename )
		{
			if ( filename.Length == 0 ) return filename;
			return Path.GetFileNameWithoutExtension ( filename ).ToLower () + Path.GetExtension ( filename );
		}

		public static string NameToUpperFirstLetterOnly ( string filename )
		{
			if ( filename.Length == 0 ) return filename;
			string [] split = filename.Split ( ' ' );
			for ( int i = 0; i < split.Length; ++i )
			{
				char [] arr = split [ i ].ToArray ();
				arr [ 0 ] = char.ToUpper ( arr [ 0 ] );
				split [ i ] = new string ( arr );
			}
			return string.Join ( " ", split );
		}

		/// <summary>
		/// 숫자를 제외하고 모두 제거
		/// </summary>
		/// <param name="filename">파일명</param>
		/// <returns>숫자만 남은 파일명</returns>
		public static string DeleteWithoutNumber ( string filename )
		{
			if ( filename.Length == 0 ) return filename;
			StringBuilder sb = new StringBuilder ();
			foreach ( char ch in Path.GetFileNameWithoutExtension ( filename ) )
				if ( ch >= '0' && ch <= '9' )
					sb.Append ( ch );
			return sb.ToString () + Path.GetExtension ( filename );
		}

		public static string DeleteWithoutNumberWordly ( string filename )
		{
			if ( filename.Length == 0 ) return filename;
			List<string> split = new List<string> ( Path.GetFileNameWithoutExtension ( filename ).Split(' ') );
			for ( int i = 0; i < split.Count; ++i )
				for ( int j = 0; j < split[i].Length;++i )
					if ( !( split [ i ] [ j ] >= '0' && split [ i ] [ j ] <= '9' ) )
					{
						split.RemoveAt ( i );
						--i;
					}
			return string.Join ( " ", split.ToArray () ) + Path.GetExtension ( filename );
		}

		public static string SameNumberOfDigit ( string filename, int digitCount, bool isOffsetFromBack )
		{
			string fn = Path.GetFileNameWithoutExtension ( filename );

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
			sb.Append ( Path.GetExtension ( filename ) );

			return sb.ToString ();
		}

		public static string AddNumber ( string filename, int a, bool isOffsetFromBack )
		{
			return string.Format ( isOffsetFromBack ? "{0}{1}{2}" : "{1}{0}{2}",
				Path.GetFileNameWithoutExtension ( filename ), a, Path.GetExtension ( filename ) );
		}

		public static string NumberIncrese ( string filename, int increaseValue, bool isOffsetFromBack )
		{
			string fn = Path.GetFileNameWithoutExtension ( filename );

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

			return fn + Path.GetExtension ( filename );
		}

		public static string RemoveExtension ( string filename )
		{
			return Path.GetFileNameWithoutExtension ( filename );
		}

		public static string AddExtension ( string filename, string ext )
		{
			return string.Format ( "{0}.{1}", filename, ext );
		}

		public static string ChangeExtension ( string filename, string ext )
		{
			string originalExt = Path.GetExtension ( filename );
			return string.Format ( "{0}.{1}", filename.Substring ( 0, filename.LastIndexOf ( '.' ) ), ext );
		}

		public static string ExtensionToUpper ( string filename )
		{
			return string.Format ( "{0}{1}", Path.GetFileNameWithoutExtension ( filename ),
					Path.GetExtension ( filename ).ToUpper () );
		}

		public static string ExtensionToLower ( string filename )
		{
			return string.Format ( "{0}{1}", Path.GetFileNameWithoutExtension ( filename ),
					Path.GetExtension ( filename ).ToLower () );
		}

		public static string RegularExpression ( string filename, Regex exp, string formatString )
		{
			try
			{
				string ext = Path.GetExtension ( filename );
				Match match = exp.Match ( Path.GetFileNameWithoutExtension ( filename ) );
				GroupCollection group = match.Groups;
				object [] groupArr = new object [ group.Count ];
				for ( int i = 0; i < groupArr.Length; i++ )
					groupArr [ i ] = group [ i ].Value.Trim ();
				return string.Format ( formatString, groupArr ) + ext;
			}
			catch { return filename; }
		}

		public static string AddCreationDate ( string original, string filename, bool lastLocationAdd, string formatString = "yyyyMMdd" )
		{
			DateTime dateTime = File.GetCreationTime ( original );
			if ( !lastLocationAdd ) return dateTime.ToString ( formatString ) + filename;
			else return Path.GetFileNameWithoutExtension ( filename ) + dateTime.ToString ( formatString ) + Path.GetExtension ( filename );
		}

		public static string AddLastWriteDate ( string original, string filename, bool lastLocationAdd, string formatString = "yyyyMMdd" )
		{
			DateTime dateTime = File.GetLastWriteTime ( original );
			if ( !lastLocationAdd ) return dateTime.ToString ( formatString ) + filename;
			else return Path.GetFileNameWithoutExtension ( filename ) + dateTime.ToString ( formatString ) + Path.GetExtension ( filename );
		}

		public static string AddLastAccessDate ( string original, string filename, bool lastLocationAdd, string formatString = "yyyyMMdd" )
		{
			DateTime dateTime = File.GetLastAccessTime ( original );
			if ( !lastLocationAdd ) return dateTime.ToString ( formatString ) + filename;
			else return Path.GetFileNameWithoutExtension ( filename ) + dateTime.ToString ( formatString ) + Path.GetExtension ( filename );
		}
	}
}

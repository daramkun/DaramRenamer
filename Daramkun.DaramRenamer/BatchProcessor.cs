using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public static class BatchProcessor
	{
		private static string [] NewLineSplitter = new [] { "\n" };

		public static void BatchProcess ( this FileInfo fileInfo, string batchScript )
		{
			string [] lines = batchScript.Split ( NewLineSplitter, StringSplitOptions.RemoveEmptyEntries );

			List<string> arguments = new List<string> ();
			StringBuilder builder = new StringBuilder ();

			foreach ( string line in lines )
			{
				string cur = line.Trim ();
				arguments.Clear ();
				builder.Clear ();
				
				// Functional Mode
				if ( cur [ 0 ] == '!' )
				{
					string function = cur.Substring ( 1, cur.IndexOf ( ':' ) - 1 );
					bool started = false;
					bool formatstarted = false;
					bool backslashed = false;
					for ( int i = function.Length + 2; i < cur.Length; ++i )
					{
						char ch = cur [ i ];
						if ( started )
						{
							if ( ch == '\\' && backslashed == false ) backslashed = true;
							else if ( ch == '\\' && backslashed == true ) { backslashed = false; builder.Append ( '\\' ); }
							else if ( ch == 't' && backslashed == true ) { backslashed = false; builder.Append ( '\t' ); }
							else if ( ch == '"' && backslashed == true ) { backslashed = false; builder.Append ( '"' ); }
							else if ( ch == '"' && backslashed == false )
							{
								backslashed = false;
								started = false;
								formatstarted = false;
								arguments.Add ( builder.ToString () );
								builder.Clear ();
							}
							else if ( backslashed == false )
								builder.Append ( ch );
							else throw new Exception ( "Argument error." );
						}
						else if ( formatstarted && started == false )
						{
							if ( ch == '}' )
							{
								backslashed = false;
								started = false;
								formatstarted = false;
								arguments.Add ( ConvertFormatString ( fileInfo, builder.ToString () ) );
								builder.Clear ();
							}
							else builder.Append ( ch );
						}
						else
						{
							if ( ch == '"' ) started = true;
							else if ( ch == '{' ) formatstarted = true;
							else if ( ch == ' ' || ch == '\t' || ch == '\r' ) continue;
							else throw new Exception ( "Script error." );
						}
					}

					ProcessFunction ( fileInfo, function, arguments.ToArray () );
				}
				// Format Mode
				else
				{
					
				}
			}
		}

		public static string ConvertFormatString ( FileInfo fileInfo, string formatString )
		{
			string argument = null;
			if ( formatString.IndexOf ( ':' ) > 0 )
			{
				argument = formatString.Substring ( formatString.IndexOf ( ':' ) + 1 );
				formatString = formatString.Substring ( 0, formatString.IndexOf ( ':' ) - 1 );
			}

			switch ( formatString )
			{
				case "filecreation":

					break;
			}
			return null;
		}

		public static void ProcessFunction ( FileInfo fileInfo, string function, string [] arguments )
		{
			switch ( function )
			{
				case "replace:":
					if ( arguments.Length == 2 )
						fileInfo.Replace ( arguments [ 0 ], arguments [ 1 ] );
					else if ( arguments.Length == 3 )
						fileInfo.Replace ( arguments [ 0 ], arguments [ 1 ], bool.Parse ( arguments [ 2 ] ) );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "concat:":
					if ( arguments.Length == 1 )
						fileInfo.Concat ( arguments [ 0 ] );
					else if ( arguments.Length == 2 )
						fileInfo.Concat ( arguments [ 0 ], bool.Parse ( arguments [ 1 ] ) );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "trim:":
					if ( arguments.Length == 0 )
						fileInfo.Trimming ();
					else if ( arguments.Length == 1 )
						fileInfo.Trimming ( bool.Parse ( arguments [ 0 ] ) );
					else if ( arguments.Length == 2 )
						fileInfo.Trimming ( bool.Parse ( arguments [ 0 ] ), bool.Parse ( arguments [ 1 ] ) );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "delblock:":
					if ( arguments.Length == 2 )
						fileInfo.DeleteEnclosed ( arguments [ 0 ], arguments [ 1 ] );
					else if ( arguments.Length == 3 )
						fileInfo.DeleteEnclosed ( arguments [ 0 ], arguments [ 1 ], bool.Parse ( arguments [ 2 ] ) );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "delname:":
					if ( arguments.Length == 0 )
						fileInfo.DeleteName ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "uppercasename:":
					if ( arguments.Length == 0 || ( arguments.Length == 1 && arguments [ 0 ] == "false" ) )
						fileInfo.NameToUpper ();
					else if ( arguments.Length == 1 && arguments [ 0 ] == "true" )
						fileInfo.NameToUpperFirstLetterOnly ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "lowercasename:":
					if ( arguments.Length == 0 )
						fileInfo.NameToLower ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "onlynum:":
					if ( arguments.Length == 0 || ( arguments.Length == 1 && arguments [ 0 ] == "false" ) )
						fileInfo.DeleteWithoutNumber ();
					else if ( arguments.Length == 1 && arguments [ 0 ] == "true" )
						fileInfo.DeleteWithoutNumberWordly ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "digitcount:":
					if ( arguments.Length == 1 )
						fileInfo.SameNumberOfDigit ( int.Parse ( arguments [ 0 ] ) );
					else if ( arguments.Length == 2 )
						fileInfo.SameNumberOfDigit ( int.Parse ( arguments [ 0 ] ), bool.Parse ( arguments [ 1 ] ) );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				//case "addnum:":
				//	break;
				case "increase:":
					if ( arguments.Length == 1 )
						fileInfo.NumberIncrese ( int.Parse ( arguments [ 0 ] ) );
					else if ( arguments.Length == 2 )
						fileInfo.NumberIncrese ( int.Parse ( arguments [ 0 ] ), bool.Parse ( arguments [ 1 ] ) );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "addext:":
					if ( arguments.Length == 1 )
						fileInfo.AddExtension ( arguments [ 0 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "delext:":
					if ( arguments.Length == 0 )
						fileInfo.DeleteExtension ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "changeext:":
					if ( arguments.Length == 1 )
						fileInfo.ChangeExtension ( arguments [ 0 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "uppercaseext:":
					if ( arguments.Length == 0 )
						fileInfo.ExtensionToUpper ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "lowercaseext:":
					if ( arguments.Length == 0 )
						fileInfo.ExtensionToLower ();
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "creationdate":
					if ( arguments.Length == 0 )
						fileInfo.AddCreationDate ();
					else if ( arguments.Length == 1 )
						fileInfo.AddCreationDate ( bool.Parse ( arguments [ 0 ] ) );
					else if ( arguments.Length == 2 )
						fileInfo.AddCreationDate ( bool.Parse ( arguments [ 0 ] ), arguments [ 1 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "accessdate":
					if ( arguments.Length == 0 )
						fileInfo.AddLastAccessDate ();
					else if ( arguments.Length == 1 )
						fileInfo.AddLastAccessDate ( bool.Parse ( arguments [ 0 ] ) );
					else if ( arguments.Length == 2 )
						fileInfo.AddLastAccessDate ( bool.Parse ( arguments [ 0 ] ), arguments [ 1 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "modifydate":
					if ( arguments.Length == 0 )
						fileInfo.AddLastWriteDate ();
					else if ( arguments.Length == 1 )
						fileInfo.AddLastWriteDate ( bool.Parse ( arguments [ 0 ] ) );
					else if ( arguments.Length == 2 )
						fileInfo.AddLastWriteDate ( bool.Parse ( arguments [ 0 ] ), arguments [ 1 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "changepath:":
					if ( arguments.Length == 1 )
						fileInfo.ChangePath ( arguments [ 0 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
				case "regexp:":
					if ( arguments.Length == 2 )
						fileInfo.RegularExpression ( new Regex ( arguments [ 0 ] ), arguments [ 1 ] );
					else throw new Exception ( string.Format ( "Function Mode Argument error: {0}", function ) );
					break;
			}
		}
	}
}

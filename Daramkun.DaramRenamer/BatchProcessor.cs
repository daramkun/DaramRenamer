using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public static class BatchProcessor
	{
		private static string [] NewLineSplitter = new [] { "\n" };
		private static char [] WordSplitter = new [] { ' ', '\t', ',', ';', '-' };
		private static char [] ArgumentsSplitter = new [] { '.' };

		internal static void BatchProcess ( this FileInfo fileInfo, string batchScript )
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
					StringBuilder stringBuilder = new StringBuilder ();
					bool started = false;
					for ( int i = 0; i < cur.Length; ++i )
					{
						char ch = cur [ i ];
						if ( started )
						{
							if ( ch == '}' )
							{
								started = false;
								stringBuilder.Append ( ConvertFormatString ( fileInfo, builder.ToString () ) );
								builder.Clear ();
							}
							else builder.Append ( ch );
						}
						else
						{
							if ( ch == '{' ) started = true;
							else stringBuilder.Append ( ch );
						}
					}
				}
			}
		}

		internal static string ConvertFormatString ( FileInfo fileInfo, string formatString )
		{
			string argument = null;
			if ( formatString.IndexOf ( ':' ) > 0 )
			{
				argument = formatString.Substring ( formatString.IndexOf ( ':' ) + 1 );
				formatString = formatString.Substring ( 0, formatString.IndexOf ( ':' ) - 1 );
			}

			switch ( formatString )
			{
				case "ts":
				case "timestamp":
					{
						string [] split = argument.Split ( ';' );
						string format = ( split.Length == 2 ) ? split [ 1 ] : "yyyyMMdd";
						switch ( split [ 0 ] )
						{
							case "create": return System.IO.File.GetCreationTime ( fileInfo.OriginalFullName ).ToString ( format );
							case "lastaccess": return System.IO.File.GetLastAccessTime ( fileInfo.OriginalFullName ).ToString ( format );
							case "lastwrite": return System.IO.File.GetLastWriteTime ( fileInfo.OriginalFullName ).ToString ( format );
						}
					}
					break;

				case "wfo":
				case "word-from-originalname":
					{
						string [] split = fileInfo.OriginalName.Split ( WordSplitter, StringSplitOptions.RemoveEmptyEntries );
						return split [ int.Parse ( argument ) ];
					}
					break;

				case "wfc":
				case "word-from-changedname":
					{
						string [] split = fileInfo.ChangedName.Split ( WordSplitter, StringSplitOptions.RemoveEmptyEntries );
						return split [ int.Parse ( argument ) ];
					}
					break;

				case "ssfo":
				case "substr-from-originalname":
					{
						string [] split = argument.Split ( '-' );
						return fileInfo.OriginalName.Substring ( int.Parse ( split [ 0 ] ), int.Parse ( split [ 1 ] ) );
					}
					break;

				case "ssfc":
				case "substr-from-changedname":
					{
						string [] split = argument.Split ( '-' );
						return fileInfo.ChangedName.Substring ( int.Parse ( split [ 0 ] ), int.Parse ( split [ 1 ] ) );
					}
					break;

				case "mi":
				case "media-info":
					{
						using ( TagLib.File file = TagLib.File.Create ( fileInfo.OriginalFullName ) )
						{
							string [] arguments = argument.Split ( ArgumentsSplitter, StringSplitOptions.RemoveEmptyEntries );
							switch ( arguments [ 0 ] )
							{
								case "audio-bitrate":
									return file.Properties.AudioBitrate.ToString ();
								case "audio-samplerate":
									return file.Properties.AudioSampleRate.ToString ();
								case "audio-channels":
									return file.Properties.AudioChannels.ToString ();
								case "audio-bitspersample":
									return file.Properties.BitsPerSample.ToString ();
								case "audio-codec":
									foreach ( var codec in file.Properties.Codecs )
										if ( codec.MediaTypes == TagLib.MediaTypes.Audio )
											return codec.Description;
									return null;

								case "video-width":
									return file.Properties.VideoWidth.ToString ();
								case "video-height":
									return file.Properties.VideoHeight.ToString ();
								case "video-codec":
									foreach ( var codec in file.Properties.Codecs )
										if ( codec.MediaTypes == TagLib.MediaTypes.Video )
											return codec.Description;
									return null;

								case "image-width":
									return file.Properties.PhotoWidth.ToString ();
								case "image-height":
									return file.Properties.PhotoHeight.ToString ();
								case "image-quality":
									return file.Properties.PhotoQuality.ToString ();
								case "image-codec":
									foreach ( var codec in file.Properties.Codecs )
										if ( codec.MediaTypes == TagLib.MediaTypes.Photo )
											return codec.Description;
									return null;

								case "audio-album":
									return file.Tag.Album;
								case "audio-album-artists":
									return arguments.Length == 1 ? string.Join ( ",", file.Tag.AlbumArtists ) : file.Tag.AlbumArtists [ int.Parse ( arguments [ 1 ] ) ];
								case "audio-composers":
									return arguments.Length == 1 ? string.Join ( ",", file.Tag.Composers ) : file.Tag.Composers [ int.Parse ( arguments [ 1 ] ) ];
								case "audio-conductor":
									return file.Tag.Conductor;
								case "video-copyright":
								case "audio-copyright":
									return file.Tag.Copyright;
								case "audio-disc":
									return file.Tag.Disc.ToString ();
								case "audio-disc-count":
									return file.Tag.DiscCount.ToString ();
								case "audio-genre":
								case "video-genre":
									return arguments.Length == 1 ? string.Join ( ",", file.Tag.Genres ) : file.Tag.Genres [ int.Parse ( arguments [ 1 ] ) ];
								case "audio-performers":
									return arguments.Length == 1 ? string.Join ( ",", file.Tag.Performers ) : file.Tag.Performers [ int.Parse ( arguments [ 1 ] ) ];
								case "audio-title":
								case "video-title":
									return file.Tag.Title;
								case "audio-track":
									return file.Tag.Track.ToString ();
								case "audio-track-count":
									return file.Tag.TrackCount.ToString ();
								case "audio-year":
								case "video-year":
									return file.Tag.Year.ToString ();

								default:
									throw new Exception ( "Not support this info argument." );
							}
						}
					}
					break;

				case "di":
				case "doc-info":
					{
						OpenDocumentTag.File file = new OpenDocumentTag.File ( fileInfo.OriginalFullName );
						switch ( argument )
						{
							case "title":
								return file.Title;
							case "author":
								return file.Author;

							default:
								throw new Exception ( "Not support this info argument." );
						}
					}
					break;

				case "hash":
					{
						byte [] returnValue = null;
						switch ( argument )
						{
							case "sha1":
								{
									using ( Stream stream = System.IO.File.Open ( fileInfo.OriginalFullName, System.IO.FileMode.Open ) )
										returnValue = System.Security.Cryptography.SHA1.Create ().ComputeHash ( stream );
								}
								break;

							case "sha256":
								{
									using ( Stream stream = System.IO.File.Open ( fileInfo.OriginalFullName, System.IO.FileMode.Open ) )
										returnValue = System.Security.Cryptography.SHA256.Create ().ComputeHash ( stream );
								}
								break;

							case "sha384":
								{
									using ( Stream stream = System.IO.File.Open ( fileInfo.OriginalFullName, System.IO.FileMode.Open ) )
										returnValue = System.Security.Cryptography.SHA384.Create ().ComputeHash ( stream );
								}
								break;

							case "sha512":
								{
									using ( Stream stream = System.IO.File.Open ( fileInfo.OriginalFullName, System.IO.FileMode.Open ) )
										returnValue = System.Security.Cryptography.SHA512.Create ().ComputeHash ( stream );
								}
								break;

							case "md5":
								{
									using ( Stream stream = System.IO.File.Open ( fileInfo.OriginalFullName, System.IO.FileMode.Open ) )
										returnValue = MD5.Create ().ComputeHash ( stream );
								}
								break;

							default:
								throw new Exception ( "Not support this hash method." );
						}

						StringBuilder sBuilder = new StringBuilder ();
						for ( int i = 0; i < returnValue.Length; i++ )
						{
							sBuilder.Append ( returnValue [ i ].ToString ( "x2" ) );
						}
						return sBuilder.ToString ();
					}
					break;
			}
			return null;
		}

		internal static void ProcessFunction ( FileInfo fileInfo, string function, string [] arguments )
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

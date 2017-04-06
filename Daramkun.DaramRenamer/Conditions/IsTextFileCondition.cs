using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Conditions
{
	public class IsTextFileCondition : ICondition
	{
		public string Name => "condition_is_text_file";

		public bool IsValid ( FileInfo file )
		{
			return IsText ( file.OriginalFullPath, 4096 );
		}

		// Original Source code from http://stackoverflow.com/a/6613967
		private static bool IsText ( string fileName, int windowSize )
		{
			using ( var fileStream = File.OpenRead ( fileName ) )
			{
				var rawData = new byte [ windowSize ];
				var text = new char [ windowSize ];
				var isText = true;

				var rawLength = fileStream.Read ( rawData, 0, rawData.Length );
				fileStream.Seek ( 0, SeekOrigin.Begin );

				Encoding encoding;

				if ( rawData [ 0 ] == 0xef && rawData [ 1 ] == 0xbb && rawData [ 2 ] == 0xbf )
					encoding = Encoding.UTF8;
				else if ( rawData [ 0 ] == 0xfe && rawData [ 1 ] == 0xff )
					encoding = Encoding.Unicode;
				else if ( rawData [ 0 ] == 0 && rawData [ 1 ] == 0 && rawData [ 2 ] == 0xfe && rawData [ 3 ] == 0xff )
					encoding = Encoding.UTF32;
				else if ( rawData [ 0 ] == 0x2b && rawData [ 1 ] == 0x2f && rawData [ 2 ] == 0x76 )
					encoding = Encoding.UTF7;
				else
					encoding = Encoding.Default;

				using ( var streamReader = new StreamReader ( fileStream ) )
					streamReader.Read ( text, 0, text.Length );

				using ( var memoryStream = new MemoryStream () )
				{
					using ( var streamWriter = new StreamWriter ( memoryStream, encoding ) )
					{
						streamWriter.Write ( text );
						streamWriter.Flush ();

						var memoryBuffer = memoryStream.GetBuffer ();
						for ( var i = 0; i < rawLength && isText; i++ )
							isText = rawData [ i ] == memoryBuffer [ i ];
					}
				}

				return isText;
			}
		}
	}
}

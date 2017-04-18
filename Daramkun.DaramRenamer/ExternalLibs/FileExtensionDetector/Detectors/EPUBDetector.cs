using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExtensionDetector;

namespace FileExtensionDetector.Detectors
{
	public class EPUBDetector : ExtDetector
	{
		public override string Extension { get { return "epub"; } }
		public override string Precondition { get { return "zip"; } }

		public override bool Detect ( Stream stream )
		{
			try
			{
				using ( ZipArchive archive = new ZipArchive ( stream, ZipArchiveMode.Read, true ) )
				{
					foreach ( var entry in archive.Entries )
					{
						if ( entry.FullName == "mimetype" )
						{
							using ( Stream mimetypeStream = entry.Open () )
							{
								byte [] buffer = new byte [ "application/epub+zip".Length ];
								if ( mimetypeStream.Read ( buffer, 0, buffer.Length ) != buffer.Length )
									return false;
								if ( Encoding.ASCII.GetString ( buffer ) == "application/epub+zip" )
									return true;
							}
							return false;
						}
					}
				}
				return false;
			}
			catch { return false; }
		}
	}
}

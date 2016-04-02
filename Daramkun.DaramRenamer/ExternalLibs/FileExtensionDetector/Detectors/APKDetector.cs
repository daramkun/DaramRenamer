using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class APKDetector : ExtDetector
	{
		public override string Extension { get { return "apk"; } }
		public override string Precondition { get { return "zip"; } }

		public override bool Detect ( Stream stream )
		{
			try
			{
				bool classesDetect = false;
				bool manifestDetect = false;
				using ( ZipArchive archive = new ZipArchive ( stream, ZipArchiveMode.Read, true ) )
				{
					foreach ( var entry in archive.Entries )
						if ( entry.FullName == "classes.dex" )
							classesDetect = true;
						else if ( entry.FullName == "AndroidManifest.xml" )
							manifestDetect = true;
				}
				return classesDetect && manifestDetect;
			}
			catch { return false; }
		}
	}
}

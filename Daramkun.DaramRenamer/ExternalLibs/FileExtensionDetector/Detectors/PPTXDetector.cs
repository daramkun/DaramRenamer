using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class PPTXDetector : ExtDetector
	{
		public override string Extension { get { return "pptx"; } }
		public override string Precondition { get { return "zip"; } }

		public override bool Detect ( Stream stream )
		{
			try
			{
				bool contentTypesDetect = false;
				bool relsDetect = false;
				bool wordDetect = false;
				using ( ZipArchive archive = new ZipArchive ( stream, ZipArchiveMode.Read, true ) )
				{
					foreach ( var entry in archive.Entries )
						if ( entry.FullName == "[Content_Types].xml" )
							contentTypesDetect = true;
						else if ( entry.FullName == "_rels/.rels" )
							relsDetect = true;
						else if ( entry.FullName == "ppt/_rels/presentation.xml.rels" )
							wordDetect = true;
				}
				return contentTypesDetect && relsDetect && wordDetect;
			}
			catch { return false; }
		}
	}
}

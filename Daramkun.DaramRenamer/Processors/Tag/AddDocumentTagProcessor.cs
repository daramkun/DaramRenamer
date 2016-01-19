﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Tag
{
	public class AddDocumentTagProcessor : IProcessor
	{
		public string Name { get { return "process_add_document_tag"; } }
		public bool CannotMultithreadProcess { get { return false; } }

		[Globalized ( "document_tag_type", 0 )]
		public DocumentTag Tag { get; set; }
		[Globalized ( "document_tag_pos", 1 )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			OpenDocumentTag.File f = null;
			try
			{
				f = new OpenDocumentTag.File ( file.OriginalFullPath );
			}
			catch { return false; }

			string tag = "";
			switch ( Tag )
			{
				case DocumentTag.Title: tag = f.Title; break;
				case DocumentTag.Author: tag = f.Author; break;
			}

			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			file.ChangedFilename = Position == OnePointPosition.StartPoint ? $"{tag}{fn}{ext}" :
				( Position == OnePointPosition.EndPoint ? $"{fn}{tag}{ext}" :
				$"{tag}{fn}{tag}{ext}" );

			return true;
		}
	}
}

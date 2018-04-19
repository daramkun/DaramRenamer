using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Daramkun.DaramRenamer.Processors.Tag
{
	[Serializable]
	public class AddDocumentTagProcessor : IProcessor
	{
		public string Name => "process_add_document_tag";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "document_tag_type" )]
		public DocumentTag Tag { get; set; }
		[Argument ( Name = "document_tag_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			AddDocumentTagProcessor.File f = null;
			try
			{
				f = new AddDocumentTagProcessor.File ( file.OriginalFullPath );
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

		class File
		{
			public string Title { get; private set; }
			public string Author { get; private set; }

			public File ( string path )
			{
				using ( var stream = new FileStream ( path, FileMode.Open ) )
				{
					var archive = new System.IO.Compression.ZipArchive ( stream );
					foreach ( var entry in archive.Entries )
					{
						if ( entry.FullName == "docProps/core.xml" )
						{
							var xml = new XmlDocument ();
							xml.LoadXml ( new StreamReader ( entry.Open () ).ReadToEnd () );

							foreach ( var element in xml.DocumentElement.ChildNodes )
							{
								if ( ( element as XmlElement ).LocalName == "title" ) Title = ( element as XmlElement ).InnerText;
								else if ( ( element as XmlElement ).LocalName == "creator" ) Author = ( element as XmlElement ).InnerText;
							}
						}
						else if ( entry.FullName == "meta.xml" )
						{
							var xml = new XmlDocument ();
							xml.LoadXml ( new StreamReader ( entry.Open () ).ReadToEnd () );

							foreach ( var element in xml.DocumentElement.ChildNodes [ 0 ].ChildNodes )
							{
								if ( ( element as XmlElement ).LocalName == "title" ) Title = ( element as XmlElement ).InnerText;
								else if ( ( element as XmlElement ).LocalName == "creator" ) Author = ( element as XmlElement ).InnerText;
							}
						}
					}
				}
			}
		}
	}
}

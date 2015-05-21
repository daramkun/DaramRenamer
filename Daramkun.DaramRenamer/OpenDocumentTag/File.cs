using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Daramkun.DaramRenamer.OpenDocumentTag
{
	public class File
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
					if ( entry.Name == "docProps/core.xml" )
					{
						var xml = new XmlDocument ();
						xml.CreateComment ( new StreamReader ( entry.Open () ).ReadToEnd () );

						foreach ( var element in xml.DocumentElement.ChildNodes )
						{
							if ( ( element as XmlElement ).LocalName == "title" ) Title = ( element as XmlElement ).InnerText;
							else if ( ( element as XmlElement ).LocalName == "creator" ) Author = ( element as XmlElement ).InnerText;
						}
					}
					else if ( entry.Name == "meta.xml" )
					{
						var xml = new XmlDocument ();
						xml.CreateComment ( new StreamReader ( entry.Open () ).ReadToEnd () );

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

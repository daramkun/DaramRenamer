using System;
using System.IO;
using System.Xml;

namespace DaramRenamer.Commands.Tags
{
	[Serializable, LocalizationKey("Command_Name_AddDocumentTag")]
	public class AddDocumentTagCommand : ICommand
	{
		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Tag;

		[LocalizationKey("Command_Argument_AddDocumentTag_Tag")]
		public DocumentTag Tag { get; set; }
		[LocalizationKey("Command_Argument_AddDocumentTag_Position")]
		public Position1 Position { get; set; } = Position1.EndPoint;

		public bool DoCommand(FileInfo file)
		{
			File f;
			try
			{
				f = new File(file.OriginalFullPath);
			}
			catch { return false; }

			var tag = Tag switch
			{
				DocumentTag.Title => f.Title,
				DocumentTag.Author => f.Author,
				_ => ""
			};

			var fn = Path.GetFileNameWithoutExtension(file.ChangedFilename);
			var ext = Path.GetExtension(file.ChangedFilename);
			file.ChangedFilename = Position switch
			{
				Position1.StartPoint => $"{tag}{fn}{ext}",
				Position1.EndPoint => $"{fn}{tag}{ext}",
				_ => $"{tag}{fn}{tag}{ext}"
			};

			return true;
		}

		public class File
		{
			public string Title { get; }
			public string Author { get; }

			public File(string path)
			{
				using var stream = new FileStream(path, FileMode.Open);
				var archive = new System.IO.Compression.ZipArchive(stream);
				foreach (var entry in archive.Entries)
				{
					switch (entry.FullName)
					{
						case "docProps/core.xml":
							{
								var xml = new XmlDocument();
								xml.LoadXml(new StreamReader(entry.Open()).ReadToEnd());

								if (xml.DocumentElement == null)
									continue;
								foreach (var element in xml.DocumentElement.ChildNodes)
								{
									if (element != null)
										switch ((element as XmlElement)?.LocalName)
										{
											case "title":
												Title = ((XmlElement)element).InnerText;
												break;
											case "creator":
												Author = ((XmlElement)element).InnerText;
												break;
										}
								}

								break;
							}
						case "meta.xml":
							{
								var xml = new XmlDocument();
								xml.LoadXml(new StreamReader(entry.Open()).ReadToEnd());

								if (xml.DocumentElement == null)
									continue;
								foreach (var element in xml.DocumentElement.ChildNodes[0].ChildNodes)
								{
									if (element != null)
										switch ((element as XmlElement)?.LocalName)
										{
											case "title":
												Title = (element as XmlElement).InnerText;
												break;
											case "creator":
												Author = (element as XmlElement).InnerText;
												break;
										}
								}

								break;
							}
					}
				}
			}
		}
	}
}

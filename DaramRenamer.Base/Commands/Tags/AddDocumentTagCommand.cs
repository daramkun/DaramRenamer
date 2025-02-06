using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddDocumentTag")]
public class AddDocumentTagCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Tag;
    
    [LocalizationKey("Command_Argument_AddDocumentTag_Tag")]
    public DocumentTag Tag { get; set; }

    [LocalizationKey("Command_Argument_AddDocumentTag_Position")]
    public Position Position { get; set; } = Position.End;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (fileInfo.IsDirectory)
            return true;

        File f;
        try
        {
            f = new File(fileInfo.OriginalFullPath);
        }
        catch
        {
            return false;
        }

        var tag = Tag switch
        {
            DocumentTag.Title => f.Title,
            DocumentTag.Author => f.Author,
            _ => ""
        };

        var fn = fileInfo.ChangedNameWithoutExtension;
        var ext = fileInfo.ChangedNameExtension;
        fileInfo.ChangedName = Position switch
        {
            Position.Begin => $"{tag}{fn}{ext}",
            Position.End => $"{fn}{tag}{ext}",
            Position.Both => $"{tag}{fn}{tag}{ext}",
            _ => $"{tag}{fn}{tag}{ext}"
        };

        return true;
    }

    public class File
    {
        public File(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            var archive = new ZipArchive(stream);
            foreach (var entry in archive.Entries)
                switch (entry.FullName)
                {
                    case "docProps/core.xml":
                    {
                        var xml = new XmlDocument();
                        xml.LoadXml(new StreamReader(entry.Open()).ReadToEnd());

                        if (xml.DocumentElement == null)
                            continue;
                        foreach (var element in xml.DocumentElement.ChildNodes)
                            if (element != null)
                                switch ((element as XmlElement)?.LocalName)
                                {
                                    case "title":
                                        Title = ((XmlElement) element).InnerText;
                                        break;
                                    case "creator":
                                        Author = ((XmlElement) element).InnerText;
                                        break;
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

                        break;
                    }
                }
        }

        public string Title { get; }
        public string Author { get; }
    }
}
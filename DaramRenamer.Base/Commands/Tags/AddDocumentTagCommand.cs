using System.ComponentModel;
using System.IO.Compression;
using System.Xml;

namespace DaramRenamer.Commands;

[Serializable]
public class AddDocumentTagCommand : ICommand
{
    public DocumentTag Tag
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tag)));
        }
    } = DocumentTag.Title;

    public Position3 Position
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    } = Position3.End;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Tag;
    
    public void Apply(FileItem item, int _)
    {
        if (item.IsDirectory)
            return;

        File f;
        try
        {
            f = new File(item.SourceFullPath);
        }
        catch
        {
            return;
        }

        var tag = Tag switch
        {
            DocumentTag.Title => f.Title ?? string.Empty,
            DocumentTag.Author => f.Author ?? string.Empty,
            _ => string.Empty
        };

        var fn = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;
        item.ChangedName = Position switch
        {
            Position3.Begin => $"{tag}{fn}{ext}",
            Position3.End => $"{fn}{tag}{ext}",
            Position3.Both => $"{tag}{fn}{tag}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
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
                        
                        var childNode = xml.DocumentElement.ChildNodes[0];

                        if (childNode != null)
                        {
                            foreach (var element in childNode.ChildNodes)
                                if (element != null)
                                    switch ((element as XmlElement)?.LocalName)
                                    {
                                        case "title":
                                            Title = (element as XmlElement)?.InnerText;
                                            break;
                                        case "creator":
                                            Author = (element as XmlElement)?.InnerText;
                                            break;
                                    }
                        }

                        break;
                    }
                }
        }

        public string? Title { get; }
        public string? Author { get; }
    }
}
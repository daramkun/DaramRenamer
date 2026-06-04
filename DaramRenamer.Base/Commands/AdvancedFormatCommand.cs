using System.ComponentModel;
using System.Text;
using DaramRenamer.FileTypes;
using TagLib;
using File = TagLib.File;

namespace DaramRenamer.Commands;

[Serializable]
public class AdvancedFormatCommand : ICommand
{
    private string _fileNameFormatString = string.Empty;
    private List<FormatStringNode> _fileNameNodes = new();
    private string _pathFormatString = string.Empty;
    private List<FormatStringNode> _pathNodes = new();

    public AdvancedFormatCommand()
    {
        FileNameFormatString = "${ProceedFileName}${ProceedExtension}";
        PathFormatString = "${ProceedPath}";
    }

    public string FileNameFormatString
    {
        get => _fileNameFormatString;
        set
        {
            _fileNameFormatString = value;
            ParseFormatString(value, _fileNameNodes);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileNameFormatString)));
        }
    }

    public string PathFormatString
    {
        get => _pathFormatString;
        set
        {
            _pathFormatString = value;
            ParseFormatString(value, _pathNodes);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PathFormatString)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => 1;
    public CommandCategory Category => CommandCategory.Etc;

    public void Apply(FileItem item, int index)
    {
        var builder = new StringBuilder();

        foreach (var node in _fileNameNodes)
            builder.Append(node.GetValue(item, index));

        item.ChangedName = builder.ToString();

        builder.Clear();

        foreach (var node in _pathNodes)
            builder.Append(node.GetValue(item, index));

        item.ChangedPath = builder.ToString();
    }

    private void ParseFormatString(string formatString, List<FormatStringNode> nodes)
    {
        nodes.Clear();

        var queue = new Queue<char>(formatString);
        var isFormat = false;
        var token = new StringBuilder();

        while (queue.Count > 0)
        {
            var ch = queue.Dequeue();
            if (!isFormat && ch == '$')
            {
                if (token.Length > 0)
                {
                    nodes.Add(new StringNode(token.ToString()));
                    token.Clear();
                }

                token.Append(ch);
            }
            else if (!isFormat && ch == '{' && token.Length == 1 && token[0] == '$')
            {
                token.Clear();
                isFormat = true;
            }
            else if (isFormat && ch == '}')
            {
                isFormat = false;

                var tokenString = token.ToString();
                try
                {
                    if (EnvironmentVariableNode.IsEnvironmentVariable(tokenString))
                        nodes.Add(new EnvironmentVariableNode(tokenString));
                    else if (MediaTagNode.IsMediaTag(tokenString))
                        nodes.Add(new MediaTagNode(tokenString));
                    else if (DocumentTagNode.IsDocumentTag(tokenString))
                        nodes.Add(new DocumentTagNode(tokenString));
                    else if (HashNode.IsHash(tokenString))
                        nodes.Add(new HashNode(tokenString));
                    else if (GitNode.IsGit(tokenString))
                        nodes.Add(new GitNode(tokenString));
                    else if (MacroNode.IsMacro(tokenString))
                        nodes.Add(new MacroNode(tokenString));
                    else
                        nodes.Add(new StringNode($"${{{tokenString}}}"));
                }
                catch
                {
                    nodes.Add(new StringNode($"${{{tokenString}}}"));
                }

                token.Clear();
            }
            else
            {
                token.Append(ch);
            }
        }

        if (token.Length > 0) nodes.Add(new StringNode(token.ToString()));
    }

    private abstract class FormatStringNode
    {
        public readonly string Token;

        protected FormatStringNode(string token)
        {
            Token = token;
        }

        public abstract string GetValue(FileItem item, int index);
    }

    private class StringNode : FormatStringNode
    {
        public StringNode(string token) : base(token) { }

        public override string GetValue(FileItem item, int index) => Token;
    }

    private class EnvironmentVariableNode : FormatStringNode
    {
        public readonly string VariableName;
        public readonly EnvironmentVariableTarget VariableTarget;

        public EnvironmentVariableNode(string token) : base(token)
        {
            var identifier = token.IndexOf(':');
            var varName = token[(identifier + 1)..];
            var secondIdentifier = varName.IndexOf(':');
            if (secondIdentifier > 0)
            {
                VariableName = varName[..secondIdentifier];
                VariableTarget = Enum.Parse<EnvironmentVariableTarget>(varName[(secondIdentifier + 1)..], true);
            }
            else
            {
                VariableName = varName;
                VariableTarget = EnvironmentVariableTarget.Process;
            }
        }

        public static bool IsEnvironmentVariable(string token) => token.StartsWith("env:");

        public override string GetValue(FileItem item, int index) =>
            Environment.GetEnvironmentVariable(VariableName, VariableTarget) ?? string.Empty;
    }

    private class MediaTagNode : FormatStringNode
    {
        public readonly int Arguments;
        public readonly MediaTag TagType;

        public MediaTagNode(string token) : base(token)
        {
            var identifier = token.IndexOf(':');
            var tag = token[(identifier + 1)..];
            var secondIdentifier = tag.IndexOf(':');

            if (secondIdentifier > 0)
            {
                TagType = Enum.Parse<MediaTag>(tag[..secondIdentifier], true);
                Arguments = int.Parse(tag[(secondIdentifier + 1)..]);
            }
            else
            {
                TagType = Enum.Parse<MediaTag>(tag, true);
                Arguments = -1;
            }
        }

        public static bool IsMediaTag(string token) => token.StartsWith("media:");

        public override string GetValue(FileItem item, int index)
        {
            TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF8;
            TagLib.Id3v2.Tag.DefaultVersion = 4;

            File f;
            try
            {
                f = File.Create(new File.LocalFileAbstraction(item.SourceFullPath), ReadStyle.PictureLazy);
            }
            catch
            {
                return string.Empty;
            }

            var tag = "";
            switch (TagType)
            {
                case MediaTag.AudioAlbum:
                    tag = f.Tag.Album;
                    break;
                case MediaTag.AudioAlbumArtists:
                    tag = Arguments != -1 ? f.Tag.AlbumArtists[Arguments] : string.Join(",", f.Tag.AlbumArtists);
                    break;
                case MediaTag.AudioComposers:
                    tag = Arguments != -1 ? f.Tag.Composers[Arguments] : string.Join(",", f.Tag.Composers);
                    break;
                case MediaTag.AudioCopyright:
                case MediaTag.VideoCopyright:
                    tag = f.Tag.Copyright;
                    break;
                case MediaTag.AudioDisc:
                    tag = f.Tag.Disc.ToString();
                    break;
                case MediaTag.AudioDiscCount:
                    tag = f.Tag.DiscCount.ToString();
                    break;
                case MediaTag.AudioGenres:
                case MediaTag.VideoGenres:
                    tag = Arguments != -1 ? f.Tag.Genres[Arguments] : string.Join(",", f.Tag.Genres);
                    break;
                case MediaTag.AudioPerformers:
                    tag = Arguments != -1 ? f.Tag.Performers[Arguments] : string.Join(",", f.Tag.Performers);
                    break;
                case MediaTag.AudioTitle:
                case MediaTag.VideoTitle:
                    tag = f.Tag.Title;
                    break;
                case MediaTag.AudioTrack:
                    tag = f.Tag.Track.ToString();
                    break;
                case MediaTag.AudioTrackCount:
                    tag = f.Tag.TrackCount.ToString();
                    break;
                case MediaTag.AudioYear:
                case MediaTag.VideoYear:
                    tag = f.Tag.Year.ToString();
                    break;
                case MediaTag.AudioConductor:
                    tag = f.Tag.Conductor;
                    break;
                case MediaTag.AudioDuration:
                case MediaTag.VideoDuration:
                    tag = f.Properties.Duration.ToString("hh:mm:ss");
                    break;
                case MediaTag.AudioCodec:
                    foreach (var codec in f.Properties.Codecs)
                        if (codec.MediaTypes == MediaTypes.Audio)
                            tag = codec.Description;
                    break;
                case MediaTag.AudioSamplerate:
                    tag = f.Properties.AudioSampleRate.ToString();
                    break;
                case MediaTag.AudioBitrate:
                    tag = f.Properties.AudioBitrate.ToString();
                    break;
                case MediaTag.AudioBitsPerSample:
                    tag = f.Properties.BitsPerSample.ToString();
                    break;
                case MediaTag.AudioChannels:
                    tag = f.Properties.AudioChannels.ToString();
                    break;
                case MediaTag.ImageWidth:
                    tag = f.Properties.PhotoWidth.ToString();
                    break;
                case MediaTag.ImageHeight:
                    tag = f.Properties.PhotoHeight.ToString();
                    break;
                case MediaTag.ImageQuality:
                    tag = f.Properties.PhotoQuality.ToString();
                    break;
                case MediaTag.ImageCodec:
                    foreach (var codec in f.Properties.Codecs)
                        if (codec.MediaTypes == MediaTypes.Audio)
                            tag = codec.Description;
                    break;
                case MediaTag.VideoWidth:
                    tag = f.Properties.VideoWidth.ToString();
                    break;
                case MediaTag.VideoHeight:
                    tag = f.Properties.VideoHeight.ToString();
                    break;
                case MediaTag.VideoCodec:
                    foreach (var codec in f.Properties.Codecs)
                        if (codec.MediaTypes == MediaTypes.Video)
                            tag = codec.Description;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            f.Dispose();

            return AddMediaTagCommand.ConvertUnicodeText(tag ?? string.Empty);
        }
    }

    private class DocumentTagNode : FormatStringNode
    {
        public readonly int Arguments;
        public readonly DocumentTag TagType;

        public DocumentTagNode(string token) : base(token)
        {
            var identifier = token.IndexOf(':');
            var tag = token[(identifier + 1)..];
            var secondIdentifier = tag.IndexOf(':');

            if (secondIdentifier > 0)
            {
                TagType = Enum.Parse<DocumentTag>(tag[..secondIdentifier], true);
                Arguments = int.Parse(tag[(secondIdentifier + 1)..]);
            }
            else
            {
                TagType = Enum.Parse<DocumentTag>(tag, true);
                Arguments = -1;
            }
        }

        public static bool IsDocumentTag(string token) => token.StartsWith("doc:");

        public override string GetValue(FileItem item, int index)
        {
            AddDocumentTagCommand.File f;
            try
            {
                f = new AddDocumentTagCommand.File(item.SourceFullPath);
            }
            catch
            {
                return string.Empty;
            }

            return TagType switch
            {
                DocumentTag.Title => f.Title ?? string.Empty,
                DocumentTag.Author => f.Author ?? string.Empty,
                _ => string.Empty
            };
        }
    }

    private class HashNode : FormatStringNode
    {
        public readonly HashKind HashKind;

        public HashNode(string token) : base(token)
        {
            var identifier = token.IndexOf(':');
            HashKind = Enum.Parse<HashKind>(token[(identifier + 1)..]);
        }

        public static bool IsHash(string token) => token.StartsWith("hash:");

        public override string GetValue(FileItem item, int index) =>
            AddHashCommand.ComputeHash(HashKind, item.SourceFullPath);
    }

    private class GitNode : FormatStringNode
    {
        public readonly GitInfo GitInfo;

        public GitNode(string token) : base(token)
        {
            var identifier = token.IndexOf(':');
            GitInfo = Enum.Parse<GitInfo>(token[(identifier + 1)..]);
        }

        public static bool IsGit(string token) => token.StartsWith("git:");

        public override string GetValue(FileItem item, int index) =>
            AddGitInfoCommand.GetGitValue(GitInfo, item.SourceFullPath);
    }

    private enum MacroTypes
    {
        FileName,
        ProceedFileName,
        Extension,
        ProceedExtension,
        CurrentDirectory,
        ProceedDirectory,
        Path,
        ProceedPath,
        Index,
        FoundExtension
    }

    private class MacroNode : FormatStringNode
    {
        public readonly int Arguments;
        public readonly MacroTypes MacroType;

        public MacroNode(string token) : base(token)
        {
            var secondIdentifier = token.IndexOf(':');

            if (secondIdentifier > 0)
            {
                MacroType = Enum.Parse<MacroTypes>(token[..secondIdentifier], true);
                Arguments = int.Parse(token[(secondIdentifier + 1)..]);
            }
            else
            {
                MacroType = Enum.Parse<MacroTypes>(token, true);
                Arguments = -1;
            }
        }

        public static bool IsMacro(string token)
        {
            var i = token.IndexOf(':');
            if (i >= 0)
                token = token[..i];
            return Enum.TryParse<MacroTypes>(token, true, out _);
        }

        public override string GetValue(FileItem item, int index)
        {
            switch (MacroType)
            {
                case MacroTypes.FileName:
                    return item.IsDirectory
                        ? item.SourceName
                        : System.IO.Path.GetFileNameWithoutExtension(item.SourceName);

                case MacroTypes.Extension:
                    return item.IsDirectory
                        ? string.Empty
                        : System.IO.Path.GetExtension(item.SourceName);

                case MacroTypes.ProceedFileName:
                    return item.IsDirectory
                        ? item.ChangedName
                        : System.IO.Path.GetFileNameWithoutExtension(item.ChangedName);

                case MacroTypes.ProceedExtension:
                    return item.IsDirectory
                        ? string.Empty
                        : System.IO.Path.GetExtension(item.ChangedName);

                case MacroTypes.CurrentDirectory:
                    return System.IO.Path.GetFileName(item.SourcePath);

                case MacroTypes.ProceedDirectory:
                    return System.IO.Path.GetFileName(item.ChangedPath);

                case MacroTypes.Path:
                    return item.SourcePath;

                case MacroTypes.ProceedPath:
                    return item.ChangedPath;

                case MacroTypes.Index:
                    var i = (index + 1).ToString();
                    return Arguments < 0 ? i : i.PadLeft(Arguments, '0');

                case MacroTypes.FoundExtension:
                {
                    using Stream stream = System.IO.File.OpenRead(item.SourceFullPath);
                    var detector = FileTypeDetector.Detect(stream);
                    return detector == null ? string.Empty : detector.Extension;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

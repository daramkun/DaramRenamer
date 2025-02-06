using System.Text;
using Daramee.FileTypeDetector;
using DaramRenamer.Attributes;
using DaramRenamer.Utilities;
using TagLib;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AdvancedFormat")]
internal class AdvancedFormatCommand : BaseCommand, ITargetContains
{
    public override CommandCategory Category => CommandCategory.Etc;
    public override int Order => 1;

    private IEnumerable<BaseFileInfo> _files = [];
    
    private string _fileNameFormatString = string.Empty;
    private List<FormatStringNode> _fileNameNodes = new();
    private string _pathFormatString = string.Empty;
    private List<FormatStringNode> _pathNodes = new();

    public AdvancedFormatCommand()
    {
        FileNameFormatString = "${ProceedFileName}{ProceedExtension}";
        PathFormatString = "${ProceedPath}";
    }

    [LocalizationKey("Command_Argument_AdvancedFormat_FileNameFormatString")]
    public string FileNameFormatString
    {
        get => _fileNameFormatString;
        set
        {
            _fileNameFormatString = value;
            ParseFormatString(value, _fileNameNodes);
        }
    }

    [LocalizationKey("Command_Argument_AdvancedFormat_PathFormatString")]
    public string PathFormatString
    {
        get => _pathFormatString;
        set
        {
            _pathFormatString = value;
            ParseFormatString(value, _pathNodes);
        }
    }

    public override bool DoCommand(BaseFileInfo file)
    {
        var builder = new StringBuilder();

        foreach (var node in _fileNameNodes)
            builder.Append(node.GetValue(file));

        file.ChangedName = builder.ToString();

        builder.Clear();

        foreach (var node in _pathNodes)
            builder.Append(node.GetValue(file));

        file.ChangedPath = builder.ToString();

        return true;
    }

    public void SetTargets(IEnumerable<BaseFileInfo> files) =>
        _files = files;

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
                    nodes.Add(new StringNode(this, token.ToString()));
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
                        nodes.Add(new EnvironmentVariableNode(this, tokenString));
                    else if (MediaTagNode.IsMediaTag(tokenString))
                        nodes.Add(new MediaTagNode(this, tokenString));
                    else if (DocumentTagNode.IsDocumentTag(tokenString))
                        nodes.Add(new DocumentTagNode(this, tokenString));
                    else if (HashNode.IsHash(tokenString))
                        nodes.Add(new HashNode(this, tokenString));
                    else if (GitNode.IsGit(tokenString))
                        nodes.Add(new GitNode(this, tokenString));
                    else if (MacroNode.IsMacro(tokenString))
                        nodes.Add(new MacroNode(this, tokenString));
                    else
                        nodes.Add(new StringNode(this, $"${{{tokenString}}}"));
                }
                catch
                {
                    nodes.Add(new StringNode(this, $"${{{tokenString}}}"));
                }

                token.Clear();
            }
            else
            {
                token.Append(ch);
            }
        }

        if (token.Length > 0) nodes.Add(new StringNode(this, token.ToString()));
    }

    [Serializable]
    private abstract class FormatStringNode(AdvancedFormatCommand owner, string token)
    {
        protected readonly WeakReference<AdvancedFormatCommand> Owner = new(owner);
        public readonly string Token = token;

        public abstract string GetValue(BaseFileInfo fileInfo);
    }

    [Serializable]
    private class StringNode(AdvancedFormatCommand owner, string token)
        : FormatStringNode(owner, token)
    {
        public override string GetValue(BaseFileInfo fileInfo) => Token;
    }

    [Serializable]
    private class EnvironmentVariableNode : FormatStringNode
    {
        public readonly string VariableName;
        public readonly EnvironmentVariableTarget VariableTarget;

        public EnvironmentVariableNode(AdvancedFormatCommand owner, string token)
            : base(owner, token)
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

        public static bool IsEnvironmentVariable(string token) =>
            token.StartsWith("env:");

        public override string GetValue(BaseFileInfo fileInfo) =>
            Environment.GetEnvironmentVariable(VariableName, VariableTarget) ?? string.Empty;
    }

    [Serializable]
    private class MediaTagNode : FormatStringNode
    {
        public readonly int Arguments;
        public readonly MediaTag TagType;

        public MediaTagNode(AdvancedFormatCommand owner, string token) : base(owner, token)
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

        public static bool IsMediaTag(string token) =>
            token.StartsWith("media:");

        public override string GetValue(BaseFileInfo fileInfo)
        {
            Tag.DefaultEncoding = StringType.UTF8;
            Tag.DefaultVersion = 4;

            var f = File.Create(new File.LocalFileAbstraction(fileInfo.OriginalFullPath), ReadStyle.PictureLazy);

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

            return AddMediaTagCommand.ConvertUnicodeText(tag);
        }
    }

    [Serializable]
    private class DocumentTagNode : FormatStringNode
    {
        public readonly int Arguments;
        public readonly DocumentTag TagType;

        public DocumentTagNode(AdvancedFormatCommand owner, string token) : base(owner, token)
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

        public static bool IsDocumentTag(string token) =>
            token.StartsWith("doc:");

        public override string GetValue(BaseFileInfo fileInfo)
        {
            AddDocumentTagCommand.File f = new(fileInfo.OriginalFullPath);

            return TagType switch
            {
                DocumentTag.Title => f.Title,
                DocumentTag.Author => f.Author,
                _ => ""
            };
        }
    }

    [Serializable]
    private class HashNode : FormatStringNode
    {
        public readonly HashType HashType;

        public HashNode(AdvancedFormatCommand owner, string token) : base(owner, token)
        {
            var identifier = token.IndexOf(':');
            HashType = Enum.Parse<HashType>(token[(identifier + 1)..]);
        }

        public static bool IsHash(string token) =>
            token.StartsWith("hash:");

        public override string GetValue(BaseFileInfo fileInfo) =>
            AddHashCommand.ComputeHash(HashType, fileInfo.OriginalFullPath);
    }

    [Serializable]
    private class GitNode : FormatStringNode
    {
        public readonly GitDetermineKind GitInfo;

        public GitNode(AdvancedFormatCommand owner, string token) : base(owner, token)
        {
            var identifier = token.IndexOf(':');
            GitInfo = Enum.Parse<GitDetermineKind>(token[(identifier + 1)..]);
        }

        public static bool IsGit(string token) =>
            token.StartsWith("git:");

        public override string GetValue(BaseFileInfo fileInfo) =>
            GitUtil.Determine(GitInfo, fileInfo.OriginalFullPath) ?? string.Empty;
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

    [Serializable]
    private class MacroNode : FormatStringNode
    {
        public readonly int Arguments;
        public readonly MacroTypes MacroType;

        public MacroNode(AdvancedFormatCommand owner, string token) : base(owner, token)
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

        public override string GetValue(BaseFileInfo fileInfo)
        {
            switch (MacroType)
            {
                case MacroTypes.FileName:
                    return fileInfo.IsDirectory
                        ? fileInfo.OriginalName
                        : Path.GetFileNameWithoutExtension(fileInfo.OriginalName);

                case MacroTypes.Extension:
                    return fileInfo.IsDirectory
                        ? string.Empty
                        : Path.GetExtension(fileInfo.OriginalName);

                case MacroTypes.ProceedFileName:
                    return fileInfo.IsDirectory
                        ? fileInfo.ChangedName
                        : fileInfo.ChangedNameWithoutExtension;

                case MacroTypes.ProceedExtension:
                    return fileInfo.IsDirectory
                        ? string.Empty
                        : fileInfo.ChangedNameExtension;

                case MacroTypes.CurrentDirectory:
                    return Path.GetFileName(fileInfo.OriginalPath);

                case MacroTypes.ProceedDirectory:
                    return Path.GetFileName(fileInfo.ChangedPath);

                case MacroTypes.Path:
                    return fileInfo.OriginalPath;

                case MacroTypes.ProceedPath:
                    return fileInfo.ChangedPath;

                case MacroTypes.Index:
                    if (Owner.TryGetTarget(out var owner))
                    {
                        var i = (owner._files.IndexOf(fileInfo) + 1).ToString();
                        return Arguments < 0 ? i : i.PadLeft(Arguments, '0');
                    }

                    return string.Empty;

                case MacroTypes.FoundExtension:
                {
                    using Stream stream = System.IO.File.OpenRead(fileInfo.OriginalFullPath);
                    var detector = DetectorService.DetectDetector(stream);
                    return detector == null
                        ? string.Empty
                        : detector.Extension;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
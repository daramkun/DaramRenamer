using System.ComponentModel;
using System.Globalization;
using System.Text;
using TagLib;
using File = TagLib.File;

namespace DaramRenamer.Commands;

[Serializable]
public class AddMediaTagCommand : ICommand
{
    public MediaTag Tag
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tag)));
        }
    }

    public int Arguments
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Arguments)));
        }
    } = -1;

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

    public int Order => int.MinValue + 4;
    public CommandCategory Category => CommandCategory.Tag;
    
    public void Apply(FileItem item, int _)
    {
        if (item.IsDirectory)
            return;

        TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF8;
        TagLib.Id3v2.Tag.DefaultVersion = 4;

        File f;
        try
        {
            f = File.Create(new File.LocalFileAbstraction(item.SourceFullPath), ReadStyle.PictureLazy);
        }
        catch
        {
            return;
        }

        var tag = "";
        switch (Tag)
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

        tag = ConvertUnicodeText(tag);

        f.Dispose();

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

    // Code idea from https://github.com/mildblue/DaramRenamer/commit/d3a2b71c081bacfc30a3b195280f0d10eff08944
    public static string ConvertUnicodeText(string text)
    {
        var latinStr =
            Encoding.Default.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text.Replace("?", "")));
        return !latinStr.Contains('?') && latinStr.Any(ch =>
            char.GetUnicodeCategory(ch) == UnicodeCategory.OtherLetter)
            ? Encoding.Default.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text))
            : text;
    }
}
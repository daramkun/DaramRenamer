using System.Globalization;
using System.Text.RegularExpressions;
using ATL;
using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Util;
using MetadataDirectory = MetadataExtractor.Directory;
using MetadataTag = MetadataExtractor.Tag;

namespace DaramRenamer.Commands;

internal static partial class MediaTagReader
{
    public static string Read(string path, MediaTag tag, int argument = -1)
    {
        try
        {
            if (MatroskaMediaTagReader.TryRead(path, tag, argument, out var matroskaValue))
                return matroskaValue;

            return IsAudioTag(tag)
                ? ReadAudio(path, tag, argument)
                : ReadVideoOrImage(path, tag, argument);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool IsAudioTag(MediaTag tag) => tag is
        MediaTag.AudioBitrate or
        MediaTag.AudioSamplerate or
        MediaTag.AudioChannels or
        MediaTag.AudioBitsPerSample or
        MediaTag.AudioCodec or
        MediaTag.AudioAlbum or
        MediaTag.AudioAlbumArtists or
        MediaTag.AudioComposers or
        MediaTag.AudioPerformers or
        MediaTag.AudioCopyright or
        MediaTag.AudioDisc or
        MediaTag.AudioDiscCount or
        MediaTag.AudioGenres or
        MediaTag.AudioTitle or
        MediaTag.AudioTrack or
        MediaTag.AudioTrackCount or
        MediaTag.AudioConductor or
        MediaTag.AudioYear or
        MediaTag.AudioDuration;

    private static string ReadAudio(string path, MediaTag tag, int argument)
    {
        var track = new Track(path);
        return tag switch
        {
            MediaTag.AudioAlbum => track.Album ?? string.Empty,
            MediaTag.AudioAlbumArtists => SelectMultiple(track.AlbumArtist, argument),
            MediaTag.AudioComposers => SelectMultiple(track.Composer, argument),
            MediaTag.AudioCopyright => track.Copyright ?? string.Empty,
            MediaTag.AudioDisc => Format(track.DiscNumber),
            MediaTag.AudioDiscCount => Format(track.DiscTotal),
            MediaTag.AudioGenres => SelectMultiple(track.Genre, argument),
            MediaTag.AudioPerformers => SelectMultiple(track.Artist, argument),
            MediaTag.AudioTitle => track.Title ?? string.Empty,
            MediaTag.AudioTrack => Format(track.TrackNumber),
            MediaTag.AudioTrackCount => Format(track.TrackTotal),
            MediaTag.AudioYear => Format(track.Year),
            MediaTag.AudioConductor => track.Conductor ?? string.Empty,
            MediaTag.AudioDuration => FormatDuration(TimeSpan.FromMilliseconds(track.DurationMs)),
            MediaTag.AudioCodec => Format(track.AudioFormat),
            MediaTag.AudioSamplerate => Format(track.SampleRate),
            MediaTag.AudioBitrate => Format(track.Bitrate),
            MediaTag.AudioBitsPerSample => track.BitDepth < 0 ? string.Empty : Format(track.BitDepth),
            MediaTag.AudioChannels => Format(track.ChannelsArrangement?.NbChannels),
            _ => string.Empty
        };
    }

    private static string ReadVideoOrImage(string path, MediaTag tag, int argument)
    {
        var directories = ImageMetadataReader.ReadMetadata(path);
        return tag switch
        {
            MediaTag.VideoTitle => SelectMultiple(ReadTag(directories, "Title", "Movie Title", "Content Name"), argument),
            MediaTag.VideoDuration => ReadDuration(directories),
            MediaTag.VideoWidth => ReadDimension(directories, "Video Width", "Width", "Image Width"),
            MediaTag.VideoHeight => ReadDimension(directories, "Video Height", "Height", "Image Height"),
            MediaTag.VideoCodec => ReadTag(directories, "Video Codec", "Compressor Name", "Compression Type", "Codec", "Compressor"),
            MediaTag.VideoGenres => SelectMultiple(ReadTag(directories, "Genre"), argument),
            MediaTag.VideoYear => ReadYear(directories),
            MediaTag.VideoCopyright => ReadTag(directories, "Copyright", "Copyright Notice", "Rights"),
            MediaTag.ImageWidth => ReadDimension(directories, "Image Width", "Width"),
            MediaTag.ImageHeight => ReadDimension(directories, "Image Height", "Height"),
            MediaTag.ImageCodec => ReadImageCodec(path),
            // MetadataExtractor does not expose a format-specific image quality indicator.
            MediaTag.ImageQuality => string.Empty,
            _ => string.Empty
        };
    }

    private static string ReadImageCodec(string path)
    {
        using var stream = File.OpenRead(path);
        return FileTypeDetector.DetectFileType(stream, path).GetName();
    }

    private static string ReadTag(IEnumerable<MetadataDirectory> directories, params string[] names)
    {
        var normalizedNames = names.Select(NormalizeName).ToHashSet(StringComparer.Ordinal);
        foreach (var directory in directories)
        foreach (var tag in directory.Tags)
        {
            if (!normalizedNames.Contains(NormalizeName(tag.Name)))
                continue;

            var description = tag.Description;
            if (!string.IsNullOrWhiteSpace(description))
                return description;

            var value = directory.GetObject(tag.Type);
            if (value is not null)
                return Format(value);
        }

        return string.Empty;
    }

    private static string ReadDimension(IEnumerable<MetadataDirectory> directories, params string[] names)
    {
        var normalizedNames = names.Select(NormalizeName).ToHashSet(StringComparer.Ordinal);
        foreach (var directory in directories)
        foreach (var tag in directory.Tags)
        {
            if (!normalizedNames.Contains(NormalizeName(tag.Name)))
                continue;

            var description = tag.Description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                var match = NumberRegex().Match(description);
                if (match.Success)
                    return match.Value.Replace(",", string.Empty, StringComparison.Ordinal);
            }

            var value = directory.GetObject(tag.Type);
            if (value is not null)
                return Format(value);
        }

        return string.Empty;
    }

    private static string ReadDuration(IReadOnlyList<MetadataDirectory> directories)
    {
        var movieHeader = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
        if (movieHeader is not null)
        {
            var duration = ToDouble(movieHeader.GetObject(QuickTimeMovieHeaderDirectory.TagDuration));
            var timeScale = ToDouble(movieHeader.GetObject(QuickTimeMovieHeaderDirectory.TagTimeScale));
            if (duration is > 0 && timeScale is > 0)
                return FormatDuration(TimeSpan.FromSeconds(duration.Value / timeScale.Value));
        }

        var durationTag = FindTag(directories, "Duration", "Media Duration", "Track Duration");
        if (durationTag is null)
            return string.Empty;

        var tagValue = durationTag.Value.Value;
        if (tagValue is TimeSpan timeSpan)
            return FormatDuration(timeSpan);

        var description = durationTag.Value.Tag.Description ?? Format(tagValue);
        if (TimeSpan.TryParse(description, CultureInfo.InvariantCulture, out timeSpan))
            return FormatDuration(timeSpan);

        var match = DurationRegex().Match(description);
        if (!match.Success || !double.TryParse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return string.Empty;

        var unit = match.Groups["unit"].Value;
        var durationValue = unit.StartsWith("ms", StringComparison.OrdinalIgnoreCase)
            ? TimeSpan.FromMilliseconds(value)
            : TimeSpan.FromSeconds(value);
        return FormatDuration(durationValue);
    }

    private static (MetadataTag Tag, object? Value)? FindTag(IEnumerable<MetadataDirectory> directories, params string[] names)
    {
        var normalizedNames = names.Select(NormalizeName).ToHashSet(StringComparer.Ordinal);
        foreach (var directory in directories)
        foreach (var tag in directory.Tags)
        {
            if (normalizedNames.Contains(NormalizeName(tag.Name)))
                return (tag, directory.GetObject(tag.Type));
        }

        return null;
    }

    private static string ReadYear(IEnumerable<MetadataDirectory> directories)
    {
        var value = ReadTag(directories, "Year", "Release Year", "Creation Date", "Creation Time", "Create Date", "Date Created");
        var match = YearRegex().Match(value);
        return match.Success ? match.Value : string.Empty;
    }

    private static string SelectMultiple(string? value, int argument)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var values = value.Split(ATL.Settings.DisplayValueSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (argument >= 0)
            return argument < values.Length ? values[argument] : string.Empty;

        return string.Join(',', values);
    }

    private static string Format(object? value) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatDuration(TimeSpan duration) => duration.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture);

    private static double? ToDouble(object? value)
    {
        if (value is null)
            return null;

        try
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeName(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    [GeneratedRegex(@"-?[\d,]+(?:\.\d+)?", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"(?<value>[\d.,]+)\s*(?<unit>milliseconds?|ms|seconds?|secs?|s)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"\b(?:18|19|20|21)\d{2}\b", RegexOptions.CultureInvariant)]
    private static partial Regex YearRegex();
}

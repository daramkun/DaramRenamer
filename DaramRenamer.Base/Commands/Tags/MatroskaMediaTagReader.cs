using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace DaramRenamer.Commands;

/// <summary>
/// Reads container-level metadata from Matroska and WebM files without loading a codec or native library.
/// </summary>
internal static class MatroskaMediaTagReader
{
    private const uint EbmlHeaderId = 0x1A45DFA3;
    private const uint SegmentId = 0x18538067;
    private const uint InfoId = 0x1549A966;
    private const uint TracksId = 0x1654AE6B;
    private const uint TagsId = 0x1254C367;
    private const uint ClusterId = 0x1F43B675;

    private const uint DocTypeId = 0x4282;
    private const uint TimestampScaleId = 0x2AD7B1;
    private const uint DurationId = 0x4489;
    private const uint SegmentTitleId = 0x7BA9;
    private const uint TrackEntryId = 0xAE;
    private const uint TrackTypeId = 0x83;
    private const uint FlagDefaultId = 0x88;
    private const uint CodecIdId = 0x86;
    private const uint CodecNameId = 0x258688;
    private const uint VideoId = 0xE0;
    private const uint PixelWidthId = 0xB0;
    private const uint PixelHeightId = 0xBA;
    private const uint TagId = 0x7373;
    private const uint SimpleTagId = 0x67C8;
    private const uint TagNameId = 0x45A3;
    private const uint TagStringId = 0x4487;

    private const int MaximumStringSize = 1024 * 1024;
    private const int MaximumSimpleTagDepth = 16;

    private static readonly HashSet<uint> SegmentLevelIds =
    [
        0x114D9B74, // SeekHead
        InfoId,
        TracksId,
        ClusterId,
        0x1C53BB6B, // Cues
        0x1941A469, // Attachments
        0x1043A770, // Chapters
        TagsId
    ];

    public static bool TryRead(string path, MediaTag tag, int argument, out string value)
    {
        value = string.Empty;
        if (!IsVideoTag(tag))
            return false;

        try
        {
            using var stream = File.OpenRead(path);
            var reader = new EbmlReader(stream);
            var documentType = ReadDocumentType(reader);
            if (documentType is not ("matroska" or "webm"))
                return false;

            var metadata = new MatroskaMetadata();
            ReadDocument(reader, metadata);
            value = metadata.GetValue(tag, argument);
            return true;
        }
        catch
        {
            // Invalid, truncated, or unsupported EBML is left to the existing metadata readers.
            return false;
        }
    }

    private static bool IsVideoTag(MediaTag tag) => tag is
        MediaTag.VideoTitle or
        MediaTag.VideoDuration or
        MediaTag.VideoWidth or
        MediaTag.VideoHeight or
        MediaTag.VideoCodec or
        MediaTag.VideoGenres or
        MediaTag.VideoYear or
        MediaTag.VideoCopyright;

    private static string? ReadDocumentType(EbmlReader reader)
    {
        var documentEnd = reader.Stream.Length;
        if (!reader.TryReadElement(documentEnd, out var header) || header.Id != EbmlHeaderId || header.IsUnknownSize)
            return null;

        string? documentType = null;
        while (reader.Stream.Position < header.End)
        {
            var child = reader.ReadElement(header.End);
            if (child.Id == DocTypeId)
                documentType = reader.ReadString(child);
            else
                reader.Skip(child);
        }

        return documentType;
    }

    private static void ReadDocument(EbmlReader reader, MatroskaMetadata metadata)
    {
        var documentEnd = reader.Stream.Length;
        while (reader.Stream.Position < documentEnd)
        {
            var element = reader.ReadElement(documentEnd);
            if (element.Id == SegmentId)
            {
                ReadSegment(reader, element.IsUnknownSize ? documentEnd : element.End, metadata);
                return;
            }

            reader.Skip(element);
        }
    }

    private static void ReadSegment(EbmlReader reader, long segmentEnd, MatroskaMetadata metadata)
    {
        while (reader.Stream.Position < segmentEnd)
        {
            var element = reader.ReadElement(segmentEnd);
            if (element.Id == ClusterId && element.IsUnknownSize)
            {
                SkipUnknownSizeCluster(reader, segmentEnd);
                continue;
            }

            if (element.IsUnknownSize)
                throw new InvalidDataException("Only the Segment and Cluster elements may have an unknown size here.");

            switch (element.Id)
            {
                case InfoId:
                    ReadInfo(reader, element, metadata);
                    break;
                case TracksId:
                    ReadTracks(reader, element, metadata);
                    break;
                case TagsId:
                    ReadTags(reader, element, metadata);
                    break;
                default:
                    reader.Skip(element);
                    break;
            }
        }
    }

    private static void SkipUnknownSizeCluster(EbmlReader reader, long segmentEnd)
    {
        while (reader.Stream.Position < segmentEnd)
        {
            if (!reader.TryReadElement(segmentEnd, out var child))
                return;

            if (SegmentLevelIds.Contains(child.Id))
            {
                reader.Stream.Position = child.HeaderOffset;
                return;
            }

            if (child.IsUnknownSize)
                throw new InvalidDataException("Unexpected unknown-sized element inside a Cluster.");

            reader.Skip(child);
        }
    }

    private static void ReadInfo(EbmlReader reader, EbmlElement info, MatroskaMetadata metadata)
    {
        while (reader.Stream.Position < info.End)
        {
            var element = reader.ReadElement(info.End);
            switch (element.Id)
            {
                case TimestampScaleId:
                    metadata.TimestampScale = reader.ReadUnsignedInteger(element);
                    break;
                case DurationId:
                    metadata.Duration = reader.ReadFloatingPoint(element);
                    break;
                case SegmentTitleId:
                    metadata.SegmentTitle = reader.ReadString(element);
                    break;
                default:
                    reader.Skip(element);
                    break;
            }
        }
    }

    private static void ReadTracks(EbmlReader reader, EbmlElement tracks, MatroskaMetadata metadata)
    {
        while (reader.Stream.Position < tracks.End)
        {
            var element = reader.ReadElement(tracks.End);
            if (element.Id == TrackEntryId)
                metadata.VideoTracks.Add(ReadTrack(reader, element));
            else
                reader.Skip(element);
        }
    }

    private static VideoTrack ReadTrack(EbmlReader reader, EbmlElement trackEntry)
    {
        var track = new VideoTrack();
        ulong trackType = 0;

        while (reader.Stream.Position < trackEntry.End)
        {
            var element = reader.ReadElement(trackEntry.End);
            switch (element.Id)
            {
                case TrackTypeId:
                    trackType = reader.ReadUnsignedInteger(element);
                    break;
                case FlagDefaultId:
                    track.IsDefault = reader.ReadUnsignedInteger(element) != 0;
                    break;
                case CodecIdId:
                    track.CodecId = reader.ReadString(element);
                    break;
                case CodecNameId:
                    track.CodecName = reader.ReadString(element);
                    break;
                case VideoId:
                    ReadVideoTrack(reader, element, track);
                    break;
                default:
                    reader.Skip(element);
                    break;
            }
        }

        track.IsVideo = trackType == 1;
        return track;
    }

    private static void ReadVideoTrack(EbmlReader reader, EbmlElement video, VideoTrack track)
    {
        while (reader.Stream.Position < video.End)
        {
            var element = reader.ReadElement(video.End);
            switch (element.Id)
            {
                case PixelWidthId:
                    track.Width = reader.ReadUnsignedInteger(element);
                    break;
                case PixelHeightId:
                    track.Height = reader.ReadUnsignedInteger(element);
                    break;
                default:
                    reader.Skip(element);
                    break;
            }
        }
    }

    private static void ReadTags(EbmlReader reader, EbmlElement tags, MatroskaMetadata metadata)
    {
        while (reader.Stream.Position < tags.End)
        {
            var element = reader.ReadElement(tags.End);
            if (element.Id == TagId)
                ReadTag(reader, element, metadata);
            else
                reader.Skip(element);
        }
    }

    private static void ReadTag(EbmlReader reader, EbmlElement tag, MatroskaMetadata metadata)
    {
        while (reader.Stream.Position < tag.End)
        {
            var element = reader.ReadElement(tag.End);
            if (element.Id == SimpleTagId)
                AddSimpleTagValues(ReadSimpleTag(reader, element, 0), metadata);
            else
                reader.Skip(element);
        }
    }

    private static SimpleTag ReadSimpleTag(EbmlReader reader, EbmlElement simpleTag, int depth)
    {
        if (depth >= MaximumSimpleTagDepth)
        {
            reader.Skip(simpleTag);
            return new SimpleTag();
        }

        var result = new SimpleTag();
        while (reader.Stream.Position < simpleTag.End)
        {
            var element = reader.ReadElement(simpleTag.End);
            switch (element.Id)
            {
                case TagNameId:
                    result.Name = reader.ReadString(element);
                    break;
                case TagStringId:
                    result.Value = reader.ReadString(element);
                    break;
                case SimpleTagId:
                    result.Children.Add(ReadSimpleTag(reader, element, depth + 1));
                    break;
                default:
                    reader.Skip(element);
                    break;
            }
        }

        return result;
    }

    private static void AddSimpleTagValues(SimpleTag tag, MatroskaMetadata metadata)
    {
        if (!string.IsNullOrWhiteSpace(tag.Name) && !string.IsNullOrWhiteSpace(tag.Value))
            metadata.AddTag(tag.Name, tag.Value);

        foreach (var child in tag.Children)
            AddSimpleTagValues(child, metadata);
    }

    private sealed class MatroskaMetadata
    {
        private readonly Dictionary<string, List<string>> _tags = new(StringComparer.Ordinal);

        public ulong TimestampScale { get; set; } = 1_000_000;
        public double? Duration { get; set; }
        public string? SegmentTitle { get; set; }
        public List<VideoTrack> VideoTracks { get; } = [];

        public void AddTag(string name, string value)
        {
            var normalizedName = NormalizeName(name);
            if (!_tags.TryGetValue(normalizedName, out var values))
                _tags.Add(normalizedName, values = []);
            values.Add(value);
        }

        public string GetValue(MediaTag tag, int argument) => tag switch
        {
            MediaTag.VideoTitle => SelectMultiple(
                !string.IsNullOrWhiteSpace(SegmentTitle) ? SegmentTitle : GetJoinedTags("TITLE"), argument),
            MediaTag.VideoDuration => GetDuration(),
            MediaTag.VideoWidth => FormatTrackValue(track => track.Width),
            MediaTag.VideoHeight => FormatTrackValue(track => track.Height),
            MediaTag.VideoCodec => GetVideoCodec(),
            MediaTag.VideoGenres => SelectMultiple(GetJoinedTags("GENRE"), argument),
            MediaTag.VideoYear => GetYear(),
            MediaTag.VideoCopyright => GetFirstTag("COPYRIGHT"),
            _ => string.Empty
        };

        private string GetDuration()
        {
            if (Duration is not { } durationValue || !double.IsFinite(durationValue) || durationValue <= 0 || TimestampScale == 0)
                return string.Empty;

            var seconds = durationValue * TimestampScale / 1_000_000_000d;
            if (!double.IsFinite(seconds) || seconds > TimeSpan.MaxValue.TotalSeconds)
                return string.Empty;

            var timeSpan = TimeSpan.FromSeconds(seconds);
            return ((long)timeSpan.TotalHours).ToString("00", CultureInfo.InvariantCulture) +
                   timeSpan.ToString("\\:mm\\:ss", CultureInfo.InvariantCulture);
        }

        private string FormatTrackValue(Func<VideoTrack, ulong> selector)
        {
            var selectedTrack = GetSelectedTrack();
            var value = selectedTrack is null ? 0 : selector(selectedTrack);
            if (value == 0)
                value = VideoTracks.Where(track => track.IsVideo).Select(selector).FirstOrDefault(candidate => candidate > 0);
            return value > 0 && value <= int.MaxValue ? value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private string GetVideoCodec()
        {
            var selectedTrack = GetSelectedTrack();
            var codec = selectedTrack is null ? null : FirstNonEmpty(selectedTrack.CodecName, selectedTrack.CodecId);
            if (string.IsNullOrWhiteSpace(codec))
            {
                codec = VideoTracks.Where(track => track.IsVideo)
                    .Select(track => FirstNonEmpty(track.CodecName, track.CodecId))
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            }

            return codec switch
            {
                "V_MPEG4/ISO/AVC" => "H.264/AVC",
                "V_MPEGH/ISO/HEVC" => "H.265/HEVC",
                "V_AV1" => "AV1",
                "V_VP8" => "VP8",
                "V_VP9" => "VP9",
                "V_MPEG2" => "MPEG-2 Video",
                _ => codec ?? string.Empty
            };
        }

        private string GetYear()
        {
            foreach (var value in GetTagValues("DATE_RELEASED", "DATE_RECORDED", "DATE_TAGGED", "YEAR"))
            {
                for (var index = 0; index <= value.Length - 4; index++)
                {
                    if (!char.IsAsciiDigit(value[index]) || !char.IsAsciiDigit(value[index + 1]) ||
                        !char.IsAsciiDigit(value[index + 2]) || !char.IsAsciiDigit(value[index + 3]))
                        continue;

                    var year = value.AsSpan(index, 4);
                    if ((year[0] is '1' or '2') && int.TryParse(year, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedYear) &&
                        parsedYear is >= 1800 and <= 2199)
                        return year.ToString();
                }
            }

            return string.Empty;
        }

        private string GetJoinedTags(string name) =>
            _tags.TryGetValue(NormalizeName(name), out var values)
                ? string.Join(ATL.Settings.DisplayValueSeparator.ToString(), values)
                : string.Empty;

        private string GetFirstTag(string name) =>
            _tags.TryGetValue(NormalizeName(name), out var values) ? values.FirstOrDefault() ?? string.Empty : string.Empty;

        private IEnumerable<string> GetTagValues(params string[] names) =>
            names.Select(NormalizeName).Where(_tags.ContainsKey).SelectMany(name => _tags[name]);

        private VideoTrack? GetSelectedTrack() =>
            VideoTracks.FirstOrDefault(track => track.IsVideo && track.IsDefault) ??
            VideoTracks.FirstOrDefault(track => track.IsVideo);

        private static string SelectMultiple(string? value, int argument)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var values = value.Split(
                ATL.Settings.DisplayValueSeparator,
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return argument >= 0
                ? argument < values.Length ? values[argument] : string.Empty
                : string.Join(',', values);
        }

        private static string NormalizeName(string value) =>
            new(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

        private static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private sealed class VideoTrack
    {
        public bool IsVideo { get; set; }
        public bool IsDefault { get; set; } = true;
        public string? CodecId { get; set; }
        public string? CodecName { get; set; }
        public ulong Width { get; set; }
        public ulong Height { get; set; }
    }

    private sealed class SimpleTag
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public List<SimpleTag> Children { get; } = [];
    }

    private readonly record struct EbmlElement(uint Id, long HeaderOffset, long DataOffset, long End, bool IsUnknownSize);

    private sealed class EbmlReader(Stream stream)
    {
        public Stream Stream { get; } = stream;

        public bool TryReadElement(long parentEnd, out EbmlElement element)
        {
            var headerOffset = Stream.Position;
            var firstByte = Stream.ReadByte();
            if (firstByte < 0)
            {
                element = default;
                return false;
            }

            var idLength = GetVIntLength((byte)firstByte);
            if (idLength > 4)
                throw new InvalidDataException("An EBML element ID cannot be longer than four bytes.");

            uint id = (byte)firstByte;
            for (var index = 1; index < idLength; index++)
                id = (id << 8) | ReadRequiredByte();

            var firstSizeByte = ReadRequiredByte();
            var sizeLength = GetVIntLength(firstSizeByte);
            var size = (ulong)(firstSizeByte & (0xFF >> sizeLength));
            for (var index = 1; index < sizeLength; index++)
                size = (size << 8) | ReadRequiredByte();

            var maximumSize = ulong.MaxValue >> (64 - sizeLength * 7);
            var isUnknownSize = size == maximumSize;
            var dataOffset = Stream.Position;
            var end = isUnknownSize ? parentEnd : checked(dataOffset + checked((long)size));
            if (end > parentEnd || end > Stream.Length)
                throw new InvalidDataException("An EBML element extends beyond its parent or file.");

            element = new EbmlElement(id, headerOffset, dataOffset, end, isUnknownSize);
            return true;
        }

        public EbmlElement ReadElement(long parentEnd) =>
            TryReadElement(parentEnd, out var element)
                ? element
                : throw new EndOfStreamException("Unexpected end of the EBML document.");

        public void Skip(EbmlElement element)
        {
            if (element.IsUnknownSize)
                throw new InvalidDataException("Cannot skip an unknown-sized EBML element.");
            Stream.Position = element.End;
        }

        public string ReadString(EbmlElement element)
        {
            if (element.IsUnknownSize)
                throw new InvalidDataException("An EBML string cannot have an unknown size.");

            var length = element.End - Stream.Position;
            if (length > MaximumStringSize)
            {
                Skip(element);
                return string.Empty;
            }

            var bytes = new byte[checked((int)length)];
            Stream.ReadExactly(bytes);
            return Encoding.UTF8.GetString(bytes).TrimEnd('\0');
        }

        public ulong ReadUnsignedInteger(EbmlElement element)
        {
            var length = element.End - Stream.Position;
            if (element.IsUnknownSize || length is < 1 or > sizeof(ulong))
                throw new InvalidDataException("Invalid EBML unsigned integer size.");

            Span<byte> bytes = stackalloc byte[sizeof(ulong)];
            bytes.Clear();
            Stream.ReadExactly(bytes[(sizeof(ulong) - (int)length)..]);
            return BinaryPrimitives.ReadUInt64BigEndian(bytes);
        }

        public double? ReadFloatingPoint(EbmlElement element)
        {
            var length = element.End - Stream.Position;
            if (element.IsUnknownSize)
                throw new InvalidDataException("An EBML float cannot have an unknown size.");

            Span<byte> bytes = stackalloc byte[sizeof(double)];
            if (length == sizeof(float))
            {
                Stream.ReadExactly(bytes[..sizeof(float)]);
                return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(bytes[..sizeof(float)]));
            }

            if (length == sizeof(double))
            {
                Stream.ReadExactly(bytes);
                return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(bytes));
            }

            Skip(element);
            return null;
        }

        private byte ReadRequiredByte()
        {
            var value = Stream.ReadByte();
            return value < 0 ? throw new EndOfStreamException("Unexpected end of EBML element header.") : (byte)value;
        }

        private static int GetVIntLength(byte firstByte)
        {
            if (firstByte == 0)
                throw new InvalidDataException("Invalid zero-prefixed EBML variable integer.");

            var marker = 0x80;
            var length = 1;
            while ((firstByte & marker) == 0)
            {
                marker >>= 1;
                length++;
            }

            if (length > 8)
                throw new InvalidDataException("Invalid EBML variable integer length.");
            return length;
        }
    }
}

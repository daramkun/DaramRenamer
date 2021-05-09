using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DaramRenamer.Commands.Tags
{
	[Serializable, LocalizationKey("Command_Name_AddMediaTag")]
	public class AddMediaTagCommand : ICommand
	{
		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Tag;

		[LocalizationKey("Command_Argument_AddMediaTag_Tag")]
		public MediaTag Tag { get; set; }
		[LocalizationKey("Command_Argument_AddMediaTag_Arguments")]
		public int Arguments { get; set; } = -1;
		[LocalizationKey("Command_Argument_AddMediaTag_Position")]
		public Position1 Position { get; set; } = Position1.EndPoint;

		public bool DoCommand(FileInfo file)
		{
			TagLib.Id3v2.Tag.DefaultEncoding = TagLib.StringType.UTF8;
			TagLib.Id3v2.Tag.DefaultVersion = 4;

			TagLib.File f;
			try
			{
				f = TagLib.File.Create(new TagLib.File.LocalFileAbstraction(file.OriginalFullPath), TagLib.ReadStyle.PictureLazy);
			}
			catch { return false; }

			var tag = "";
			switch (Tag)
			{
				case MediaTag.AudioAlbum: tag = f.Tag.Album; break;
				case MediaTag.AudioAlbumArtists: tag = (Arguments != -1) ? f.Tag.AlbumArtists[Arguments] : string.Join(",", f.Tag.AlbumArtists); break;
				case MediaTag.AudioComposers: tag = (Arguments != -1) ? f.Tag.Composers[Arguments] : string.Join(",", f.Tag.Composers); break;
				case MediaTag.AudioCopyright:
				case MediaTag.VideoCopyright: tag = f.Tag.Copyright; break;
				case MediaTag.AudioDisc: tag = f.Tag.Disc.ToString(); break;
				case MediaTag.AudioDiscCount: tag = f.Tag.DiscCount.ToString(); break;
				case MediaTag.AudioGenres:
				case MediaTag.VideoGenres: tag = (Arguments != -1) ? f.Tag.Genres[Arguments] : string.Join(",", f.Tag.Genres); break;
				case MediaTag.AudioPerformers: tag = (Arguments != -1) ? f.Tag.Performers[Arguments] : string.Join(",", f.Tag.Performers); break;
				case MediaTag.AudioTitle:
				case MediaTag.VideoTitle: tag = f.Tag.Title; break;
				case MediaTag.AudioTrack: tag = f.Tag.Track.ToString(); break;
				case MediaTag.AudioTrackCount: tag = f.Tag.TrackCount.ToString(); break;
				case MediaTag.AudioYear:
				case MediaTag.VideoYear: tag = f.Tag.Year.ToString(); break;
				case MediaTag.AudioConductor: tag = f.Tag.Conductor; break;
				case MediaTag.AudioDuration:
				case MediaTag.VideoDuration: tag = f.Properties.Duration.ToString("hh:mm:ss"); break;
				case MediaTag.AudioCodec:
					foreach (var codec in f.Properties.Codecs)
						if (codec.MediaTypes == TagLib.MediaTypes.Audio)
							tag = codec.Description;
					break;
				case MediaTag.AudioSamplerate: tag = f.Properties.AudioSampleRate.ToString(); break;
				case MediaTag.AudioBitrate: tag = f.Properties.AudioBitrate.ToString(); break;
				case MediaTag.AudioBitsPerSample: tag = f.Properties.BitsPerSample.ToString(); break;
				case MediaTag.AudioChannels: tag = f.Properties.AudioChannels.ToString(); break;

				case MediaTag.ImageWidth: tag = f.Properties.PhotoWidth.ToString(); break;
				case MediaTag.ImageHeight: tag = f.Properties.PhotoHeight.ToString(); break;
				case MediaTag.ImageQuality: tag = f.Properties.PhotoQuality.ToString(); break;
				case MediaTag.ImageCodec:
					foreach (var codec in f.Properties.Codecs)
						if (codec.MediaTypes == TagLib.MediaTypes.Audio)
							tag = codec.Description;
					break;

				case MediaTag.VideoWidth: tag = f.Properties.VideoWidth.ToString(); break;
				case MediaTag.VideoHeight: tag = f.Properties.VideoHeight.ToString(); break;
				case MediaTag.VideoCodec:
					foreach (var codec in f.Properties.Codecs)
						if (codec.MediaTypes == TagLib.MediaTypes.Video)
							tag = codec.Description;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			tag = ConvertUnicodeText(tag);

			f.Dispose();

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

		// Code idea from https://github.com/mildblue/DaramRenamer/commit/d3a2b71c081bacfc30a3b195280f0d10eff08944
		private string ConvertUnicodeText(string text)
		{
			var latinStr =
				Encoding.Default.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text.Replace("?", "")));
			return !latinStr.Contains('?') && latinStr.Any(ch =>
				char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.OtherLetter)
				? Encoding.Default.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text))
				: text;
		}
	}
}

using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Tag
{
	[Serializable]
	public class AddMediaTagProcessor : IProcessor
	{
		public string Name => "process_add_media_tag";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "media_tag_type" )]
		public MediaTag Tag { get; set; }
		[Argument ( Name = "media_tag_args" )]
		public int Arguments { get; set; } = -1;
		[Argument ( Name = "media_tag_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;
		
		public bool Process ( FileInfo file )
		{
			TagLib.File f = null;
			try
			{
				f = TagLib.File.Create ( new TagLib.File.LocalFileAbstraction ( file.OriginalFullPath ) );
			}
			catch { return false; }

			string tag = "";
			switch ( Tag )
			{
				case MediaTag.AudioAlbum: tag = f.Tag.Album; break;
				case MediaTag.AudioAlbumArtists: tag = ( Arguments != -1 ) ? f.Tag.AlbumArtists [ Arguments ] : string.Join ( ",", f.Tag.AlbumArtists ); break;
				case MediaTag.AudioComposers: tag = ( Arguments != -1 ) ? f.Tag.Composers [ Arguments ] : string.Join ( ",", f.Tag.Composers ); break;
				case MediaTag.AudioCopyright:
				case MediaTag.VideoCopyright: tag = f.Tag.Copyright; break;
				case MediaTag.AudioDisc: tag = f.Tag.Disc.ToString (); break;
				case MediaTag.AudioDiscCount: tag = f.Tag.DiscCount.ToString (); break;
				case MediaTag.AudioGenres:
				case MediaTag.VideoGenres: tag = ( Arguments != -1 ) ? f.Tag.Genres [ Arguments ] : string.Join ( ",", f.Tag.Genres ); break;
				case MediaTag.AudioPerformers: tag = ( Arguments != -1 ) ? f.Tag.Performers [ Arguments ] : string.Join ( ",", f.Tag.Performers ); break;
				case MediaTag.AudioTitle:
				case MediaTag.VideoTitle: tag = f.Tag.Title; break;
				case MediaTag.AudioTrack: tag = f.Tag.Track.ToString (); break;
				case MediaTag.AudioTrackCount: tag = f.Tag.TrackCount.ToString (); break;
				case MediaTag.AudioYear:
				case MediaTag.VideoYear: tag = f.Tag.Year.ToString (); break;
				case MediaTag.AudioDuration:
				case MediaTag.VideoDuration: tag = f.Properties.Duration.ToString ( "hh:mm:ss" ); break;
				case MediaTag.AudioCodec:
					foreach ( var codec in f.Properties.Codecs )
						if ( codec.MediaTypes == TagLib.MediaTypes.Audio )
							tag = codec.Description;
					break;
				case MediaTag.AudioSamplerate: tag = f.Properties.AudioSampleRate.ToString (); break;
				case MediaTag.AudioBitrate: tag = f.Properties.AudioBitrate.ToString (); break;
				case MediaTag.AudioBitsPerSample: tag = f.Properties.BitsPerSample.ToString (); break;
				case MediaTag.AudioChannels: tag = f.Properties.AudioChannels.ToString (); break;

				case MediaTag.ImageWidth: tag = f.Properties.PhotoWidth.ToString (); break;
				case MediaTag.ImageHeight: tag = f.Properties.PhotoHeight.ToString (); break;
				case MediaTag.ImageQuality: tag = f.Properties.PhotoQuality.ToString (); break;
				case MediaTag.ImageCodec:
					foreach ( var codec in f.Properties.Codecs )
						if ( codec.MediaTypes == TagLib.MediaTypes.Audio )
							tag = codec.Description;
					break;

				case MediaTag.VideoWidth: tag = f.Properties.VideoWidth.ToString (); break;
				case MediaTag.VideoHeight: tag = f.Properties.VideoHeight.ToString (); break;
				case MediaTag.VideoCodec:
					foreach ( var codec in f.Properties.Codecs )
						if ( codec.MediaTypes == TagLib.MediaTypes.Video )
							tag = codec.Description;
					break;
			}

			f.Dispose ();

			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			file.ChangedFilename = Position == OnePointPosition.StartPoint ? $"{tag}{fn}{ext}" :
				( Position == OnePointPosition.EndPoint ? $"{fn}{tag}{ext}" :
				$"{tag}{fn}{tag}{ext}" );

			return true;
		}
	}
}

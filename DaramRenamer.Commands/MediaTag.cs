using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public enum MediaTag
	{
		AudioBitrate, AudioSamplerate, AudioChannels, AudioBitsPerSample,
		AudioCodec, AudioAlbum, AudioAlbumArtists, AudioComposers,
		AudioPerformers, AudioCopyright, AudioDisc, AudioDiscCount,
		AudioGenres, AudioTitle, AudioTrack, AudioTrackCount,

		AudioConductor, AudioYear, AudioDuration,

		VideoTitle, VideoDuration, VideoWidth, VideoHeight,
		VideoCodec, VideoGenres, VideoYear, VideoCopyright,

		ImageWidth, ImageHeight, ImageCodec, ImageQuality,
	}
}

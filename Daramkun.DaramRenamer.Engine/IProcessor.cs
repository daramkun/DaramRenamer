using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public enum Position
	{
		StartPoint,
		EndPoint,
		BothPoint,
	}
	public enum OnePointPosition
	{
		StartPoint,
		EndPoint,
	}
	public enum Casecast
	{
		AllToUppercase,
		AllToLowercase,
		UppercaseFirstLetter,
	}
	public enum CasecastBW
	{
		AllToUppercase,
		AllToLowercase,
	}
	public enum DateType
	{
		CreationDate,
		ModifiedDate,
		AccessedDate,
		Now,
	}
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
	public enum DocumentTag
	{
		Title,
		Author,
	}
	public enum HashType
	{
		MD5,
		SHA1,
		SHA256,
		SHA384,
		SHA512,
	}

	public interface IBatchable
	{
		string Name { get; }
	}

	public interface ICondition : IBatchable
	{
		bool IsValid ( FileInfo file );
	}

	public interface IProcessor : IBatchable
	{
		bool CannotMultithreadProcess { get; }
		bool Process ( FileInfo file );
	}
}

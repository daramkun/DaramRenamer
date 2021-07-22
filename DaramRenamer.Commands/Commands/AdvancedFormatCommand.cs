using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DaramRenamer.Commands.Tags;

namespace DaramRenamer.Commands
{
	[Serializable, LocalizationKey("Command_Name_AdvancedFormat")]
	class AdvancedFormatCommand : ICommand, IOrderBy
	{
		public int Order => 1;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Etc;

		private string _fileNameFormatString = string.Empty;
		private List<FormatStringNode> _fileNameNodes = new ();
		private string _pathFormatString = string.Empty;
		private List<FormatStringNode> _pathNodes = new ();

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

		public bool DoCommand(FileInfo file)
		{
			var builder = new StringBuilder();

			foreach (var node in _fileNameNodes)
				builder.Append(node.GetValue(file));

			file.ChangedFilename = builder.ToString();

			builder.Clear();

			foreach (var node in _pathNodes)
				builder.Append(node.GetValue(file));

			file.ChangedPath = builder.ToString();

			return true;
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

			if (token.Length > 0)
			{
				nodes.Add(new StringNode(token.ToString()));
			}
		}

		[Serializable]
		abstract class FormatStringNode
		{

			public readonly string Token;

			public FormatStringNode(string token) => Token = token;

			public abstract string GetValue(FileInfo fileInfo);
		}

		[Serializable]
		class StringNode : FormatStringNode
		{
			public StringNode(string token) : base(token) { }

			public override string GetValue(FileInfo fileInfo) => Token;
		}

		[Serializable]
		class EnvironmentVariableNode : FormatStringNode
		{
			public readonly string VariableName;
			public readonly EnvironmentVariableTarget VariableTarget;

			public static bool IsEnvironmentVariable(string token) => token.StartsWith("env:");

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

			public override string GetValue(FileInfo fileInfo) => Environment.GetEnvironmentVariable(VariableName, VariableTarget);
		}

		[Serializable]
		class MediaTagNode : FormatStringNode
		{
			public readonly MediaTag TagType;
			public readonly int Arguments;

			public static bool IsMediaTag(string token) => token.StartsWith("media:");

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

			public override string GetValue(FileInfo fileInfo)
			{
				TagLib.Id3v2.Tag.DefaultEncoding = TagLib.StringType.UTF8;
                TagLib.Id3v2.Tag.DefaultVersion = 4;

                var f = TagLib.File.Create(new TagLib.File.LocalFileAbstraction(fileInfo.OriginalFullPath), TagLib.ReadStyle.PictureLazy);

                var tag = "";
                switch (TagType)
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

                f.Dispose();

                return AddMediaTagCommand.ConvertUnicodeText(tag);
			}
		}

		[Serializable]
		class DocumentTagNode : FormatStringNode
		{
			public readonly DocumentTag TagType;
			public readonly int Arguments;

			public static bool IsDocumentTag(string token) => token.StartsWith("doc:");

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

			public override string GetValue(FileInfo fileInfo)
			{
				AddDocumentTagCommand.File f = new (fileInfo.OriginalFullPath);

				return TagType switch
				{
					DocumentTag.Title => f.Title,
					DocumentTag.Author => f.Author,
					_ => ""
				};
			}
		}

		[Serializable]
		class HashNode : FormatStringNode
		{
			public readonly HashType HashType;

			public static bool IsHash(string token) => token.StartsWith("hash:");

			public HashNode(string token) : base(token)
			{
				var identifier = token.IndexOf(':');
				HashType = Enum.Parse<HashType>(token[(identifier + 1)..]);
			}

			public override string GetValue(FileInfo fileInfo)
			{
				return AddHashCommand.ComputeHash(HashType, fileInfo.OriginalFullPath);
			}
		}

		[Serializable]
		class GitNode : FormatStringNode
		{
			public readonly GitInfo GitInfo;

			public static bool IsGit(string token) => token.StartsWith("git:");

			public GitNode(string token) : base(token)
			{
				var identifier = token.IndexOf(':');
				GitInfo = Enum.Parse<GitInfo>(token[(identifier + 1)..]);
			}

			public override string GetValue(FileInfo fileInfo)
			{
				return AddGitInfoCommand.GetGitValue(GitInfo, fileInfo.OriginalFullPath);
			}
		}

		enum MacroTypes
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
		}

		[Serializable]
		class MacroNode : FormatStringNode
		{

			public readonly MacroTypes MacroType;
			public readonly int Arguments;

			public static bool IsMacro(string token)
			{
				var i = token.IndexOf(':');
				if (i >= 0)
					token = token[..i];
				return Enum.TryParse<MacroTypes>(token, true, out _);
			}

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

			public override string GetValue(FileInfo fileInfo)
			{
				switch (MacroType)
				{
					case MacroTypes.FileName:
						return fileInfo.IsDirectory
							? fileInfo.OriginalFilename
							: Path.GetFileNameWithoutExtension(fileInfo.OriginalFilename);

					case MacroTypes.Extension:
						return fileInfo.IsDirectory
							? string.Empty
							: Path.GetExtension(fileInfo.OriginalFilename);

					case MacroTypes.ProceedFileName:
						return fileInfo.IsDirectory
							? fileInfo.ChangedFilename
							: Path.GetFileNameWithoutExtension(fileInfo.ChangedFilename);

					case MacroTypes.ProceedExtension:
						return fileInfo.IsDirectory
							? string.Empty
							: Path.GetExtension(fileInfo.ChangedFilename);

					case MacroTypes.CurrentDirectory:
						return Path.GetFileName(fileInfo.OriginalPath);

					case MacroTypes.ProceedDirectory:
						return Path.GetFileName(fileInfo.ChangedPath);

					case MacroTypes.Path:
						return fileInfo.OriginalPath;

					case MacroTypes.ProceedPath:
						return fileInfo.ChangedPath;

					case MacroTypes.Index:
						var i = (FileInfo.Files.IndexOf(fileInfo) + 1).ToString();
						return Arguments < 0 ? i : i.PadLeft(Arguments, '0');

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}

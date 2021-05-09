using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DaramRenamer
{
	public static class FileInfoSerializer
	{
		private static readonly MemoryStream MemoryStream = new();
		private static readonly BinaryWriter BinaryWriter = new(MemoryStream, Encoding.UTF8);
		private static readonly BinaryReader BinaryReader = new(MemoryStream, Encoding.UTF8);

		public static byte[] Serialize(FileInfo fileInfo)
		{
			lock (MemoryStream)
			{
				try
				{
					BinaryWriter.Write(fileInfo.OriginalFullPath);
					BinaryWriter.Write(fileInfo.ChangedFilename);
					BinaryWriter.Write(fileInfo.ChangedPath);
					BinaryWriter.Flush();
					MemoryStream.Position = 0;

					return MemoryStream.ToArray();
				}
				finally
				{
					MemoryStream.SetLength(0);
				}
			}
		}

		public static byte[] SerializeCollection(ObservableCollection<FileInfo> fileInfos)
		{
			lock (MemoryStream)
			{
				try
				{
					BinaryWriter.Write(fileInfos.Count);
					foreach (var fileInfo in fileInfos)
					{
						BinaryWriter.Write(fileInfo.OriginalFullPath);
						BinaryWriter.Write(fileInfo.ChangedFilename);
						BinaryWriter.Write(fileInfo.ChangedPath);
					}

					BinaryWriter.Flush();
					MemoryStream.Position = 0;

					return MemoryStream.ToArray();
				}
				finally
				{
					MemoryStream.SetLength(0);
				}
			}
		}

		public static FileInfo Deserialize(byte[] serialized)
		{
			lock (MemoryStream)
			{
				try
				{
					MemoryStream.Write(serialized);
					MemoryStream.Position = 0;

					var originalFullPath = BinaryReader.ReadString();
					var changedFilename = BinaryReader.ReadString();
					var changedPath = BinaryReader.ReadString();
					return new FileInfo(originalFullPath, changedFilename, changedPath);
				}
				finally
				{
					MemoryStream.SetLength(0);
				}
			}
		}

		public static ObservableCollection<FileInfo> DeserializeCollection(byte[] serialized)
		{
			lock (MemoryStream)
			{
				try
				{
					MemoryStream.Write(serialized);
					MemoryStream.Position = 0;

					var count = BinaryReader.ReadInt32();
					var collection = new ObservableCollection<FileInfo>();
					for (var i = 0; i < count; ++i)
					{
						var originalFullPath = BinaryReader.ReadString();
						var changedFilename = BinaryReader.ReadString();
						var changedPath = BinaryReader.ReadString();
						var fileInfo = new FileInfo(originalFullPath, changedFilename, changedPath);
						collection.Add(fileInfo);
					}
					return collection;
				}
				finally
				{
					MemoryStream.SetLength(0);
				}
			}
		}
	}
}

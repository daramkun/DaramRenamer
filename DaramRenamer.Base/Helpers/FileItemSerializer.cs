using System.Collections.ObjectModel;
using System.Text;

namespace DaramRenamer.Helpers;

public static class FileItemSerializer
{
    private static readonly MemoryStream MemoryStream = new();
    private static readonly BinaryWriter BinaryWriter = new(MemoryStream, Encoding.UTF8);
    private static readonly BinaryReader BinaryReader = new(MemoryStream, Encoding.UTF8);

    public static byte[] Serialize(FileItem item)
    {
        lock (MemoryStream)
        {
            try
            {
                BinaryWriter.Write(item.SourceFullPath);
                BinaryWriter.Write(item.ChangedName);
                BinaryWriter.Write(item.ChangedPath);
                BinaryWriter.Write(item.IsDirectory);
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

    public static byte[] SerializeCollection(ObservableCollection<FileItem> items)
    {
        lock (MemoryStream)
        {
            try
            {
                BinaryWriter.Write(items.Count);
                foreach (var fileInfo in items)
                {
                    BinaryWriter.Write(fileInfo.SourceFullPath);
                    BinaryWriter.Write(fileInfo.ChangedName);
                    BinaryWriter.Write(fileInfo.ChangedPath);
                    BinaryWriter.Write(fileInfo.IsDirectory);
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

    public static FileItem Deserialize(byte[] serialized)
    {
        lock (MemoryStream)
        {
            try
            {
                MemoryStream.Write(serialized);
                MemoryStream.Position = 0;

                var sourceFullPath = BinaryReader.ReadString();
                var changedName = BinaryReader.ReadString();
                var changedPath = BinaryReader.ReadString();
                var isDirectory = BinaryReader.ReadBoolean();
                return new FileItem(sourceFullPath, changedName, changedPath, isDirectory);
            }
            finally
            {
                MemoryStream.SetLength(0);
            }
        }
    }

    public static ObservableCollection<FileItem> DeserializeCollection(byte[] serialized)
    {
        lock (MemoryStream)
        {
            try
            {
                MemoryStream.Write(serialized);
                MemoryStream.Position = 0;

                var count = BinaryReader.ReadInt32();
                var collection = new ObservableCollection<FileItem>();
                for (var i = 0; i < count; ++i)
                {
                    var sourceFullPath = BinaryReader.ReadString();
                    var changedName = BinaryReader.ReadString();
                    var changedPath = BinaryReader.ReadString();
                    var isDirectory = BinaryReader.ReadBoolean();
                    var fileInfo = new FileItem(sourceFullPath, changedName, changedPath, isDirectory);
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
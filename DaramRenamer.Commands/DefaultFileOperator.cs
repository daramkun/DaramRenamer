using System.Collections.Generic;
using System.IO;

namespace DaramRenamer
{
	class DefaultFileOperator : IFileOperator
	{
		public void BeginBatch() { }

		public void EndBatch() { }

		public void Move(string destination, string source, bool overwrite)
		{
			var deleteOriginal = false;
			string moveName = null;
			if (File.Exists(destination) && overwrite)
			{
				File.Move(source, moveName = (source + Path.GetTempFileName()));
				deleteOriginal = true;
			}

			try
			{
				File.Move(source, destination);
			}
			catch
			{
				if (deleteOriginal)
					File.Move(moveName, source);
				throw;
			}

			if (deleteOriginal)
				File.Delete(moveName);
		}

		public void Copy(string destination, string source, bool overwrite)
		{
			File.Copy(source, destination, overwrite);
		}

		private readonly string[] EmptyList = new string[] { };

		public IEnumerable<string> GetFiles(string directory, bool topDirectoryOnly)
		{
			if (!Directory.Exists(directory))
				return EmptyList;
			return Directory.EnumerateFiles(directory, "*",
				topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
		}

		public bool FileExists (string path)
		{
			return File.Exists (path) && File.GetAttributes (path) != FileAttributes.Directory;
		}

		public bool DirectoryExists (string path)
		{
			return Directory.Exists (path) && File.GetAttributes (path) == FileAttributes.Directory;
		}
	}
}
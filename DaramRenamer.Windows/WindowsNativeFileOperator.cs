using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer
{
	class WindowsNativeFileOperator : IFileOperator
	{
		public void BeginBatch()
		{
			Daramee.Winston.File.Operation.Begin(true);
		}

		public void EndBatch()
		{
			Daramee.Winston.File.Operation.End();
		}

		public void Move(string destination, string source, bool overwrite)
		{
			Daramee.Winston.File.Operation.Move(destination, source, overwrite);
		}

		public void Copy(string destination, string source, bool overwrite)
		{
			Daramee.Winston.File.Operation.Copy(destination, source, overwrite);
		}

		public IEnumerable<string> GetFiles(string directory, bool topDirectoryOnly)
		{
			return Daramee.Winston.File.FilesEnumerator.EnumerateFiles(directory, "*.*", topDirectoryOnly);
		}

		public bool FileExists(string path)
		{
			return File.Exists(path) && File.GetAttributes(path) != FileAttributes.Directory;
		}

		public bool DirectoryExists(string path)
		{
			return Directory.Exists (path) && File.GetAttributes (path) == FileAttributes.Directory;
		}
	}
}

using Daramee.DaramCommonLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public enum ErrorCode
	{
		NoError,
		Unknown,
		UnauthorizedAccess,
		PathTooLong,
		DirectoryNotFound,
		IOError,
		FailedOverwrite,
	}

	[Serializable]
	public class FileInfo : IComparable<FileInfo>, INotifyPropertyChanged
	{
		public static ObservableCollection<FileInfo> Files { get; set; } = new ObservableCollection<FileInfo> ();

		string originalFullPath;
		string changedPath, changedFilename;

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public string OriginalFullPath
		{
			get { return originalFullPath; }
			private set
			{
				originalFullPath = value;
				PC ( nameof ( OriginalFullPath ) );
				PC ( nameof ( OriginalPath ) );
				PC ( nameof ( OriginalFilename ) );
			}
		}
		public string OriginalPath => Path.GetDirectoryName ( OriginalFullPath );
		public string OriginalFilename => Path.GetFileName ( OriginalFullPath );
		public string ChangedPath { get => changedPath; set { changedPath = value; PC ( nameof ( ChangedPath ) ); } }
		public string ChangedFilename { get => changedFilename; set { changedFilename = value; PC ( nameof ( ChangedFilename ) ); } }
		public string ChangedFullPath => Path.Combine ( ChangedPath, ChangedFilename );
		public bool IsDirectory { get; set; }

		public FileInfo ( string fullPath )
		{
			OriginalFullPath = fullPath; ChangedFilename = OriginalFilename; ChangedPath = OriginalPath;
			IsDirectory = File.GetAttributes ( fullPath ).HasFlag ( FileAttributes.Directory );
		}
		public FileInfo ( FileInfo file )
		{
			OriginalFullPath = file.OriginalFullPath;
			ChangedPath = file.ChangedPath;
			ChangedFilename = file.ChangedFilename;
			IsDirectory = file.IsDirectory;
		}
		
		public static bool Move ( FileInfo fileInfo, bool overwrite, out ErrorCode errorMessage )
		{
			try
			{
				Daramee.Winston.File.Operation.Move ( fileInfo.ChangedFullPath, fileInfo.OriginalFullPath, overwrite );
				errorMessage = ErrorCode.NoError;
				return true;
			}
			catch ( UnauthorizedAccessException ) { errorMessage = ErrorCode.UnauthorizedAccess; }
			catch ( PathTooLongException ) { errorMessage = ErrorCode.PathTooLong; }
			catch ( DirectoryNotFoundException ) { errorMessage = ErrorCode.DirectoryNotFound; }
			catch ( IOException ) { errorMessage = ErrorCode.IOError; }
			catch ( Exception ) { errorMessage = ErrorCode.Unknown; }
			return false;
		}

		public static bool Copy ( FileInfo fileInfo, bool overwrite, out ErrorCode errorMessage )
		{
			try
			{
				if ( overwrite && File.Exists ( fileInfo.ChangedFullPath ) )
					File.Delete ( fileInfo.ChangedFullPath );

				//File.Copy ( fileInfo.OriginalFullPath, fileInfo.ChangedFullPath, overwrite );
				Daramee.Winston.File.Operation.Copy ( fileInfo.ChangedFullPath, fileInfo.OriginalFullPath, overwrite );
				
				errorMessage = ErrorCode.NoError;
				return true;
			}
			catch ( UnauthorizedAccessException ) { errorMessage = ErrorCode.UnauthorizedAccess; }
			catch ( PathTooLongException ) { errorMessage = ErrorCode.PathTooLong; }
			catch ( DirectoryNotFoundException ) { errorMessage = ErrorCode.DirectoryNotFound; }
			catch ( IOException ) { errorMessage = overwrite ? ErrorCode.FailedOverwrite : ErrorCode.IOError; }
			catch ( Exception ) { errorMessage = ErrorCode.Unknown; }
			return false;
		}

		public void Changed ()
		{
			OriginalFullPath = ChangedFullPath;
		}

		public static void Sort ( ObservableCollection<FileInfo> source )
		{
			if ( source == null ) return;
			Daramee.DaramCommonLib.Sort.Quicksort<FileInfo> ( source );
		}

		public int CompareTo ( FileInfo other ) => ChangedFilename.CompareTo ( other.ChangedFilename );

		public override bool Equals ( object obj ) => obj is FileInfo ? OriginalFullPath == ( obj as FileInfo ).OriginalFullPath : false;
		public override int GetHashCode () => OriginalFullPath.GetHashCode ();

		private void PC ( string name ) { PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( name ) ); }
	}
}

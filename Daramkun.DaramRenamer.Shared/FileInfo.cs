using Daramee.DaramCommonLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
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
		FileNotFound,
	}

	public enum RenameMode : byte { Move, Copy }

	[Serializable]
	public class FileInfo : IComparable<FileInfo>, INotifyPropertyChanged
	{
		public static ObservableCollection<FileInfo> Files { get; set; } = new ObservableCollection<FileInfo> ();

		public static void Sort ( ObservableCollection<FileInfo> source )
		{
			if ( source == null ) return;
			Daramee.DaramCommonLib.Sort.Quicksort<FileInfo> ( source );
		}

		public static void Apply ( bool autoFix, RenameMode renameMode, bool overwrite,
			Action<FileInfo, ErrorCode> progressIncrement )
		{
			bool parallelApply = true;
			Parallel.ForEach ( Files, ( fileInfo, parallelLoopState ) =>
			{
				if ( File.Exists ( fileInfo.OriginalFullPath ) )
				{
					parallelApply = false;
					parallelLoopState.Break ();
				}
			} );

			int failed = 0;
			ConcurrentQueue<FileInfo> succeededItems = new ConcurrentQueue<FileInfo> ();
			void itemChanger ( FileInfo fileInfo )
			{
				if ( autoFix )
				{
					fileInfo.ChangedPath = FilesHelper.ReplaceInvalidPathCharacters ( fileInfo.ChangedPath );
					fileInfo.ChangedFilename = FilesHelper.ReplaceInvalidFilenameCharacters ( fileInfo.ChangedFilename );
				}
				ErrorCode errorMessage = ErrorCode.NoError;
				if ( renameMode == RenameMode.Move )
					fileInfo.Move ( overwrite, out errorMessage );
				else if ( renameMode == RenameMode.Copy )
					fileInfo.Copy ( overwrite, out errorMessage );

				progressIncrement?.Invoke ( fileInfo, errorMessage );

				if ( errorMessage != ErrorCode.NoError )
					Interlocked.Increment ( ref failed );
				else succeededItems.Enqueue ( fileInfo );
			}

			if ( parallelApply )
			{
				Daramee.Winston.File.Operation.Begin ( true );
				Parallel.ForEach ( Files, itemChanger );
				Daramee.Winston.File.Operation.End ();
			}
			else
			{
				Daramee.Winston.File.Operation.Begin ( true );
				Parallel.ForEach ( from f in Files where !File.Exists ( f.ChangedFullPath ) select f, itemChanger );
				Daramee.Winston.File.Operation.End ();

				List<FileInfo> sortingFileInfo = new List<FileInfo> ( from f in Files where !succeededItems.Contains ( f ) && f.OriginalFullPath != f.ChangedFullPath select f );
				List<FileInfo> temp = new List<FileInfo> ();
				bool changed = false;
				do
				{
					changed = false;
					Daramee.Winston.File.Operation.Begin ( true );
					foreach ( var fileInfo in sortingFileInfo )
					{
						if ( !File.Exists ( fileInfo.ChangedFullPath ) )
						{
							itemChanger ( fileInfo );
							changed = true;
							temp.Add ( fileInfo );
						}
						else
						{
							foreach ( var succeededFileInfo in Enumerable.Concat ( succeededItems, temp ) )
							{
								if ( succeededFileInfo.ChangedFullPath != fileInfo.ChangedFullPath &&
									succeededFileInfo.OriginalFullPath == fileInfo.ChangedFullPath )
								{
									itemChanger ( fileInfo );
									changed = true;
									temp.Add ( fileInfo );
									break;
								}
							}
						}
					}
					Daramee.Winston.File.Operation.End ();
					foreach ( var proceed in temp )
						sortingFileInfo.Remove ( proceed );
					temp.Clear ();
				} while ( changed );

				if ( overwrite )
				{
					Parallel.ForEach ( sortingFileInfo, ( fileInfo ) =>
					{
						itemChanger ( fileInfo );
					} );
				}
				else
				{
					failed += sortingFileInfo.Count;
					foreach ( var fileInfo in sortingFileInfo )
						progressIncrement ( fileInfo, ErrorCode.IOError );
				}
			}

			Parallel.ForEach ( succeededItems, ( fileInfo ) => fileInfo.Changed () );
		}

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
		
		public bool Move ( bool overwrite, out ErrorCode errorMessage )
		{
			try
			{
				Daramee.Winston.File.Operation.Move ( ChangedFullPath, OriginalFullPath, overwrite );
				errorMessage = ErrorCode.NoError;
				return true;
			}
			catch ( UnauthorizedAccessException ) { errorMessage = ErrorCode.UnauthorizedAccess; }
			catch ( PathTooLongException ) { errorMessage = ErrorCode.PathTooLong; }
			catch ( DirectoryNotFoundException ) { errorMessage = ErrorCode.DirectoryNotFound; }
			catch ( FileNotFoundException ) { errorMessage = ErrorCode.FileNotFound; }
			catch ( IOException ) { errorMessage = ErrorCode.IOError; }
			catch ( Exception ) { errorMessage = ErrorCode.Unknown; }
			return false;
		}

		public bool Copy ( bool overwrite, out ErrorCode errorMessage )
		{
			try
			{
				Daramee.Winston.File.Operation.Copy ( ChangedFullPath, OriginalFullPath, overwrite );
				errorMessage = ErrorCode.NoError;
				return true;
			}
			catch ( UnauthorizedAccessException ) { errorMessage = ErrorCode.UnauthorizedAccess; }
			catch ( PathTooLongException ) { errorMessage = ErrorCode.PathTooLong; }
			catch ( DirectoryNotFoundException ) { errorMessage = ErrorCode.DirectoryNotFound; }
			catch ( FileNotFoundException ) { errorMessage = ErrorCode.FileNotFound; }
			catch ( IOException ) { errorMessage = overwrite ? ErrorCode.FailedOverwrite : ErrorCode.IOError; }
			catch ( Exception ) { errorMessage = ErrorCode.Unknown; }
			return false;
		}

		public void Changed ()
		{
			OriginalFullPath = ChangedFullPath;
		}

		public int CompareTo ( FileInfo other ) => ChangedFilename.CompareTo ( other.ChangedFilename );

		public override bool Equals ( object obj ) => obj is FileInfo ? OriginalFullPath == ( obj as FileInfo ).OriginalFullPath : false;
		public override int GetHashCode () => OriginalFullPath.GetHashCode ();

		private void PC ( string name ) { PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( name ) ); }
	}
}

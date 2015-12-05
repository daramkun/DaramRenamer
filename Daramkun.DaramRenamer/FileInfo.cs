using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
		string originalFullPath;
		string changedPath, changedFilename;
		
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
		public string OriginalPath { get { return Path.GetDirectoryName ( OriginalFullPath ); } }
		public string OriginalFilename { get { return Path.GetFileName ( OriginalFullPath ); } }
		public string ChangedPath { get { return changedPath; } set { changedPath = value; PC ( nameof ( ChangedPath ) ); } }
		public string ChangedFilename { get { return changedFilename; } set { changedFilename = value; PC ( nameof ( ChangedFilename ) ); } }
		public string ChangedFullPath { get { return Path.Combine ( ChangedPath, ChangedFilename ); } }

		public FileInfo ( string fullPath ) { OriginalFullPath = fullPath; ChangedFilename = OriginalFilename; ChangedPath = OriginalPath; }
		public FileInfo ( FileInfo file )
		{
			OriginalFullPath = file.OriginalFullPath;
			ChangedPath = file.ChangedPath;
			ChangedFilename = file.ChangedFilename;
		}

		private char GetInvalidToValid ( char ch )
		{
			switch ( ch )
			{
				case '?': return '？';
				case '\\': return '＼';
				case '/': return '／';
				case '<': return '〈';
				case '>': return '〉';
				case '*': return '＊';
				case '|': return '｜';
				case ':': return '：';
				case '"': return '＂';
				default: return ch;
			}
		}

		public void ReplaceInvalidPathCharacters ()
		{
			foreach ( var ch in Path.GetInvalidPathChars () )
			{
				if ( ChangedPath.IndexOf ( ch ) < 0 )
					ChangedPath = ChangedPath.Replace ( ch, GetInvalidToValid ( ch ) );
			}
		}

		public void ReplaceInvalidFilenameCharacters ()
		{
			foreach ( var ch in Path.GetInvalidFileNameChars () )
			{
				if ( ChangedFilename.IndexOf ( ch ) < 0 )
					ChangedFilename = ChangedFilename.Replace ( ch, GetInvalidToValid ( ch ) );
			}
		}

		public bool Move ( out ErrorCode errorMessage )
		{
			try
			{
				File.Move ( OriginalFullPath, ChangedFullPath );
				Changed ();
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

		public bool Copy ( bool overwrite, out ErrorCode errorMessage )
		{
			try
			{
				File.Copy ( OriginalFullPath, ChangedFullPath, overwrite );
				Changed ();
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

		private void Changed ()
		{
			OriginalFullPath = ChangedFullPath;
		}

		public static void Sort ( ObservableCollection<FileInfo> source )
		{
			if ( source == null ) return;
			ParallelSort.QuicksortParallel<FileInfo> ( source );
		}

		public int CompareTo ( FileInfo other ) => ChangedFilename.CompareTo ( other.ChangedFilename );

		private void PC ( string name ) { if ( PropertyChanged != null ) PropertyChanged ( this, new PropertyChangedEventArgs ( name ) ); }
	}

	static class ParallelSort
	{
		public static void QuicksortParallel<T>( ObservableCollection<T> arr ) where T : IComparable<T> { QuicksortParallel ( arr, 0, arr.Count - 1 ); }
		private static void QuicksortParallel<T>( ObservableCollection<T> arr, int left, int right ) where T : IComparable<T>
		{
			if ( right > left )
			{
				int pivot = Partition ( arr, left, right );
				Parallel.Invoke ( new Action [] { () => QuicksortParallel ( arr, left, pivot - 1 ), () => QuicksortParallel ( arr, pivot + 1, right ) } );
			}
		}
		private static void Swap<T>( ObservableCollection<T> a, int i, int j ) { T t = a [ i ]; a [ i ] = a [ j ]; a [ j ] = t; }
		private static int Partition<T>( ObservableCollection<T> arr, int low, int high ) where T : IComparable<T>
		{
			int pivotPos = ( high + low ) / 2, left = low;
			T pivot = arr [ pivotPos ];
			Swap ( arr, low, pivotPos );
			for ( int i = low + 1; i <= high; i++ )
				if ( arr [ i ].CompareTo ( pivot ) < 0 )
					Swap ( arr, i, ++left );
			Swap ( arr, low, left );
			return left;
		}
	}
}

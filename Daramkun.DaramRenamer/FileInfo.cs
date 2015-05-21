using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	[Serializable]
	public class FileInfo : INotifyPropertyChanged, IComparable<FileInfo>
	{
		string originalName, changeName, originalPath, changePath;

		public string OriginalName { get { return originalName; } set { originalName = value; PC ( "OriginalName" ); } }
		public string ChangedName { get { return changeName; } set { changeName = value; PC ( "ChangedName" ); } }
		public string OriginalPath { get { return originalPath; } set { originalPath = value; PC ( "OriginalPath" ); } }
		public string ChangedPath { get { return changePath; } set { changePath = value; PC ( "ChangedPath" ); } }

		public string OriginalFullName { get { return Path.Combine ( OriginalPath, OriginalName ); } }
		public string ChangedFullName { get { return Path.Combine ( ChangedPath, ChangedName ); } }

		public FileInfo () { originalName = ""; changeName = ""; originalPath = ""; changePath = ""; }
		public FileInfo ( FileInfo fileInfo ) { ValueCopy ( fileInfo ); }

		public void ValueCopy ( FileInfo fileInfo )
		{
			OriginalName = fileInfo.OriginalName.Clone () as string;
			ChangedName = fileInfo.ChangedName.Clone () as string;
			OriginalPath = fileInfo.OriginalPath.Clone () as string;
			ChangedPath = fileInfo.ChangedPath.Clone () as string;
		}

		private void PC ( string p )
		{
			if ( PropertyChanged != null )
				PropertyChanged ( this, new PropertyChangedEventArgs ( p ) );
		}

		private string ReplaceInvalidCharacter(char ch)
		{
			switch(ch)
			{
				case '?': return "？";
				case '\\': return "＼";
				case '/': return "／";
				case '<': return "〈";
				case '>': return "〉";
				case '*': return "＊";
				case '|': return "｜";
				case ':': return "：";
				case '"': return "＂";
				default: return "";
			}
		}

		public void FixFilename ()
		{
			foreach ( var ch in System.IO.Path.GetInvalidPathChars () )
			{
				if ( ChangedPath.IndexOf ( ch ) < 0 )
					ChangedPath = ChangedPath.Replace ( "" + ch, ReplaceInvalidCharacter ( ch ) );
			}

			foreach ( var ch in System.IO.Path.GetInvalidFileNameChars () )
			{
				if ( ChangedName.IndexOf ( ch ) < 0 )
					ChangedName = ChangedPath.Replace ( "" + ch, ReplaceInvalidCharacter ( ch ) );
			}
		}

		public void Changed ()
		{
			OriginalName = changeName.Clone () as string;
			OriginalPath = changePath.Clone () as string;
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public int CompareTo ( FileInfo other ) { return changeName.CompareTo ( other.changeName ); }

		public bool ToMove ()
		{
			try
			{
				File.Move ( OriginalFullName, ChangedFullName );
				Changed ();
				return true;
			}
			catch ( UnauthorizedAccessException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_NoAuthentication, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( PathTooLongException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_PathIsTooLong, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( DirectoryNotFoundException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_NoPath, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( IOException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_IOException, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( Exception ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_Unknown, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			return false;
		}
		public bool ToCopy ()
		{
			try
			{
				File.Copy ( OriginalFullName, ChangedFullName );
				Changed ();
				return true;
			}
			catch ( UnauthorizedAccessException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_NoAuthentication, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( PathTooLongException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_PathIsTooLong, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( DirectoryNotFoundException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_NoPath, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( FileNotFoundException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_FileIsNotFound, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( IOException ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_IOException, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			catch ( Exception ex )
			{
				MainWindow.SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_Unknown, OriginalName ) );
				Debug.WriteLine ( ex.Message );
			}
			return false;
		}

		public static void Sort ( ObservableCollection<FileInfo> source )
		{
			if ( source == null ) return;
			ParallelSort.QuicksortParallel<FileInfo> ( source );
		}
	}

	static class ParallelSort
	{
		public static void QuicksortParallel<T> ( ObservableCollection<T> arr ) where T : IComparable<T> { QuicksortParallel ( arr, 0, arr.Count - 1 ); }
		private static void QuicksortParallel<T> ( ObservableCollection<T> arr, int left, int right ) where T : IComparable<T>
		{
			if ( right > left )
			{
				int pivot = Partition ( arr, left, right );
				Parallel.Invoke ( new Action [] { () => QuicksortParallel ( arr, left, pivot - 1 ), () => QuicksortParallel ( arr, pivot + 1, right ) } );
			}
		}
		private static void Swap<T> ( ObservableCollection<T> a, int i, int j ) { T t = a [ i ]; a [ i ] = a [ j ]; a [ j ] = t; }
		private static int Partition<T> ( ObservableCollection<T> arr, int low, int high ) where T : IComparable<T>
		{
			int pivotPos = ( high + low ) / 2, left = low;
			T pivot = arr [ pivotPos ];
			Swap ( arr, low, pivotPos );
			for ( int i = low + 1; i <= high; i++ ) if ( arr [ i ].CompareTo ( pivot ) < 0 ) Swap ( arr, i, ++left );
			Swap ( arr, low, left );
			return left;
		}
	}
}

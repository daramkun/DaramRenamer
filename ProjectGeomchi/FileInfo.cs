using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupRenamer
{
	[Serializable]
	public class FileInfo : INotifyPropertyChanged, IComparable<FileInfo>
	{
		string originalName, changeName, originalPath, changePath;

		public string ON { get { return originalName; } set { originalName = value; PC ( "ON" ); } }
		public string CN { get { return changeName; } set { changeName = value; PC ( "CN" ); } }
		public string OP { get { return originalPath; } set { originalPath = value; PC ( "OP" ); } }
		public string CP { get { return changePath; } set { changePath = value; PC ( "CP" ); } }

		private void PC ( string p )
		{
			if ( PropertyChanged != null )
				PropertyChanged ( this, new PropertyChangedEventArgs ( p ) );
		}

		public void Changed ()
		{
			ON = changeName.Clone () as string;
			OP = changePath.Clone () as string;
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public int CompareTo ( FileInfo other )
		{
			return changeName.CompareTo ( other.changeName );
		}

		public static ObservableCollection<FileInfo> Sort ( ObservableCollection<FileInfo> source )
		{
			if ( source == null ) return null;
			List<FileInfo> list = source.ToList();
			ParallelSort.QuicksortParallel<FileInfo> ( list );
			return new ObservableCollection<FileInfo> ( list );
		}
	}
}

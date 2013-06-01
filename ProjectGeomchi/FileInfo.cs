using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GroupRenamer
{
	static class ObservableCollectionUtility
	{
		public static void Sort<TSource, TKey> ( this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector, bool desc = false )
		{
			if ( source == null ) return;

			Comparer<TKey> comparer = Comparer<TKey>.Default;

			for ( int i = source.Count - 1; i >= 0; i-- )
			{
				for ( int j = 1; j <= i; j++ )
				{
					TSource o1 = source [ j - 1 ];
					TSource o2 = source [ j ];
					int comparison = comparer.Compare ( keySelector ( o1 ), keySelector ( o2 ) );
					if ( desc && comparison < 0 )
						source.Move ( j, j - 1 );
					else if ( !desc && comparison > 0 )
						source.Move ( j - 1, j );
				}
			}
		}
	}

	[Serializable]
	public class FileInfo : INotifyPropertyChanged
	{
		string originalName, changeName, originalPath, changePath;

		public string OriginalName { get { return originalName; } set { originalName = value; PC ( "OriginalName" ); } }
		public string ChangeName { get { return changeName; } set { changeName = value; PC ( "ChangeName" ); } }
		public string OriginalPath { get { return originalPath; } set { originalPath = value; PC ( "ChangePath" ); } }
		public string ChangePath { get { return changePath; } set { changePath = value; PC ( "ChangePath" ); } }

		private void PC ( string p )
		{
			if ( PropertyChanged != null )
				PropertyChanged ( this, new PropertyChangedEventArgs ( p ) );
		}

		public void Changed ()
		{
			OriginalName = changeName.Clone () as string;
			OriginalPath = changePath.Clone () as string;
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
	}
}

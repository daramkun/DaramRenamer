using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupRenamer
{
	public class ParallelSort
	{
		public static void QuicksortParallel<T> ( List<T> arr ) where T : IComparable<T>
		{
			QuicksortParallel ( arr, 0, arr.Count - 1 );
		}

		private static void QuicksortParallel<T> ( List<T> arr, int left, int right ) where T : IComparable<T>
		{
			if ( right > left )
			{
				int pivot = Partition ( arr, left, right );
				Parallel.Invoke ( new Action [] {
					() => { QuicksortParallel ( arr, left, pivot - 1 ); },
					() => { QuicksortParallel ( arr, pivot + 1, right ); }
				} );
			}
		}

		private static void Swap<T> ( List<T> a, int i, int j ) { T t = a [ i ]; a [ i ] = a [ j ]; a [ j ] = t; }

		private static int Partition<T> ( List<T> arr, int low, int high ) where T : IComparable<T>
		{
			int pivotPos = ( high + low ) / 2;
			T pivot = arr [ pivotPos ];
			Swap ( arr, low, pivotPos );

			int left = low;
			for ( int i = low + 1; i <= high; i++ )
				if ( arr [ i ].CompareTo ( pivot ) < 0 )
					Swap ( arr, i, ++left );

			Swap ( arr, low, left );
			return left;
		}
	}
}

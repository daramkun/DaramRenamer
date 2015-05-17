using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public sealed class BatchProcessor
	{
		private static string [] NewLineSplitter = new [] { "\n" };

		public FileInfo Process ( FileInfo original, string batchScript )
		{
			string [] lines = batchScript.Split ( NewLineSplitter, StringSplitOptions.RemoveEmptyEntries );

			foreach ( string line in lines )
			{
				string cur = line.Trim ();
				// Functional Mode
				if ( cur [ 0 ] == '!' )
				{
					string function = cur.Substring ( 1, cur.IndexOf ( ':' ) );
					List<object> arguments = new List<object> ();
					for ( int i = function.Length; i < cur.Length; ++i )
					{
						
					}

					switch ( function )
					{
						case "!add:": break;
					}
				}
				// Format Mode
				else
				{
					
				}
			}

			return original;
		}
	}
}

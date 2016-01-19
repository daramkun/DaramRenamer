using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Number
{
	public class DeleteWithoutNumbersProcessor : IProcessor
	{
		public string Name { get { return "process_delete_without_numbers"; } }
		public bool CannotMultithreadProcess { get { return false; } }

		public bool Process ( FileInfo file )
		{
			if ( file.ChangedFilename.Length == 0 ) return false;
			List<string> split = new List<string> ( Path.GetFileNameWithoutExtension ( file.ChangedFilename ).Split ( ' ' ) );
			for ( int i = 0; i < split.Count; ++i )
				for ( int j = 0; j < split [ i ].Length; ++i )
					if ( !( split [ i ] [ j ] >= '0' && split [ i ] [ j ] <= '9' ) )
						split.RemoveAt ( i-- );
			file.ChangedFilename = string.Join ( " ", split.ToArray () ) + Path.GetExtension ( file.ChangedFilename );
			return true;
		}
	}
}

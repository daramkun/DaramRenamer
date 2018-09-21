using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Number
{
	[Serializable]
	public class IncreaseDecreaseNumbersProcessor : IProcessor
	{
		public string Name => "process_increase_decrease_numbers";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "incdec_count" )]
		public int Count { get; set; } = 1;
		[Argument ( Name = "incdec_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			if ( Count == 0 ) return true;
			if ( file.ChangedFilename.Length == 0 ) return false;
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );

			bool meetTheNumber = false;
			int offset = 0, count = 0, size = 0;
			foreach ( char ch in Position == OnePointPosition.StartPoint ? fn : fn.Reverse () )
			{
				if ( ( ch >= '0' && ch <= '9' ) )
				{
					if ( !meetTheNumber )
					{
						offset = count;
						meetTheNumber = true;
					}
					++size;
				}
				else
				{
					if ( meetTheNumber )
						break;
				}
				++count;
			}

			if ( !meetTheNumber ) return false;

			if ( Position == OnePointPosition.EndPoint )
				offset = fn.Length - ( offset + size );

			string origin = fn.Substring ( offset, size );
			int number = int.Parse ( origin ) + Count;

			StringBuilder sb = new StringBuilder ();
			sb.Append ( number.ToString ().PadLeft ( size, '0' ) );
			fn = fn.Remove ( offset, size ).Insert ( offset, sb.ToString () );

			file.ChangedFilename = fn + Path.GetExtension ( file.ChangedFilename );
			return true;
		}
	}
}

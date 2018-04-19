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
	public class NumberCountMatchProcessor : IProcessor
	{
		public string Name => "process_matching_number_count";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "match_count" )]
		public uint Count { get; set; } = 2;
		[Argument ( Name = "match_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			if ( file.ChangedFilename.Length == 0 ) return false;
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );

			bool meetTheNumber = false;
			uint offset = 0, count = 0, size = 0;
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
					{
						if ( Position == OnePointPosition.EndPoint )
							offset = ( uint ) fn.Length - ( offset + size );
						break;
					}
				}
				++count;
			}

			if ( !meetTheNumber || size >= Count ) return false;

			StringBuilder sb = new StringBuilder ();
			sb.Append ( fn );
			size = Count - size;
			while ( size > 0 )
			{
				sb.Insert ( ( int ) offset, '0' );
				--size;
			}
			sb.Append ( Path.GetExtension ( file.ChangedFilename ) );

			file.ChangedFilename = sb.ToString ();
			return true;
		}
	}
}

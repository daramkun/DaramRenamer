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

		[Globalized ( "delwtnum_wordly" )]
		public bool IsWordlyProcessing { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			if ( file.ChangedFilename.Length == 0 ) return false;
			if ( !IsWordlyProcessing )
			{
				StringBuilder sb = new StringBuilder ();
				foreach ( char ch in Path.GetFileNameWithoutExtension ( file.ChangedFilename ) )
					if ( ch >= '0' && ch <= '9' )
						sb.Append ( ch );
				file.ChangedFilename = sb.ToString () + Path.GetExtension ( file.ChangedFilename );
			}
			else
			{
				List<string> split = new List<string> ( Path.GetFileNameWithoutExtension ( file.ChangedFilename ).Split ( new char [] {
					' ', '[', ']', ',', '.', '(', ')', '{', '}', '<', '>', '　', '\t', ':', ';', '*', '&', '@', '^'
				} ) );
				StringBuilder sb = new StringBuilder ();
				foreach ( var str in split )
				{
					foreach ( var ch in str )
						if ( ch >= '0' && ch <= '9' )
							sb.Append ( ch );
					sb.Append ( ' ' );
				}
				sb.Remove ( sb.Length - 1, 1 );
				file.ChangedFilename = $"{sb}{Path.GetExtension ( file.ChangedFilename )}";
			}
			return true;
		}
	}
}

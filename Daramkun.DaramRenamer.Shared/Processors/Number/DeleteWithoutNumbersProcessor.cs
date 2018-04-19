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
	public class DeleteWithoutNumbersProcessor : IProcessor
	{
		private static char [] spliters = new char [] {
			' ', '[', ']', ',', '.', '(', ')', '{', '}', '<', '>', '　',
			'\t', ':', ';', '*', '&', '@', '^', '-', '_', '=', '+', '~',
		};

		public string Name => "process_delete_without_numbers";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "delwtnum_wordly" )]
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
				file.ChangedFilename = $"{ sb }{ Path.GetExtension ( file.ChangedFilename ) }";
			}
			else
			{
				List<string> split = new List<string> ( Path.GetFileNameWithoutExtension ( file.ChangedFilename ).Split ( spliters ) );
				StringBuilder sb = new StringBuilder ();
				foreach ( var str in split )
				{
					foreach ( var ch in str )
						if ( ch >= '0' && ch <= '9' )
							sb.Append ( ch );
					sb.Append ( ' ' );
				}
				sb.Remove ( sb.Length - 1, 1 );
				file.ChangedFilename = $"{ sb }{ Path.GetExtension ( file.ChangedFilename ) }";
			}
			return true;
		}
	}
}

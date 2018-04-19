using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Date
{
	[Serializable]
	public class AddDateProcessor : IProcessor
	{
		public string Name => "process_add_date";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "add_date_type" )]
		public DateType Type { get; set; } = DateType.CreationDate;
		[Argument ( Name = "add_date_format" )]
		public string Format { get; set; } = "yyMMdd";
		[Argument ( Name = "add_date_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			string date;
			switch ( Type)
			{
				case DateType.CreationDate: date = File.GetCreationTime ( file.OriginalFullPath ).ToString ( Format ); break;
				case DateType.ModifiedDate: date = File.GetLastWriteTime ( file.OriginalFullPath ).ToString ( Format ); break;
				case DateType.AccessedDate: date = File.GetLastAccessTime ( file.OriginalFullPath ).ToString ( Format ); break;
				case DateType.Now: date = DateTime.Now.ToString ( Format ); break;
				default: return false;
			}
			file.ChangedFilename = Position == OnePointPosition.StartPoint ? $"{date}{fn}{ext}" :
				( Position == OnePointPosition.EndPoint ? $"{fn}{date}{ext}" : $"{date}{fn}{date}{ext}" );
			return true;
		}
	}
}

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
	public class AddIndexNumberProcessor : IProcessor
	{
		public string Name => "process_add_index_numbers";
		public bool CannotMultithreadProcess => true;

		[Argument ( Name = "add_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			var index = FileInfo.Files.IndexOf ( file ) + 1;
			file.ChangedFilename = string.Format ( Position == OnePointPosition.EndPoint ? "{0}{1}{2}" : "{1}{0}{2}",
				Path.GetFileNameWithoutExtension ( file.ChangedFilename ), index, Path.GetExtension ( file.ChangedFilename ) );
			return true;
		}
	}
}

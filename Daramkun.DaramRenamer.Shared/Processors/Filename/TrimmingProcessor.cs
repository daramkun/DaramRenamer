using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	[Serializable]
	public class TrimmingProcessor : IProcessor
	{
		public string Name => "process_trimming_text";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "trim_pos" )]
		public Position TrimPosition { get; set; } = Position.BothPoint;

		public bool Process ( FileInfo file )
		{
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			fn = TrimPosition == Position.StartPoint ? fn.TrimStart () : ( TrimPosition == Position.EndPoint ? fn.TrimEnd () : fn.Trim () );
			file.ChangedFilename = $"{ fn }{ Path.GetExtension ( file.ChangedFilename ) }";
			return true;
		}
	}
}

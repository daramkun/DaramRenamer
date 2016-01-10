using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class TrimmingProcessor : IProcessor
	{
		public string Name { get { return "process_trimming_text"; } }

		[Globalized ( "trim_pos" )]
		public Position TrimPosition { get; set; }

		public TrimmingProcessor () { TrimPosition = Position.BothPoint; }
		public TrimmingProcessor ( Position pos )
		{
			TrimPosition = pos;
		}

		public bool Process ( FileInfo file )
		{
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			fn = TrimPosition == Position.StartPoint ? fn.TrimStart () : ( TrimPosition == Position.EndPoint ? fn.TrimEnd () : fn.Trim () );
			file.ChangedFilename = $"{ fn }{ Path.GetExtension ( file.ChangedFilename ) }";
			return true;
		}
	}
}

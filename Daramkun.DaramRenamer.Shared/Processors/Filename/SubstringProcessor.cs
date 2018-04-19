using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	[Serializable]
	public class SubstringProcessor : IProcessor
	{
		public string Name => "process_substring_text";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "start_index" )]
		public uint StartIndex { get; set; } = 0;
		[Argument ( Name = "substring_length" )]
		public uint? Length { get; set; } = null;
		[Argument ( Name = "include_extension" )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = Length == null ? file.ChangedFilename.Substring ( ( int ) StartIndex ) :
				file.ChangedFilename.Substring ( ( int ) StartIndex, ( int ) Length.Value );
			return true;
		}
	}
}

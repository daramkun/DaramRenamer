using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class SubstringProcessor : IProcessor
	{
		public string Name => "process_substring_text";
		public bool CannotMultithreadProcess => false;

		[Globalized ( "start_index", 0 )]
		public uint StartIndex { get; set; } = 0;
		[Globalized ( "substring_length", 1 )]
		public uint? Length { get; set; } = null;
		[Globalized ( "include_extension", 2 )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = Length == null ? file.ChangedFilename.Substring ( ( int ) StartIndex ) :
				file.ChangedFilename.Substring ( ( int ) StartIndex, ( int ) Length.Value );
			return true;
		}
	}
}

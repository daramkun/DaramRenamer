using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class SubstringProcessor : IProcessor
	{
		public string Name { get { return "process_substring_text"; } }

		[Globalized ( "start_index" )]
		public uint StartIndex { get; set; }
		[Globalized ( "substring_length" )]
		public uint? Length { get; set; }
		[Globalized ( "include_extension" )]
		public bool IncludeExtensions { get; set; }

		public SubstringProcessor () { StartIndex = 0; Length = null; IncludeExtensions = false; }
		public SubstringProcessor ( uint start, uint? length = null, bool includeExtensions = false )
		{
			StartIndex = start; Length = length;
			IncludeExtensions = includeExtensions;
		}

		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = Length == null ? file.ChangedFilename.Substring ( ( int ) StartIndex ) :
				file.ChangedFilename.Substring ( ( int ) StartIndex, ( int ) Length.Value );
			return true;
		}
	}
}

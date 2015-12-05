using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class SubstringProcessor : IProcessor
	{
		public uint StartIndex { get; set; }
		public uint? Length { get; set; }
		public bool IncludeExtensions { get; set; }

		public SubstringProcessor ( uint start, uint? length = null, bool includeExtensions = false)
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class DeleteFilenameProcessor : IProcessor
	{
		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = Path.GetExtension ( file.ChangedFilename );
			return true;
		}
	}
}

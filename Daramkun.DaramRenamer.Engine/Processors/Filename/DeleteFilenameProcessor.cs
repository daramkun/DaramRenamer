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
	public class DeleteFilenameProcessor : IProcessor
	{
		public string Name => "process_delete_text";
		public bool CannotMultithreadProcess => false;

		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = Path.GetExtension ( file.ChangedFilename );
			return true;
		}
	}
}

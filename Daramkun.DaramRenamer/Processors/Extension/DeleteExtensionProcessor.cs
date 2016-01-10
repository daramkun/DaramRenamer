using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	public class DeleteExtensionProcessor : IProcessor
	{
		public string Name { get { return "process_delete_extension"; } }

		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			return true;
		}
	}
}

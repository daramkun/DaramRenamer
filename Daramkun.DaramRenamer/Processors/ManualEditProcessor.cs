using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors
{
	[Serializable]
	public class ManualEditProcessor : IProcessor
	{
		public string Name => "process_manual_edit";

		public bool CannotMultithreadProcess => false;

		[Localized ( "changed_name", 0 )]
		public string ChangeName { get; set; }
		[Localized ( "changed_path", 1 )]
		public string ChangePath { get; set; }

		public FileInfo ProcessingFileInfo { get; set; }

		public bool Process ( FileInfo file )
		{
			file.ChangedFilename = ChangeName;
			file.ChangedPath = ChangePath;
			return true;
		}
	}
}

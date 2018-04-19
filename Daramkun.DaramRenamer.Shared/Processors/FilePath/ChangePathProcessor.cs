using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.FilePath
{
	[Serializable]
	public class ChangePathProcessor : IProcessor
	{
		public string Name => "process_change_path";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "path_text" )]
		public DirectoryInfo Path { get; set; } = new DirectoryInfo ( "C:\\" );

		public bool Process ( FileInfo file )
		{
			if ( !Path.Exists ) return false;
			file.ChangedPath = Path.FullName;
			return true;
		}
	}
}

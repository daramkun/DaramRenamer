using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.FilePath
{
	public class ChangePathProcessor : IProcessor
	{
		public string Name { get { return "process_change_path"; } }
		public bool CannotMultithreadProcess { get { return false; } }

		[Globalized ( "path_text", 0 )]
		public DirectoryInfo Path { get; set; } = new DirectoryInfo ( "C:\\" );

		public bool Process ( FileInfo file )
		{
			if ( !Path.Exists ) return false;
			file.ChangedPath = Path.FullName;
			return true;
		}
	}
}

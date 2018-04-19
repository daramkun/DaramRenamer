using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	[Serializable]
	public class AddExtensionProcessor : IProcessor
	{
		public string Name => "process_add_extension";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "extension" )]
		public string Extension { get; set; } = "";

		public bool Process ( FileInfo file )
		{
			if ( Extension == null || Extension == "" ) return false;
			if ( Extension [ 0 ] != '.' ) Extension = $".{Extension}";
			file.ChangedFilename += Extension;
			return true;
		}
	}
}

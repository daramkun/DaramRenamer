using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	[Serializable]
	public class ReplaceExtensionProcessor : IProcessor
	{
		public string Name => "process_change_extension";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "extension" )]
		public string Extension { get; set; } = "";

		public ReplaceExtensionProcessor () { Extension = ""; }
		public ReplaceExtensionProcessor ( string ext )
		{
			Extension = ext;
		}

		public bool Process ( FileInfo file )
		{
			if ( Extension == null || Extension == "" ) return false;
			if ( Extension [ 0 ] != '.' ) Extension = $".{Extension}";
			file.ChangedFilename = $"{ Path.GetFileNameWithoutExtension ( file.ChangedFilename ) }{ Extension }";
			return true;
		}
	}
}

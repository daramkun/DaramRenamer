using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	public class ReplaceExtensionProcessor : IProcessor
	{
		public string Extension { get; set; }

		public ReplaceExtensionProcessor ( string ext )
		{
			Extension = ext;
		}

		public bool Process ( FileInfo file )
		{
			if ( Extension == null ) return false;
			file.ChangedFilename = $"{ Path.GetFileNameWithoutExtension ( file.ChangedFilename ) }{ Extension }";
			return true;
		}
	}
}

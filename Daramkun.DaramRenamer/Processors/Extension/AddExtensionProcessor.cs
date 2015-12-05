using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	public class AddExtensionProcessor : IProcessor
	{
		public string Extension { get; set; }

		public AddExtensionProcessor ( string ext )
		{
			Extension = ext;
		}

		public bool Process ( FileInfo file )
		{
			if ( Extension == null ) return false;
			file.ChangedFilename += Extension;
			return true;
		}
	}
}

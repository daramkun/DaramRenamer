using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	public class AddExtensionAutomatedProcessor : IProcessor
	{
		public string Name { get { return "process_add_extension_automatically"; } }

		public bool Process ( FileInfo file )
		{
			throw new NotImplementedException ();
		}
	}
}

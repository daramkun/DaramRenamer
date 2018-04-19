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
	public class CasecastExtensionProcessor : IProcessor
	{
		public string Name => "process_casecast_extension";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "casecast" )]
		public CasecastBW Casecast { get; set; } = CasecastBW.AllToLowercase;

		public bool Process ( FileInfo file )
		{
			string filename = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			switch ( Casecast )
			{
				case CasecastBW.AllToUppercase:
					file.ChangedFilename = $"{filename}{ext.ToUpper ()}";
					break;
				case CasecastBW.AllToLowercase:
					file.ChangedFilename = $"{filename}{ext.ToLower ()}";
					break;

				default: return false;
			}

			return true;
		}
	}
}

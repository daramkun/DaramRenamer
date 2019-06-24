using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Daramee.FileTypeDetector;

namespace Daramkun.DaramRenamer.Processors.Extension
{
	[Serializable]
	public class AddExtensionAutomatedProcessor : IProcessor
	{
		static AddExtensionAutomatedProcessor ()
		{
			DetectorService.AddDetectors ( Assembly.GetAssembly ( typeof ( Daramkun.DaramRenamer.IProcessor ) ) );
		}

		public string Name => "process_add_extension_automatically";
		public bool CannotMultithreadProcess => false;

		public bool Process ( FileInfo file )
		{
			if ( !File.Exists ( file.OriginalFullPath ) ) return false;

			using ( Stream stream = File.OpenRead ( file.OriginalFullPath ) )
			{
				var detector = DetectorService.DetectDetector ( stream );
				if ( detector == null )
					return false;
				file.ChangedFilename = $"{file.ChangedFilename}.{detector.Extension}";
			}

			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class GIFDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] GIF_Signatures = new byte [] []
		{
			new byte [] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
			new byte [] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }
		};

		public override string Extension { get { return "gif"; } }

		protected override byte [] [] Signatures { get { return GIF_Signatures; } }
	}
}

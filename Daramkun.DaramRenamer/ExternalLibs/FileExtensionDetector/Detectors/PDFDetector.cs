using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class PDFDetector : SignatureTypeDetector
	{
		static byte [] PDF_Signature = new byte [] { 0x25, 0x50, 0x44, 0x46 };

		public override string Extension { get { return "pdf"; } }

		protected override byte [] Signature { get { return PDF_Signature; } }
	}
}

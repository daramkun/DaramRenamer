using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class ZIPDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] ZIP_Signatures = new byte [] []
		{
			new byte [] { 0x50, 0x4b, 0x03, 0x04 },			// Original ZIP
			new byte [] { 0x50, 0x4b, 0x05, 0x06 },			// Empty Archive
			new byte [] { 0x50, 0x4b, 0x07, 0x08 }			// Spanned Archive
		};

		public override string Extension { get { return "zip"; } }

		protected override byte [] [] Signatures { get { return ZIP_Signatures; } }
	}
}

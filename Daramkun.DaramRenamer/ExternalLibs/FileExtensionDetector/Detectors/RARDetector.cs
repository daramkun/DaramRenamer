using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class RARDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] RAR_Signatures = new byte [] []
		{
			new byte [] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 },
			new byte [] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 }
		};

		public override string Extension { get { return "rar"; } }

		protected override byte [] [] Signatures { get { return RAR_Signatures; } }
	}
}

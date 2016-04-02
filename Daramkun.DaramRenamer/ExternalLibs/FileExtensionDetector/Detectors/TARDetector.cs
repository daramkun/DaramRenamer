using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class TARDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] TAR_Signatures = new byte [] []
		{
			new byte [] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30 },
			new byte [] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00 },
		};

		public override string Extension { get { return "tar"; } }

		protected override byte [] [] Signatures { get { return TAR_Signatures; } }
	}
}

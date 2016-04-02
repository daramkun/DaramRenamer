using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class XARDetector : SignatureTypeDetector
	{
		static byte [] XAR_Signature = new byte [] { 0x78, 0x61, 0x72, 0x21 };

		public override string Extension { get { return "xar"; } }

		protected override byte [] Signature { get { return XAR_Signature; } }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class OGGDetector : SignatureTypeDetector
	{
		static byte [] OGG_Signature = new byte [] { 0x4F, 0x67, 0x67, 0x53 };

		public override string Extension { get { return "ogg"; } }

		protected override byte [] Signature { get { return OGG_Signature; } }
	}
}

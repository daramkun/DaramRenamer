using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class FLACDetector : SignatureTypeDetector
	{
		static byte [] FLAC_Signature = new byte [] { 0x66, 0x4C, 0x61, 0x43 };

		public override string Extension { get { return "flac"; } }

		protected override byte [] Signature { get { return FLAC_Signature; } }
	}
}

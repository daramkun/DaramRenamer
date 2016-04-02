using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class PSDDetector : SignatureTypeDetector
	{
		static byte [] PSD_Signature = new byte [] { 0x38, 0x42, 0x50, 0x53 };

		public override string Extension { get { return "psd"; } }

		protected override byte [] Signature { get { return PSD_Signature; } }
	}
}

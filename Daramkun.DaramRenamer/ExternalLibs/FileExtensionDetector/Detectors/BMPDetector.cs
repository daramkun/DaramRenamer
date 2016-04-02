using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class BMPDetector : SignatureTypeDetector
	{
		static byte [] BMP_Signature = new byte [] { 0x42, 0x4D };

		public override string Extension { get { return "bmp"; } }

		protected override byte [] Signature { get { return BMP_Signature; } }
	}
}

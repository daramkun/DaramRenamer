using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class TIFFDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] TIFF_Signatures = new byte [] []
		{
			new byte [] { 0x49, 0x49, 0x2A, 0x00 },			// Little Endian
			new byte [] { 0x4D, 0x4D, 0x00, 0x2A }			// Big Endian
		};

		public override string Extension { get { return "tif"; } }

		protected override byte [] [] Signatures { get { return TIFF_Signatures; } }
	}
}

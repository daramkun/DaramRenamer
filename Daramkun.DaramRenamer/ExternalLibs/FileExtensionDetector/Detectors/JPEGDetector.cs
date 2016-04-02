using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class JPEGDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] JPEG_Signatures = new byte [] []
		{
			new byte [] { 0xFF, 0xD8, 0xFF, 0xDB },		// Original JPEG
			new byte [] { 0xFF, 0xD8, 0xFF, 0xE0 },		// JPEG into JFIF
			new byte [] { 0xFF, 0xD8, 0xFF, 0xE1 }		// JPEG into EXIF
		};

		public override string Extension { get { return "jpg"; } }

		protected override byte [] [] Signatures { get { return JPEG_Signatures; } }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class MP3Detector : MultipleSignatureTypeDetector
	{
		static byte [] [] MP3_Signatures = new byte [] []
		{
			new byte [] { 0xFF, 0xFB },
			new byte [] { 0x49, 0x44, 0x33 },
		};

		public override string Extension { get { return "mp3"; } }

		protected override byte [] [] Signatures { get { return MP3_Signatures; } }
	}
}

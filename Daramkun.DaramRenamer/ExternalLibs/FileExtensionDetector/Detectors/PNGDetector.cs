using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class PNGDetector : SignatureTypeDetector
	{
		static byte [] PNG_Signature = new byte [] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

		public override string Extension { get { return "png"; } }

		protected override byte [] Signature { get { return PNG_Signature; } }
	}
}

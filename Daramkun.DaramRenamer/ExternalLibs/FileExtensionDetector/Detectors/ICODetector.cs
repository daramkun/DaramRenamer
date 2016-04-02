using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class ICODetector : SignatureTypeDetector
	{
		static byte [] ICO_Signature = new byte [] { 0x00, 0x00, 0x01, 0x00 };

		public override string Extension { get { return "ico"; } }

		protected override byte [] Signature { get { return ICO_Signature; } }
	}
}

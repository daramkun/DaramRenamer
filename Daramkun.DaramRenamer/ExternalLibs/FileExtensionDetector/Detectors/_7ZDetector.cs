using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class _7ZDetector : SignatureTypeDetector
	{
		static byte [] _7Z_Signature = new byte [] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };

		public override string Extension { get { return "7z"; } }

		protected override byte [] Signature { get { return _7Z_Signature; } }
	}
}

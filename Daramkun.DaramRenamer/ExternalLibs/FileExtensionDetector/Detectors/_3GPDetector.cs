using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class _3GPDetector : SignatureTypeDetector
	{
		static byte [] _3GP_Signature = new byte [] { 0x66, 0x74, 0x79, 0x70, 0x33, 0x67 };

		public override string Extension { get { return "3gp"; } }

		protected override byte [] Signature { get { return _3GP_Signature; } }
	}
}

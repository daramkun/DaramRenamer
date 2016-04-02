using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class CABDetector : SignatureTypeDetector
	{
		static byte [] CAB_Signature = new byte [] { 0x4D, 0x53, 0x43, 0x46 };

		public override string Extension { get { return "cab"; } }

		protected override byte [] Signature { get { return CAB_Signature; } }
	}
}

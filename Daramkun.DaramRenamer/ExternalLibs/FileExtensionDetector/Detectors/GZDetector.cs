using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class GZDetector : SignatureTypeDetector
	{
		static byte [] GZ_Signature = new byte [] { 0x1F, 0x8B };

		public override string Extension { get { return "gz"; } }

		protected override byte [] Signature { get { return GZ_Signature; } }
	}
}

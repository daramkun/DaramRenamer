using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class DMGDetector : SignatureTypeDetector
	{
		static byte [] DMG_Signature = new byte [] { 0x78, 0x01, 0x73, 0x0D, 0x62, 0x62, 0x60 };

		public override string Extension { get { return "dmg"; } }

		protected override byte [] Signature { get { return DMG_Signature; } }
	}
}

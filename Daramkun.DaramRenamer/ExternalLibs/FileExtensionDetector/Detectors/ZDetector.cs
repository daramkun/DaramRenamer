using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class ZDetector : MultipleSignatureTypeDetector
	{
		static byte [] [] Z_Signatures = new byte [] []
		{
			new byte [] { 0x1F, 0x9D },
			new byte [] { 0x1F, 0xA0 }
		};

		public override string Extension { get { return "z"; } }

		protected override byte [] [] Signatures { get { return Z_Signatures; } }
	}
}

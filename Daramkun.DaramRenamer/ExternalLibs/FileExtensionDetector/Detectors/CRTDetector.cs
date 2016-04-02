using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class CRTDetector : ExtDetector
	{
		static readonly string CRT_Signature = "-----BEGIN CERTIFICATE-----";

		public override string Extension { get { return "crt"; } }

		public override bool Detect ( Stream stream )
		{
			byte [] buffer = new byte [ CRT_Signature.Length ];
			stream.Read ( buffer, 0, CRT_Signature.Length );
			return CRT_Signature == Encoding.UTF8.GetString ( buffer );
		}
	}
}

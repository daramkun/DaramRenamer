using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionDetector.Detectors
{
	public class MIDIDetector : SignatureTypeDetector
	{
		static byte [] MIDI_Signatures = new byte [] { 0x4D, 0x54, 0x68, 0x64 };

		public override string Extension { get { return "mid"; } }

		protected override byte [] Signature { get { return MIDI_Signatures; } }
	}
}

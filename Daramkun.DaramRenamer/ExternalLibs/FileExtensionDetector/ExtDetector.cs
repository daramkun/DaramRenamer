using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExtensionDetector.Detectors;

namespace FileExtensionDetector
{
	public abstract class ExtDetector
	{
		public abstract string Extension { get; }
		public virtual string Precondition { get { return null; } }

		public abstract bool Detect ( Stream stream );

		public static IReadOnlyList<ExtDetector> Detectors { get; private set; } = new List<ExtDetector> ();
		private static Dictionary<string, List<ExtDetector>> SubDetectors { get; set; } = new Dictionary<string, List<ExtDetector>> ();

		static ExtDetector ()
		{
			var detectors = new ExtDetector [] {
				new ZIPDetector (), new RARDetector (), new PNGDetector (), new ICODetector (), new ZDetector (), new GIFDetector (),
				new _3GPDetector (), new TIFFDetector (), new JPEGDetector (), new APKDetector (), new CRTDetector (),
				new DOCXDetector (), new PPTXDetector (), new XLSXDetector (), new MP3Detector (), new FLACDetector (),
				new _7ZDetector (), new ICODetector (), new OGGDetector (), new TARDetector (), new XARDetector (), new GZDetector (),
				new PDFDetector (), new MIDIDetector (), new DMGDetector (), new CABDetector (), new EPUBDetector ()
			};
			foreach ( var detector in detectors )
				AddDetector ( detector );
		}

		public static void AddDetector<T> () where T : ExtDetector
		{
			var instance = Activator.CreateInstance<T> ();
			AddDetector ( instance );
		}

		public static void AddDetector ( ExtDetector instance )
		{
			if ( instance.Precondition == null )
				( Detectors as List<ExtDetector> ).Add ( instance );
			else
			{
				if ( SubDetectors.ContainsKey ( instance.Precondition ) )
					SubDetectors [ instance.Precondition ].Add ( instance );
				else
					SubDetectors.Add ( instance.Precondition, new List<ExtDetector> () { instance } );
			}
		}

		public static ExtDetector DetectDetector ( Stream stream )
		{
			ExtDetector foundDetector = null;
			foreach ( var detector in Detectors )
			{
				stream.Position = 0;
				if ( detector.Detect ( stream ) )
				{
					foundDetector = detector;

					if ( SubDetectors.ContainsKey ( foundDetector.Extension ) )
					{
						foreach ( var subDetector in SubDetectors [ foundDetector.Extension ] )
						{
							stream.Position = 0;
							if ( subDetector.Detect ( stream ) )
							{
								foundDetector = subDetector;
								break;
							}
						}
					}

					break;
				}
			}
			return foundDetector;
		}
	}

	public abstract class SignatureTypeDetector : ExtDetector
	{
		protected abstract byte [] Signature { get; }

		public override bool Detect ( Stream stream )
		{
			byte [] buffer = new byte [ Signature.Length ];
			stream.Read ( buffer, 0, Signature.Length );
			for ( int j = 0; j < Signature.Length; ++j )
			{
				if ( buffer [ j ] != Signature [ j ] )
					return false;
			}
			return true;
		}
	}

	public abstract class MultipleSignatureTypeDetector : ExtDetector
	{
		protected abstract byte [] [] Signatures { get; }

		public override bool Detect ( Stream stream )
		{
			int longestLength = 0;
			foreach ( var sig in Signatures )
				longestLength = ( longestLength < sig.Length ) ? sig.Length : longestLength;

			byte [] buffer = new byte [ longestLength ];
			stream.Read ( buffer, 0, longestLength );
			bool failed = true;
			for ( int i = 0; i < Signatures.Length && failed; ++i )
			{
				failed = false;
				for ( int j = 0; j < Signatures [ i ].Length; ++j )
				{
					if ( buffer [ j ] != Signatures [ i ] [ j ] )
					{
						failed = true;
						break;
					}
				}
			}

			return !failed;
		}
	}
}

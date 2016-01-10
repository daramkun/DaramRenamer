using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class CasecastProcessor : IProcessor
	{
		public string Name { get { return "process_casecast_text"; } }

		[Globalized ( "casecast" )]
		public Casecast Casecast { get; set; }

		public CasecastProcessor () { Casecast = Casecast.UppercaseFirstLetter; }
		public CasecastProcessor ( Casecast casecast ) { Casecast = casecast; }

		public bool Process ( FileInfo file )
		{
			string filename = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			switch ( Casecast)
			{
				case Casecast.AllToUppercase:
					file.ChangedFilename = $"{filename.ToUpper ()}{ext}";
					break;
				case Casecast.AllToLowercase:
					file.ChangedFilename = $"{filename.ToLower ()}{ext}";
					break;
				case Casecast.UppercaseFirstLetter:
					{
						string [] fn = filename.Split ( ' ' );
						for ( int i = 0; i < fn.Length; ++i )
						{
							char [] chars = fn [ i ].ToArray ();
							chars [ 0 ] = char.ToUpper ( chars [ 0 ] );
							fn [ i ] = new string ( chars );
						}
						file.ChangedFilename = string.Join ( " ", fn );
					}
					break;

				default: return false;
			}

			return true;
		}
	}
}

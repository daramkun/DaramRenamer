using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class TrimmingProcessor : IProcessor
	{
		public Position TrimPosition { get; set; }

		public TrimmingProcessor ( Position pos )
		{
			TrimPosition = pos;
		}

		public bool Process ( FileInfo file )
		{
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			fn = TrimPosition == Position.StartPoint ? fn.TrimStart () : ( TrimPosition == Position.EndPoint ? fn.TrimEnd () : fn.Trim () );
            file.ChangedFilename = $"{ fn }{ Path.GetExtension ( file.ChangedFilename ) }";
			return true;
		}

		public override string ToString ()
		{
			if ( CultureInfo.CurrentUICulture == CultureInfo.GetCultureInfo ( "ko-KR" ) ) return "양 옆 공백 제거";
			return "Trimming Filename";
		}
	}
}

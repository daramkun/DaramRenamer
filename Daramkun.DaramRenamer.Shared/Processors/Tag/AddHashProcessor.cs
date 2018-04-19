using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Tag
{
	[Serializable]
	public class AddHashProcessor : IProcessor
	{
		public string Name => "process_add_file_hash";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "hash_type" )]
		public HashType HashType { get; set; }
		[Argument ( Name = "hash_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			byte [] returnValue = null;
			switch ( HashType )
			{
				case HashType.MD5:
					using ( Stream stream = File.Open ( file.ChangedFullPath, FileMode.Open ) )
						returnValue = System.Security.Cryptography.MD5.Create ().ComputeHash ( stream );
					break;
				case HashType.SHA1:
					using ( Stream stream = File.Open ( file.ChangedFullPath, FileMode.Open ) )
						returnValue = System.Security.Cryptography.SHA1.Create ().ComputeHash ( stream );
					break;
				case HashType.SHA256:
					using ( Stream stream = File.Open ( file.ChangedFullPath, FileMode.Open ) )
						returnValue = System.Security.Cryptography.SHA256.Create ().ComputeHash ( stream );
					break;
				case HashType.SHA384:
					using ( Stream stream = File.Open ( file.ChangedFullPath, FileMode.Open ) )
						returnValue = System.Security.Cryptography.SHA384.Create ().ComputeHash ( stream );
					break;
				case HashType.SHA512:
					using ( Stream stream = File.Open ( file.ChangedFullPath, FileMode.Open ) )
						returnValue = System.Security.Cryptography.SHA512.Create ().ComputeHash ( stream );
					break;
			}

			StringBuilder sBuilder = new StringBuilder ();
			for ( int i = 0; i < returnValue.Length; i++ )
				sBuilder.Append ( returnValue [ i ].ToString ( "x2" ) );
			string hash = sBuilder.ToString ();

			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			file.ChangedFilename = Position == OnePointPosition.StartPoint ? $"{hash}{fn}{ext}" :
				( Position == OnePointPosition.EndPoint ? $"{fn}{hash}{ext}" :
				$"{hash}{fn}{hash}{ext}" );

			return true;
		}
	}
}

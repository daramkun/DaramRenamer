using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Daramee.DaramCommonLib;
using Daramee.Nargs;
using Daramkun.DaramRenamer.Processors;

namespace Daramkun.DaramRenamer
{
	class Program
	{
		class Argument
		{
			[Argument ( Name = "version", ShortName = "v" )]
			public bool PrintVersion { get; set; } = false;
			[Argument ( Name = "files", ShortName = "f" )]
			public string [] Files { get; set; }
			[Argument ( Name = "processor", ShortName = "p" )]
			public string Processor { get; set; }
			[Argument ( Name = "copymode", ShortName = "c" )]
			public bool IsCopyMode { get; set; } = false;
			[Argument ( Name = "overwrite", ShortName = "o" )]
			public bool IsOverwriteMode { get; set; } = false;

			[ArgumentStore]
			public Dictionary<string, string> RestArguments { get; set; }
		}

		private static bool IsSubtypeOf ( Type majorType, Type minorType )
		{
			if ( majorType == minorType || majorType.IsSubclassOf ( minorType ) )
				return true;
			else if ( minorType.IsInterface )
			{
				foreach ( Type type in majorType.GetInterfaces () )
					if ( type == minorType )
						return true;
			}
			return false;
		}

		static void Main ( string [] args )
		{
			Argument arg = ArgumentParser.Parse<Argument> ( args, ArgumentStyle.UNIXStyle );

			Assembly assembly = Assembly.GetExecutingAssembly ();
			if ( arg.PrintVersion )
			{
				var version = assembly.GetName ().Version;
				Console.WriteLine ( $"DaramRenamer v{0}.{1}{2}{3}", version.Major, version.Minor, version.Build, version.Revision );
			}

			if ( arg.Files == null || arg.Files.Length == 0 )
			{
				if ( !arg.PrintVersion )
				{
					if ( File.Exists ( "DaramRenamer.exe" ) )
						Process.Start ( "DaramRenamer.exe" );
				}
				return;
			}
			
			FileInfo.Files = new ObservableCollection<FileInfo> ();
			foreach ( string file in arg.Files )
				FileInfo.Files.Add ( new FileInfo ( file ) );

			foreach ( Type type in assembly.GetTypes () )
			{
				if ( type.IsInterface ) continue;
				if ( IsSubtypeOf ( type, typeof ( IProcessor ) ) )
				{
					IProcessor p = Activator.CreateInstance ( type ) as IProcessor;
					if ( arg.Processor == p.Name ||
						arg.Processor == $"processor_{p.Name}" )
					{
						List<string> argsList = new List<string> ();
						foreach ( var kv in arg.RestArguments )
						{
							argsList.Add ( kv.Key );
							argsList.Add ( kv.Value );
						}
						p = ArgumentParser.Parse ( type, argsList.ToArray (),
							ArgumentStyle.UNIXStyle, false ) as IProcessor;

						if ( p is BatchProcessor )
							if ( File.Exists ( ( p as BatchProcessor ).Script ) )
								( p as BatchProcessor ).Script = File.ReadAllText ( ( p as BatchProcessor ).Script );

						Daramee.Winston.File.Operation.Begin ( true );
						Parallel.ForEach ( FileInfo.Files, ( fileInfo ) =>
						{
							if ( p.Process ( fileInfo ) )
							{
								bool succeed = false;
								ErrorCode errorMessage;
								if ( arg.IsCopyMode )
									succeed = FileInfo.Copy ( fileInfo, arg.IsOverwriteMode,
										out errorMessage );
								else
									succeed = FileInfo.Move ( fileInfo, arg.IsOverwriteMode,
										out errorMessage );

								Console.WriteLine ( $"({( succeed ? "O" : "X" )}) {fileInfo.OriginalFilename} => {fileInfo.ChangedFilename}" );
							}
						} );
						Daramee.Winston.File.Operation.End ();
					}
				}
			}
		}
	}
}

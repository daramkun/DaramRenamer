using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors
{
	[Serializable]
	public class BatchProcessor : IProcessor
	{
		public string Name => "process_batch_process";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "script" )]
		public string Script { get; set; }

		public BatchProcessor () { }

        public bool Process ( FileInfo file )
		{
			Jint.Engine engine = new Jint.Engine ( cfg => cfg.AllowClr (
				Assembly.GetAssembly ( typeof ( TagLib.File ) ),
				Assembly.GetAssembly ( typeof ( Daramee.FileTypeDetector.IDetector ) ),
				Assembly.GetAssembly ( typeof ( Daramkun.DaramRenamer.IProcessor ) )
			) );
			engine.SetValue ( "file", file );

			var delegates = ProcessorExtensions.Delegates;
			foreach ( Delegate dele in delegates )
				engine.SetValue ( dele.Method.Name, dele );

			try
			{
				Jint.Engine proceed = engine.Execute ( Script );
				return proceed.GetCompletionValue ().AsBoolean ();
			}
			catch ( Exception ex )
			{
				System.Diagnostics.Debug.WriteLine ( ex.StackTrace );
				return false;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors
{
	[Serializable]
	public class BatchProcessor : IProcessor
	{
		public string Name => "process_batch_process";
		public bool CannotMultithreadProcess => false;

		public ObservableCollection<IBatchable> Processors { get; } = new ObservableCollection<IBatchable> ();
		
		public BatchProcessor () { }

        public bool Process ( FileInfo file )
		{
			ICondition condition = null;
			foreach ( var p in Processors )
			{
				if ( p is ICondition )
					condition = p as ICondition;
				else
				{
					if ( condition?.IsValid ( file ) == true )
					{
						if ( !( p as IProcessor ).Process ( file ) )
							return false;
					}
				}
			}
			return true;
		}
	}
}

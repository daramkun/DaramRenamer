﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors
{
	public class BatchProcessor : IProcessor
	{
		public ObservableCollection<IProcessor> Processors { get; } = new ObservableCollection<IProcessor> ();

		public BatchProcessor () { }

		public bool Process ( FileInfo file )
		{
			foreach ( var p in Processors )
				if ( !p.Process ( file ) )
					return false;
			return true;
		}
	}
}
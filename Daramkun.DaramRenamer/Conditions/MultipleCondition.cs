using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Conditions
{
	public class MultipleCondition : ICondition
	{
		public string Name => "multiple_condition";

		public ObservableCollection<ICondition> Conditions { get; } = new ObservableCollection<ICondition> ();

		public bool IsValid ( FileInfo file )
		{
			foreach ( var condition in Conditions )
				if ( !condition.IsValid ( file ) )
					return false;
			return true;
		}
	}
}

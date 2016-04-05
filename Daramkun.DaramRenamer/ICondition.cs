using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public interface ICondition
	{
		string Name { get; }
		bool IsValid ( FileInfo file );
	}

	public static class ConditionController
	{
		public static ObservableCollection<ICondition> Conditions { get; private set; } = new ObservableCollection<ICondition> ();

		public static bool IsValid ( FileInfo file )
		{
			foreach ( var condition in Conditions )
				if ( !condition.IsValid ( file ) )
					return false;
			return true;
		}
	}
}

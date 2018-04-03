using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Conditions
{
	public class NoOperationCondition : ICondition
	{
		public string Name => "nop";
		public bool IsValid ( FileInfo file ) { return true; }
	}
}

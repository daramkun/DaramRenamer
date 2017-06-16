using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	[AttributeUsage ( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
	public class LocalizedAttribute : Attribute
	{
		public string Field { get; set; }
		public uint Order { get; set; }
		public LocalizedAttribute ( string field, uint order = 0 ) { Field = field; Order = order; }
	}
}

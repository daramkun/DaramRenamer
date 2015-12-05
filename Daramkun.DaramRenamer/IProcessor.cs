using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public enum Position
	{
		StartPoint,
		EndPoint,
		BothPoint,
	}

	public interface IProcessor
	{
		bool Process ( FileInfo file );
	}
}

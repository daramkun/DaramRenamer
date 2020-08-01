using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public interface ICondition
	{
		bool IsSatisfyThisCondition(FileInfo file);
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public interface ITargetContains
	{
		void SetTargets(IEnumerable<FileInfo> files);
	}
}

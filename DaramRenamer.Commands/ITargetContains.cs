using System.Collections.Generic;

namespace DaramRenamer
{
	public interface ITargetContains
	{
		void SetTargets(IEnumerable<FileInfo> files);
	}
}

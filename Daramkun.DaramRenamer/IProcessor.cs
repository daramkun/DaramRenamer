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
	public enum OnePointPosition
	{
		StartPoint,
		EndPoint,
	}

	public enum Casecast
	{
		AllToUppercase,
		AllToLowercase,
		UppercaseFirstLetter,
	}

	public enum CasecastBW
	{
		AllToUppercase,
		AllToLowercase,
	}

	public interface IProcessor
	{
		string Name { get; }
		bool Process ( FileInfo file );
		bool CannotMultithreadProcess { get; }
	}
}

using System;
using System.IO;
using System.Linq;

namespace DaramRenamer.Conditions
{
	[Serializable, LocalizationKey ("Condition_Name_Extension")]
	public class ExtensionCondition : ICondition
	{
		[LocalizationKey("Condition_Argument_Extension_Extension")]
		public string Extension { get; set; } = "";

		public bool IsSatisfyThisCondition(FileInfo file)
		{
			return Extension.Split(',').Contains(Path.GetExtension(file.ChangedFilename));
		}
	}
}

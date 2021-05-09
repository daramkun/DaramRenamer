using System;

namespace DaramRenamer
{
	public class LocalizationKeyAttribute : Attribute
	{
		public string LocalizationKey { get; }

		public LocalizationKeyAttribute(string key)
		{
			LocalizationKey = key;
		}
	}
}

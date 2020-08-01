using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public class LocalizationKeyAttribute : Attribute
	{
		public string LocalizationKey { get; private set; }

		public LocalizationKeyAttribute(string key)
		{
			LocalizationKey = key;
		}
	}
}

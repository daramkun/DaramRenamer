using Daramee.Blockar;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public struct KeyBindingInfo
	{
		[FieldOption(Name = "key_binding")]
		public string KeyBinding;
		[FieldOption(Name = "command")]
		public string Command;
	}
}

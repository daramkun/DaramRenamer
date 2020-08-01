using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public interface IPluginInitializer
	{
		void Initialize();
		void Uninitialize();
	}
}

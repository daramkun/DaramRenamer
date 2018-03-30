using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Daramkun.DaramRenamer
{
	public interface ISubWindow
	{
		event RoutedEventHandler OKButtonClicked;
		event RoutedEventHandler CancelButtonClicked;

		IProcessor Processor { get; }
	}
}

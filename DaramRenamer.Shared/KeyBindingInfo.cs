using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using DaramRenamer.Annotations;

namespace DaramRenamer
{
	public class KeyBindingInfo : INotifyPropertyChanged
	{
		private string _keyBinding, _command;

		public string KeyBinding
		{
			get => _keyBinding;
			set
			{
				_keyBinding = value;
				OnPropertyChanged();
			}
		}

		public string Command
		{
			get => _command;
			set
			{
				_command = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CommandObject));
			}
		}

		public object CommandObject
		{
			get
			{
				Type commandType = null;
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypes())
					{
						if (type.FullName != _command)
							continue;

						commandType = type;
						break;
					}

					if (commandType != null)
						break;
				}

				return commandType != null ? Activator.CreateInstance(commandType) : null;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

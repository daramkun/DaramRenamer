using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
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
				OnPropertyChanged(nameof(KeyBindingKey));
				OnPropertyChanged(nameof(KeyBindingModifierKeys));
			}
		}

		public Key KeyBindingKey
		{
			get
			{
				if (string.IsNullOrEmpty(_keyBinding) || string.IsNullOrWhiteSpace(_keyBinding))
					return (Key) 0;
				var keyText = _keyBinding.Replace("Ctrl+", "").Replace("Alt+", "").Replace("Shift+", "");
				if (Enum.TryParse<Key>(keyText, out var result))
					return result;
				return (Key) 0;
			}
		}

		public ModifierKeys KeyBindingModifierKeys
		{
			get
			{
				if (string.IsNullOrEmpty(_keyBinding) || string.IsNullOrWhiteSpace(_keyBinding))
					return ModifierKeys.None;

				var modifierKeys = ModifierKeys.None;
				if (_keyBinding.Contains("Ctrl+"))
					modifierKeys |= ModifierKeys.Control;
				if (_keyBinding.Contains("Alt+"))
					modifierKeys |= ModifierKeys.Alt;
				if (_keyBinding.Contains("Shift+"))
					modifierKeys |= ModifierKeys.Shift;

				return modifierKeys;
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

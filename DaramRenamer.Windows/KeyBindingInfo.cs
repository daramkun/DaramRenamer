using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DaramRenamer.Registry;

namespace DaramRenamer;

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
                return 0;
            var keyText = _keyBinding.Replace("Ctrl+", "").Replace("Alt+", "").Replace("Shift+", "");
            return Enum.TryParse<Key>(keyText, out var result)
                ? result
                : 0;
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
            return DaramRenamerRegistry.FindCommandDescriptor(_command)?.Create();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

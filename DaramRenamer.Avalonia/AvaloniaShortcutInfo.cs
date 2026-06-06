using System;
using System.Collections.Generic;
using Avalonia.Input;
using DaramRenamer.Registry;

namespace DaramRenamer.Avalonia;

internal sealed class AvaloniaShortcutInfo
{
    public string KeyBinding { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;

    public bool Matches(KeyEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KeyBinding))
            return false;
        return TryParseGesture(KeyBinding, out var key, out var modifiers) &&
               key == e.Key &&
               modifiers == e.KeyModifiers;
    }

    public ICommand? CreateCommand() =>
        DaramRenamerRegistry.FindCommandDescriptor(Command)?.Create();

    public override string ToString()
    {
        var descriptor = DaramRenamerRegistry.FindCommandDescriptor(Command);
        var commandName = descriptor == null ? string.Empty : Strings.Instance[descriptor.LocalizationKey];
        return string.IsNullOrWhiteSpace(commandName)
            ? KeyBinding
            : $"{KeyBinding} - {commandName}";
    }

    public static bool TryParseGesture(string text, out Key key, out KeyModifiers modifiers)
    {
        key = Key.None;
        modifiers = KeyModifiers.None;

        foreach (var part in text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= KeyModifiers.Control;
            }
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= KeyModifiers.Alt;
            }
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= KeyModifiers.Shift;
            }
            else if (!Enum.TryParse(part == "Del" ? "Delete" : part, true, out key))
            {
                key = Key.None;
                modifiers = KeyModifiers.None;
                return false;
            }
        }

        return key != Key.None;
    }

    public static string FromKeyEvent(KeyEventArgs e)
    {
        var parts = new List<string>();
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        parts.Add(e.Key.ToString());
        return string.Join("+", parts);
    }
}

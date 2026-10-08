using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal static class ModalDialog
{
    public static void Show(Window dialog, Window? owner = null)
    {
        RunWithOwner(owner, () => dialog.ShowDialog(owner));
    }

    public static void RunWithOwner(Window? owner, Action action) =>
        RunWithOwner(owner, () =>
        {
            action();
            return true;
        });

    public static T RunWithOwner<T>(Window? owner, Func<T> action)
    {
        var windowsToRestore = Application.Current.AllWindows
            .Where(window => !ReferenceEquals(window, owner) && window.IsEnabled)
            .ToArray();

        foreach (var window in windowsToRestore)
            window.IsEnabled(false);

        try
        {
            return action();
        }
        finally
        {
            foreach (var window in windowsToRestore)
                if (window.Handle != 0)
                    window.IsEnabled(true);
        }
    }
}

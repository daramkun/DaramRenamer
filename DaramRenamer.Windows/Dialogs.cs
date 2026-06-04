using System.Windows;
using Microsoft.Win32;

namespace DaramRenamer;

public sealed class TaskDialogIcon
{
    public static TaskDialogIcon Warning { get; } = new(MessageBoxImage.Warning);
    public static TaskDialogIcon Error { get; } = new(MessageBoxImage.Error);
    public static TaskDialogIcon Information { get; } = new(MessageBoxImage.Information);

    private TaskDialogIcon(MessageBoxImage image)
    {
        Image = image;
    }

    public MessageBoxImage Image { get; }
}

public enum TaskDialogCommonButtonFlags
{
    OK,
    Cancel
}

public sealed class TaskDialogResult
{
    public static int Cancel { get; } = 0;
    public int Button { get; }

    public TaskDialogResult(int button)
    {
        Button = button;
    }
}

public static class TaskDialogButton
{
    public static string[] Cast(string[] buttons) => buttons;
}

public sealed class TaskDialog
{
    public string Title { get; set; } = string.Empty;
    public string MainInstruction { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TaskDialogIcon MainIcon { get; set; }
    public TaskDialogCommonButtonFlags CommonButtons { get; set; }
    public string[] Buttons { get; set; }

    public TaskDialogResult Show(Window owner)
    {
        var message = string.IsNullOrWhiteSpace(Content) ? MainInstruction : $"{MainInstruction}\n\n{Content}";
        var buttons = Buttons is { Length: > 0 } ? MessageBoxButton.YesNoCancel :
            CommonButtons == TaskDialogCommonButtonFlags.Cancel ? MessageBoxButton.OKCancel : MessageBoxButton.OK;
        var result = System.Windows.MessageBox.Show(owner, message, Title, buttons,
            MainIcon?.Image ?? MessageBoxImage.None);

        return result switch
        {
            MessageBoxResult.Yes or MessageBoxResult.OK => new TaskDialogResult(101),
            MessageBoxResult.No => new TaskDialogResult(102),
            _ => new TaskDialogResult(TaskDialogResult.Cancel)
        };
    }
}

public class FileDialog
{
    public string InitialDirectory { get; set; } = string.Empty;
    public string FileName { get; protected set; } = string.Empty;
    public string[] FileNames { get; protected set; } = [];

    public virtual bool? ShowDialog() => false;
}

public sealed class OpenFolderDialog : FileDialog
{
    public string Title { get; set; } = string.Empty;
    public bool AllowMultiSelection { get; set; }

    public override bool? ShowDialog()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = Title,
            InitialDirectory = InitialDirectory,
            Multiselect = AllowMultiSelection
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            FileName = dialog.FolderName;
            FileNames = dialog.FolderNames;
        }

        return result;
    }
}

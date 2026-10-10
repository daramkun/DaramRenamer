using Aprillz.MewUI;

namespace DaramRenamer.MewUI;

internal static class MultiFolderOpenDialog
{
    public static IReadOnlyList<string> Show(Window owner, string title)
    {
        var folder = FileDialog.SelectFolder(new FolderDialogOptions
        {
            Owner = owner,
            Title = title
        });

        return string.IsNullOrEmpty(folder) ? Array.Empty<string>() : [folder];
    }
}

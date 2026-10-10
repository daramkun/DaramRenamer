using Aprillz.MewUI;

namespace DaramRenamer.MewUI;

internal static class MultiFileOpenDialog
{
    public static IReadOnlyList<string> Show(Window owner, string title, string filter) =>
        FileDialog.OpenFiles(new OpenFileDialogOptions
        {
            Owner = owner,
            Title = title,
            Filters = FileFilter.Parse(filter),
            Multiselect = true
        }) ?? Array.Empty<string>();
}

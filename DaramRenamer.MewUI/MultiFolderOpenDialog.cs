using System.Runtime.InteropServices;
using Aprillz.MewUI;

namespace DaramRenamer.MewUI;

internal static class MultiFolderOpenDialog
{
    private static readonly Guid FileOpenDialogClassId = new("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");
    private const int Cancelled = unchecked((int)0x800704C7);
    private const uint FileSystemPath = 0x80058000;

    public static IReadOnlyList<string> Show(Window owner, string title)
    {
        var type = Type.GetTypeFromCLSID(FileOpenDialogClassId, throwOnError: true);
        var dialog = (IFileOpenDialog)Activator.CreateInstance(type!)!;
        IShellItemArray? results = null;

        try
        {
            Check(dialog.SetOptions(FileOpenDialogOptions.PickFolders |
                                    FileOpenDialogOptions.AllowMultiSelect |
                                    FileOpenDialogOptions.ForceFileSystem |
                                    FileOpenDialogOptions.PathMustExist));
            Check(dialog.SetTitle(title));

            var showResult = dialog.Show(owner.Handle);
            if (showResult == Cancelled)
                return Array.Empty<string>();
            Check(showResult);

            Check(dialog.GetResults(out results));
            Check(results.GetCount(out var count));

            var paths = new string[count];
            for (uint index = 0; index < count; ++index)
            {
                Check(results.GetItemAt(index, out var item));
                Check(item.GetDisplayName(FileSystemPath, out var path));
                paths[index] = path;
                Marshal.ReleaseComObject(item);
            }

            return paths;
        }
        finally
        {
            if (results is not null)
                Marshal.ReleaseComObject(results);
            Marshal.ReleaseComObject(dialog);
        }
    }

    private static void Check(int hresult)
    {
        if (hresult < 0)
            Marshal.ThrowExceptionForHR(hresult);
    }

    [Flags]
    private enum FileOpenDialogOptions : uint
    {
        PickFolders = 0x00000020,
        ForceFileSystem = 0x00000040,
        AllowMultiSelect = 0x00000200,
        PathMustExist = 0x00000800
    }

    [ComImport]
    [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        [PreserveSig] int Show(nint owner);
        [PreserveSig] int SetFileTypes(uint count, nint filters);
        [PreserveSig] int SetFileTypeIndex(uint index);
        [PreserveSig] int GetFileTypeIndex(out uint index);
        [PreserveSig] int Advise(nint events, out uint cookie);
        [PreserveSig] int Unadvise(uint cookie);
        [PreserveSig] int SetOptions(FileOpenDialogOptions options);
        [PreserveSig] int GetOptions(out FileOpenDialogOptions options);
        [PreserveSig] int SetDefaultFolder(IShellItem folder);
        [PreserveSig] int SetFolder(IShellItem folder);
        [PreserveSig] int GetFolder(out IShellItem folder);
        [PreserveSig] int GetCurrentSelection(out IShellItem item);
        [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
        [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
        [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
        [PreserveSig] int GetResult(out IShellItem item);
        [PreserveSig] int AddPlace(IShellItem item, uint placement);
        [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);
        [PreserveSig] int Close(int hresult);
        [PreserveSig] int SetClientGuid(ref Guid guid);
        [PreserveSig] int ClearClientData();
        [PreserveSig] int SetFilter(nint filter);
        [PreserveSig] int GetResults(out IShellItemArray items);
        [PreserveSig] int GetSelectedItems(out IShellItemArray items);
    }

    [ComImport]
    [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemArray
    {
        [PreserveSig] int BindToHandler(nint bindContext, ref Guid handlerId, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetPropertyStore(uint flags, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetPropertyDescriptionList(nint keyType, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetAttributes(uint flags, uint attributes, out uint result);
        [PreserveSig] int GetCount(out uint count);
        [PreserveSig] int GetItemAt(uint index, out IShellItem item);
        [PreserveSig] int EnumItems(out nint enumerator);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        [PreserveSig] int BindToHandler(nint bindContext, ref Guid handlerId, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetParent(out IShellItem parent);
        [PreserveSig] int GetDisplayName(uint displayNameType, [MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig] int GetAttributes(uint attributes, out uint result);
        [PreserveSig] int Compare(IShellItem item, uint hint, out int order);
    }
}

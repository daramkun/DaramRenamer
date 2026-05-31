using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DaramRenamer.Helpers;

[SupportedOSPlatform("windows")]
internal unsafe partial class WindowsNativeFileOperation : IDisposable
{
    private nint _fileOperation;

    public WindowsNativeFileOperation()
    {
        var hr = CoInitializeEx(0, 0x2);
        ThrowIfFailed(hr);

        hr = CoCreateInstance(CLSID_FileOperation, 0, 0x7, IID_IFileOperation, out _fileOperation);
        ThrowIfFailed(hr);

        if (_fileOperation == 0)
            throw new InvalidOperationException("Failed to create IFileOperation instance.");
    }

    ~WindowsNativeFileOperation()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_fileOperation != 0)
        {
            Marshal.Release(_fileOperation);
            _fileOperation = 0;
        }

        CoUninitialize();

        GC.SuppressFinalize(this);
    }

    public void SetOperationFlags(OperationFlags flags) =>
        Call(_fileOperation, VTableIndex_SetOperationFlags, flags);

    public void CopyItem(string sourcePath, string destinationPath, string? newName = null)
    {
        nint psiFrom = 0;
        nint psiTo = 0;
        var pszNewName = StringToCoTaskMemUni(newName);

        try
        {
            var hr = SHCreateItemFromParsingName(sourcePath, 0, IID_IShellItem, out psiFrom);
            ThrowIfFailed(hr);
            hr = SHCreateItemFromParsingName(destinationPath, 0, IID_IShellItem, out psiTo);
            ThrowIfFailed(hr);
            
            Call(_fileOperation, VTableIndex_CopyItem, psiFrom, psiTo, pszNewName);
        }
        finally
        {
            if (psiFrom != 0) Marshal.Release(psiFrom);
            if (psiTo != 0) Marshal.Release(psiTo);
            Marshal.FreeCoTaskMem(pszNewName);
        }
    }

    public void MoveItem(string sourcePath, string destinationPath, string? newName = null)
    {
        nint psiFrom = 0;
        nint psiTo = 0;
        var pszNewName = StringToCoTaskMemUni(newName);

        try
        {
            var hr = SHCreateItemFromParsingName(sourcePath, 0, IID_IShellItem, out psiFrom);
            ThrowIfFailed(hr);
            hr = SHCreateItemFromParsingName(destinationPath, 0, IID_IShellItem, out psiTo);
            ThrowIfFailed(hr);
            
            Call(_fileOperation, VTableIndex_MoveItem, psiFrom, psiTo, pszNewName);
        }
        finally
        {
            if (psiFrom != 0) Marshal.Release(psiFrom);
            if (psiTo != 0) Marshal.Release(psiTo);
            Marshal.FreeCoTaskMem(pszNewName);
        }
    }

    public void RenameItem(string sourcePath, string newName)
    {
        nint psiFrom = 0;
        var pszNewName = StringToCoTaskMemUni(newName);

        try
        {
            var hr = SHCreateItemFromParsingName(sourcePath, 0, IID_IShellItem, out psiFrom);
            ThrowIfFailed(hr);
            
            Call(_fileOperation, VTableIndex_RenameItem, psiFrom, pszNewName);
        }
        finally
        {
            if (psiFrom != 0) Marshal.Release(psiFrom);
            Marshal.FreeCoTaskMem(pszNewName);
        }
    }

    public void DeleteItem(string sourcePath)
    {
        nint psiFrom = 0;

        try
        {
            var hr = SHCreateItemFromParsingName(sourcePath, 0, IID_IShellItem, out psiFrom);
            ThrowIfFailed(hr);
            
            Call(_fileOperation, VTableIndex_DeleteItem, psiFrom);
        }
        finally
        {
            if (psiFrom != 0) Marshal.Release(psiFrom);
        }
    }

    public void PerformOperations()
    {
        Call(_fileOperation, VTableIndex_PerformOperations);
    }

    #region P/Invoke
    
    private static readonly Guid CLSID_FileOperation = new("3AD05575-8857-4850-9277-11B85BDB8E09");
    private static readonly Guid IID_IFileOperation = new("947AAB5F-0A5C-4C13-B4D6-4BF7836FC9F8");
    private static readonly Guid IID_IShellItem = new("43826D1E-E718-42EE-BC55-A1E261C37BFE");

    [LibraryImport("ole32", EntryPoint = "CoInitializeEx")]
    private static partial int CoInitializeEx(nint pvReserved, int dwCoInit);

    [LibraryImport("ole32", EntryPoint = "CoUninitialize")]
    private static partial void CoUninitialize();

    [LibraryImport("ole32", EntryPoint = "CoCreateInstance")]
    private static partial int CoCreateInstance(in Guid rclsid, nint pUnkOuter, uint dwClsContext, in Guid riid,
        out nint ppv);

    [LibraryImport("shell32", EntryPoint = "SHCreateItemFromParsingName", StringMarshalling = StringMarshalling.Utf16)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static partial int SHCreateItemFromParsingName(string pszPath, nint pbc, in Guid riid, out nint ppv);

    private const int VTableIndexBase = 3;

    private const int VTableIndex_CopyItem = VTableIndexBase + 3;
    private const int VTableIndex_MoveItem = VTableIndexBase + 10;
    private const int VTableIndex_RenameItem = VTableIndexBase + 8;
    private const int VTableIndex_DeleteItem = VTableIndexBase + 5;
    private const int VTableIndex_SetOperationFlags = VTableIndexBase + 12;
    private const int VTableIndex_PerformOperations = VTableIndexBase + 23;

    [Flags]
    internal enum OperationFlags : uint
    {
        AllowUndo = 0x0040,
        FilesOnly = 0x0080,
        NoConfirmation = 0x0010,
        NoConfirmMkDir = 0x0200,
        NoConnectedElements = 0x2000,
        NoCopySecurityAttribs = 0x0800,
        NoErrorUI = 0x0400,
        NoRecursion = 0x1000,
        RenameOnCollision = 0x0008,
        Silent = 0x0004,
        WantNukeWarning = 0x4000,
        AddUndoRecord = 0x20000000,
        NoSkipJunctions = 0x00010000,
        PreferHardLink = 0x00020000,
        ShowElevationPrompt = 0x00040000,
        EarlyFailure = 0x00100000,
        PreserveFileExtensions = 0x00200000,
        KeepNewerFile = 0x00400000,
        NoCopyHooks = 0x00800000,
        NoMinimizeBox = 0x01000000,
        MoveAclsAcrossVolumes = 0x02000000,
        DontDisplaySourcePath = 0x04000000,
        DontDisplayDestPath = 0x08000000,
        RecycleOnDelete = 0x00080000,
        RequireElevation = 0x10000000,
        CopyAsDownload = 0x40000000,
        DontDisplayLocations = 0x80000000,
    }
    
    #endregion
    
    #region P/Invoke Helpers

    private static void ThrowIfFailed(int hr)
    {
        if (hr < 0)
            Marshal.ThrowExceptionForHR(hr);
    }
    
    private static nint StringToCoTaskMemUni(string? s) =>
        s is null ? 0 : Marshal.StringToCoTaskMemUni(s);

    private static nint GetVTableFunction(nint comObject, int vTableIndex)
    {
        var p = (nint*)comObject;
        var vtbl = p[0];
        var slots = (nint*)vtbl;
        return slots[vTableIndex];
    }
    
    private static void Call(nint comObject, int vTableIndex)
    {
        var fn = GetVTableFunction(comObject, vTableIndex);
        var dele = (delegate* unmanaged<nint, int>)fn;
        var hr = dele(comObject);
        ThrowIfFailed(hr);
    }

    private static void Call<T>(nint comObject, int vTableIndex, T a)
    {
        var fn = GetVTableFunction(comObject, vTableIndex);
        var dele = (delegate* unmanaged<nint, T, int>)fn;
        var hr = dele(comObject, a);
        ThrowIfFailed(hr);
    }

    private static void Call<T1, T2>(nint comObject, int vTableIndex, T1 a, T2 b)
    {
        var fn = GetVTableFunction(comObject, vTableIndex);
        var dele = (delegate* unmanaged<nint, T1, T2, int>)fn;
        var hr = dele(comObject, a, b);
        ThrowIfFailed(hr);
    }

    private static void Call<T1, T2, T3>(nint comObject, int vTableIndex, T1 a, T2 b, T3 c)
    {
        var fn = GetVTableFunction(comObject, vTableIndex);
        var dele = (delegate* unmanaged<nint, T1, T2, T3, int>)fn;
        var hr = dele(comObject, a, b, c);
        ThrowIfFailed(hr);
    }

    private static void Call<T1, T2, T3, T4>(nint comObject, int vTableIndex, T1 a, T2 b, T3 c, T4 d)
    {
        var fn = GetVTableFunction(comObject, vTableIndex);
        var dele = (delegate* unmanaged<nint, T1, T2, T3, T4, int>)fn;
        var hr = dele(comObject, a, b, c, d);
        ThrowIfFailed(hr);
    }

    #endregion
}
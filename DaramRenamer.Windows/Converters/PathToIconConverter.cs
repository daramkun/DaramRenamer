using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DaramRenamer.Converters;

internal class PathToIconConverter : IValueConverter
{
    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_OPENICON = 0x000000002;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    private static readonly Dictionary<string, BitmapSource> cached = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var filename = value as string;
        string key = null;
        var doNotCache = false;
        if (File.GetAttributes(filename).HasFlag(FileAttributes.Directory))
        {
            if (cached.ContainsKey("?"))
                return cached["?"];
            key = "?";
        }
        else
        {
            var ext = Path.GetExtension(filename);

            if (ext is ".exe" or ".ico")
            {
                doNotCache = true;
            }
            else
            {
                if (cached.ContainsKey(ext))
                    return cached[ext];
                key = ext;
            }
        }

        var hIcon = File.GetAttributes(filename).HasFlag(FileAttributes.Directory)
            ? GetFolderIcon(filename)
            : GetFileIcon(filename);
        var icon = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty,
            BitmapSizeOptions.FromWidthAndHeight(16, 16));
        DestroyIcon(hIcon);
        if (icon.CanFreeze)
            icon.Freeze();

        if (!doNotCache)
            cached.Add(key, icon);

        return icon;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    [DllImport("Shell32.dll")]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi,
        uint cbFileInfo, uint uFlags);

    [DllImport("User32.dll")]
    private static extern int DestroyIcon(IntPtr hIcon);

    private static IntPtr GetFileIcon(string name)
    {
        SHGetFileInfo(name, FILE_ATTRIBUTE_NORMAL, out var shfi,
            (uint) Marshal.SizeOf(typeof(SHFILEINFO)),
            SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON);

        return shfi.hIcon;
    }

    private static IntPtr GetFolderIcon(string name)
    {
        SHGetFileInfo(name, FILE_ATTRIBUTE_DIRECTORY, out var shfi,
            (uint) Marshal.SizeOf(typeof(SHFILEINFO)),
            SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_OPENICON | SHGFI_SMALLICON);

        return shfi.hIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public const int NAMESIZE = 80;
        public readonly IntPtr hIcon;
        public readonly int iIcon;
        public readonly uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public readonly string szTypeName;
    }
}
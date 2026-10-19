using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace DaramRenamer.MewUI;

[SupportedOSPlatform("windows")]
internal static class WindowsShellIconCache
{
    private const uint ShgfiIcon = 0x000000100;
    private const uint ShgfiSmallIcon = 0x000000001;
    private const uint ShgfiOpenIcon = 0x000000002;
    private const uint ShgfiUseFileAttributes = 0x000000010;
    private const uint FileAttributeDirectory = 0x00000010;
    private const uint FileAttributeNormal = 0x00000080;

    private static readonly string CacheDirectory = Path.Combine(Path.GetTempPath(), "DaramRenamer.MewUI", "ShellIcons");
    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object CacheLock = new();

    public static string GetIconFile(string path, bool isDirectory)
    {
        string cacheKey;
        if (isDirectory)
            cacheKey = "<directory>";
        else
        {
            var extension = Path.GetExtension(path);
            cacheKey = extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
                       extension.Equals(".ico", StringComparison.OrdinalIgnoreCase)
                ? path
                : extension;
        }

        return Cache.GetOrAdd(cacheKey, _ => CreateIconFile(path, isDirectory, cacheKey));
    }

    private static string CreateIconFile(string path, bool isDirectory, string cacheKey)
    {
        lock (CacheLock)
        {
            Directory.CreateDirectory(CacheDirectory);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cacheKey)));
            var iconPath = Path.Combine(CacheDirectory, $"{hash}.png");
            if (File.Exists(iconPath))
                return iconPath;

            var flags = ShgfiIcon | ShgfiUseFileAttributes | ShgfiSmallIcon;
            if (isDirectory)
                flags |= ShgfiOpenIcon;

            var attributes = isDirectory ? FileAttributeDirectory : FileAttributeNormal;
            _ = SHGetFileInfo(path, attributes, out var fileInfo,
                (uint)Marshal.SizeOf<ShFileInfo>(), flags);
            if (fileInfo.Icon == IntPtr.Zero)
                return string.Empty;

            try
            {
                using var icon = Icon.FromHandle(fileInfo.Icon);
                using var bitmap = icon.ToBitmap();
                bitmap.Save(iconPath, ImageFormat.Png);
                return iconPath;
            }
            catch
            {
                if (File.Exists(iconPath))
                    File.Delete(iconPath);
                return string.Empty;
            }
            finally
            {
                DestroyIcon(fileInfo.Icon);
            }
        }
    }

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string path,
        uint fileAttributes,
        out ShFileInfo fileInfo,
        uint fileInfoSize,
        uint flags);

    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr icon);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileInfo
    {
        public IntPtr Icon;
        public int IconIndex;
        public uint Attributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string TypeName;
    }
}

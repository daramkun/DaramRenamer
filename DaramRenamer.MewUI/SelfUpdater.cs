using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Globalization;

namespace DaramRenamer.MewUI;

internal static class SelfUpdater
{
    private const string ApplyUpdateCommand = "--apply-update";
    private const string CleanupUpdateCommand = "--cleanup-update";
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static async Task DownloadAndStartAsync(
        UpdateInformation information,
        string runtimeIdentifier,
        int currentProcessId)
    {
        if (!IsSupportedRuntime(runtimeIdentifier))
            throw new PlatformNotSupportedException($"Updates are not available for {runtimeIdentifier}.");
        if (!IsReleaseVersion(information.StableLatestVersion))
            throw new InvalidDataException("The release version has an unexpected format.");
        if (!Uri.TryCreate(information.StableLatestUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidDataException("The update package URL is invalid.");

        var updateDirectory = Path.Combine(
            Path.GetTempPath(), $"DaramRenamer-update-{Guid.NewGuid():N}");
        var extractionDirectory = Path.Combine(updateDirectory, "extracted");
        Directory.CreateDirectory(extractionDirectory);

        try
        {
            var extension = runtimeIdentifier.StartsWith("win-", StringComparison.Ordinal) ? "zip" : "tar.gz";
            var archivePath = Path.Combine(updateDirectory, $"package.{extension}");
            using (var response = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await using var source = await response.Content.ReadAsStreamAsync();
                await using var destination = new FileStream(
                    archivePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await source.CopyToAsync(destination);
            }

            if (extension == "zip")
            {
                ZipFile.ExtractToDirectory(archivePath, extractionDirectory);
            }
            else
            {
                await using var archive = File.OpenRead(archivePath);
                await using var gzip = new GZipStream(archive, CompressionMode.Decompress);
                TarFile.ExtractToDirectory(gzip, extractionDirectory, overwriteFiles: false);
            }

            var namedPackageDirectory = Path.Combine(
                extractionDirectory, $"DaramRenamer-{information.StableLatestVersion}-{runtimeIdentifier}");
            var packageDirectory = Directory.Exists(namedPackageDirectory)
                ? namedPackageDirectory
                : extractionDirectory;

            var (payloadDirectory, targetDirectory, launchExecutable, launchTarget, helperExecutable) =
                GetUpdatePaths(packageDirectory, runtimeIdentifier);
            if (!File.Exists(helperExecutable))
                throw new InvalidDataException("The update archive does not contain the application executable.");
            EnsureUnixExecutable(helperExecutable);

            var startInfo = new ProcessStartInfo(helperExecutable)
            {
                WorkingDirectory = Path.GetDirectoryName(helperExecutable)!,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add(ApplyUpdateCommand);
            startInfo.ArgumentList.Add(currentProcessId.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(payloadDirectory);
            startInfo.ArgumentList.Add(targetDirectory);
            startInfo.ArgumentList.Add(launchExecutable);
            startInfo.ArgumentList.Add(launchTarget);
            startInfo.ArgumentList.Add(updateDirectory);

            if (Process.Start(startInfo) is null)
                throw new InvalidOperationException("The update process could not be started.");
        }
        catch
        {
            TryDeleteDirectory(updateDirectory);
            throw;
        }
    }

    public static bool TryRunCommand(string[] args, out int exitCode)
    {
        if (args.Length == 0)
        {
            exitCode = 0;
            return false;
        }

        if (args[0] == ApplyUpdateCommand)
        {
            exitCode = ApplyUpdate(args);
            return true;
        }

        if (args[0] == CleanupUpdateCommand)
        {
            exitCode = CleanupUpdate(args);
            return true;
        }

        exitCode = 0;
        return false;
    }

    private static int ApplyUpdate(string[] args)
    {
        if (args.Length != 7 ||
            !int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out var oldProcessId))
            return 2;

        string? payloadDirectory = null;
        string? targetDirectory = null;
        string? launchExecutable = null;
        string? launchTarget = null;
        string? updateDirectory = null;

        try
        {
            payloadDirectory = Path.GetFullPath(args[2]);
            targetDirectory = Path.GetFullPath(args[3]);
            launchExecutable = Path.GetFullPath(args[4]);
            launchTarget = Path.GetFullPath(args[5]);
            updateDirectory = Path.GetFullPath(args[6]);

            if (!IsSafeUpdateDirectory(updateDirectory) ||
                !IsWithinDirectory(updateDirectory, Environment.ProcessPath ?? string.Empty) ||
                !IsWithinDirectory(updateDirectory, payloadDirectory) ||
                !IsWithinDirectory(targetDirectory, launchExecutable) ||
                !Directory.Exists(payloadDirectory) ||
                !Directory.Exists(targetDirectory))
                throw new InvalidDataException("The update paths failed validation.");

            try
            {
                using var oldProcess = Process.GetProcessById(oldProcessId);
                oldProcess.WaitForExit();
            }
            catch (ArgumentException)
            {
                // The old application already exited before the helper started.
            }

            CopyDirectory(payloadDirectory, targetDirectory);

            LaunchUpdatedApplication(launchExecutable, launchTarget, targetDirectory);
            try
            {
                var cleanupInfo = new ProcessStartInfo(launchExecutable)
                {
                    WorkingDirectory = targetDirectory,
                    UseShellExecute = false
                };
                cleanupInfo.ArgumentList.Add(CleanupUpdateCommand);
                cleanupInfo.ArgumentList.Add(Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
                cleanupInfo.ArgumentList.Add(updateDirectory);
                Process.Start(cleanupInfo);
            }
            catch
            {
                // The updated application is already running; leftover temporary files are harmless.
            }

            return 0;
        }
        catch (Exception ex)
        {
            try
            {
                if (updateDirectory is not null && IsSafeUpdateDirectory(updateDirectory))
                    File.WriteAllText(Path.Combine(updateDirectory, "update-error.log"), ex.ToString());
            }
            catch
            {
                // Keep the update helper's exit path available even if logging fails.
            }

            try
            {
                if (launchExecutable is not null && launchTarget is not null && targetDirectory is not null)
                    LaunchUpdatedApplication(launchExecutable, launchTarget, targetDirectory);
            }
            catch
            {
                // The original application may no longer be runnable after a partial copy.
            }

            return 1;
        }
    }

    private static int CleanupUpdate(string[] args)
    {
        if (args.Length != 3 ||
            !int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out var helperProcessId))
            return 2;

        var updateDirectory = Path.GetFullPath(args[2]);
        if (!IsSafeUpdateDirectory(updateDirectory))
            return 2;

        try
        {
            using var helperProcess = Process.GetProcessById(helperProcessId);
            helperProcess.WaitForExit();
        }
        catch (ArgumentException)
        {
            // The update helper already exited.
        }

        TryDeleteDirectory(updateDirectory);
        return 0;
    }

    private static (string PayloadDirectory, string TargetDirectory, string LaunchExecutable,
        string LaunchTarget, string HelperExecutable) GetUpdatePaths(string packageDirectory, string runtimeIdentifier)
    {
        var executableName = OperatingSystem.IsWindows() ? "DaramRenamer.exe" : "DaramRenamer";
        if (runtimeIdentifier != "osx-arm64")
        {
            var executablePath = Path.Combine(packageDirectory, executableName);
            var targetDirectory = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
            return (packageDirectory, targetDirectory, Path.Combine(targetDirectory, executableName),
                Path.Combine(targetDirectory, executableName), executablePath);
        }

        var packageBundle = Path.Combine(packageDirectory, "DaramRenamer.app");
        var packageContents = Path.Combine(packageBundle, "Contents");
        var helperExecutable = Path.Combine(packageContents, "MacOS", executableName);
        var installedBundle = FindInstalledApplicationBundle();
        var targetContents = Path.Combine(installedBundle, "Contents");
        var launchExecutable = Path.Combine(targetContents, "MacOS", executableName);
        return (packageContents, targetContents, launchExecutable, installedBundle, helperExecutable);
    }

    private static string FindInstalledApplicationBundle()
    {
        var current = new DirectoryInfo(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));
        while (current is not null)
        {
            if (current.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("DaramRenamer must run from a .app bundle to install macOS updates.");
    }

    private static void LaunchUpdatedApplication(string executablePath, string launchTarget, string workingDirectory)
    {
        ProcessStartInfo startInfo;
        if (OperatingSystem.IsMacOS())
        {
            startInfo = new ProcessStartInfo("/usr/bin/open")
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("-n");
            startInfo.ArgumentList.Add(launchTarget);
        }
        else
        {
            startInfo = new ProcessStartInfo(executablePath)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false
            };
        }

        Process.Start(startInfo);
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var sourceSubdirectory in Directory.EnumerateDirectories(
                     sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceSubdirectory);
            var targetSubdirectory = Path.Combine(targetDirectory, relativePath);
            Directory.CreateDirectory(targetSubdirectory);
            CopyUnixMode(sourceSubdirectory, targetSubdirectory);
        }

        foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
            var targetFile = Path.Combine(targetDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            File.Copy(sourceFile, targetFile, overwrite: true);
            CopyUnixMode(sourceFile, targetFile);
        }
    }

    private static void CopyUnixMode(string sourcePath, string targetPath)
    {
        if (OperatingSystem.IsWindows())
            return;

        try
        {
            var mode = File.GetUnixFileMode(sourcePath);
            File.SetUnixFileMode(targetPath, mode);
        }
        catch (PlatformNotSupportedException)
        {
            // The package can still be installed on filesystems without Unix permission support.
        }
    }

    private static void EnsureUnixExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
            return;

        var mode = File.GetUnixFileMode(path);
        File.SetUnixFileMode(path, mode | UnixFileMode.UserExecute);
    }

    private static bool IsSafeUpdateDirectory(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var tempRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
        return string.Equals(Path.GetDirectoryName(fullPath), tempRoot,
                   OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) &&
               Path.GetFileName(fullPath).StartsWith("DaramRenamer-update-", StringComparison.Ordinal);
    }

    private static bool IsWithinDirectory(string directory, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var relativePath = Path.GetRelativePath(Path.GetFullPath(directory), Path.GetFullPath(path));
        return relativePath != "." &&
               !Path.IsPathRooted(relativePath) &&
               relativePath != ".." &&
               !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}",
                   OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private static bool IsSupportedRuntime(string runtimeIdentifier) =>
        runtimeIdentifier is "win-x64" or "win-arm64" or "linux-x64" or "osx-arm64";

    private static bool IsReleaseVersion(string version)
    {
        var parts = version.Split('.');
        return parts.Length == 3 && parts.All(part =>
            part.Length > 0 && part.All(char.IsAsciiDigit));
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Temporary update files can be removed on the next run or by the operating system.
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DaramRenamer", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        return client;
    }
}

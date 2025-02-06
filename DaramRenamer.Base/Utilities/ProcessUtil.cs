using System.Diagnostics;
using System.Text;

namespace DaramRenamer.Utilities;

public static class ProcessUtil
{
    public static string? StartProcessAndReturnStdOut(string processPath, string arguments, string workingDirectory)
    {
        var processInfo = new ProcessStartInfo(processPath, arguments)
        {
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.UTF8,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };
        
        Process? process;
        try
        {
            process = Process.Start(processInfo);
            process?.WaitForExit();
            if (process?.ExitCode != 0)
                return null;
        }
        catch
        {
            return null;
        }
        
        return process.StandardOutput.ReadToEnd().Trim();
    }
}
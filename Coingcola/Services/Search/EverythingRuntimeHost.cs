using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Coingcola.Services.Search
{
    public static class EverythingRuntimeHost
    {
        private static readonly object SyncRoot = new();

        public static bool TryEnsureStarted()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (Process.GetProcessesByName("Everything").Any())
                    {
                        return true;
                    }

                    var exe = EverythingRuntimeLocator.FindEverythingExe();
                    if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
                    {
                        return false;
                    }

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = "-startup -minimized",
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(exe) ?? AppContext.BaseDirectory
                    };

                    Process.Start(startInfo);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
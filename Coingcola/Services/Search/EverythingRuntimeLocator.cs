using System;
using System.IO;
using System.Linq;

namespace Coingcola.Services.Search
{
    internal static class EverythingRuntimeLocator
    {
        public static string? FindRuntimeRoot()
        {
            string[] candidates =
            {
                Path.Combine(AppContext.BaseDirectory, "Resources", "Everything"),
                Path.Combine(AppContext.BaseDirectory, "Everything"),
                Path.Combine(AppContext.BaseDirectory, "tools", "Everything"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Everything")
            };

            return candidates.FirstOrDefault(Directory.Exists);
        }

        public static string? FindEsExe()
        {
            var root = FindRuntimeRoot();
            if (string.IsNullOrWhiteSpace(root))
            {
                return null;
            }

            var path = Path.Combine(root, "es.exe");
            return File.Exists(path) ? path : null;
        }

        public static string? FindEverythingExe()
        {
            var root = FindRuntimeRoot();
            if (string.IsNullOrWhiteSpace(root))
            {
                return null;
            }

            var preferred = Environment.Is64BitProcess ? "Everything64.exe" : "Everything.exe";
            var preferredPath = Path.Combine(root, preferred);
            if (File.Exists(preferredPath))
            {
                return preferredPath;
            }

            var fallback = Path.Combine(root, "Everything.exe");
            return File.Exists(fallback) ? fallback : null;
        }

        public static string? FindIniPath()
        {
            var root = FindRuntimeRoot();
            if (string.IsNullOrWhiteSpace(root))
            {
                return null;
            }

            var ini = Path.Combine(root, "Everything.ini");
            return File.Exists(ini) ? ini : null;
        }
    }
}
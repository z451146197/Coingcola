using Coingcola.Models.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coingcola.Services.Search
{
    public sealed class EverythingQueryService
    {
        public async Task<IReadOnlyList<LocalSearchHit>> SearchAsync(string keyword, int maxResults = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Array.Empty<LocalSearchHit>();
            }

            var everythingResults = await TrySearchWithEverythingAsync(keyword, maxResults, cancellationToken);
            if (everythingResults.Count > 0)
            {
                return everythingResults;
            }

            return await Task.Run(() => SearchFileSystemFallback(keyword, maxResults, cancellationToken), cancellationToken);
        }

        private async Task<IReadOnlyList<LocalSearchHit>> TrySearchWithEverythingAsync(string keyword, int maxResults, CancellationToken cancellationToken)
        {
            try
            {
                EverythingRuntimeHost.TryEnsureStarted();

                var esExe = EverythingRuntimeLocator.FindEsExe();
                if (string.IsNullOrWhiteSpace(esExe) || !File.Exists(esExe))
                {
                    return Array.Empty<LocalSearchHit>();
                }

                var psi = new ProcessStartInfo
                {
                    FileName = esExe,
                    Arguments = $"-n {Math.Max(1, maxResults)} -sort-path \"{keyword}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
                {
                    return Array.Empty<LocalSearchHit>();
                }

                var lines = output
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(maxResults)
                    .ToList();

                return lines.Select(ToHit).ToList();
            }
            catch
            {
                return Array.Empty<LocalSearchHit>();
            }
        }

        private IReadOnlyList<LocalSearchHit> SearchFileSystemFallback(string keyword, int maxResults, CancellationToken cancellationToken)
        {
            var results = new List<LocalSearchHit>();
            var roots = new List<string>();

            try
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (Directory.Exists(desktop)) roots.Add(desktop);
                if (Directory.Exists(documents)) roots.Add(documents);
                if (Directory.Exists(downloads)) roots.Add(downloads);
            }
            catch
            {
                // ignore
            }

            foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var name = Path.GetFileName(file);
                        if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(ToHit(file));
                            if (results.Count >= maxResults)
                            {
                                return results;
                            }
                        }
                    }
                }
                catch
                {
                    // ignore unauthorized / IO exceptions
                }
            }

            return results;
        }

        private static LocalSearchHit ToHit(string fullPath)
        {
            var title = Path.GetFileName(fullPath);
            var extension = Path.GetExtension(fullPath)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
            var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;

            return new LocalSearchHit
            {
                Title = title,
                FullPath = fullPath,
                Subtitle = directory,
                Kind = string.IsNullOrWhiteSpace(extension) ? "file" : extension
            };
        }
    }
}
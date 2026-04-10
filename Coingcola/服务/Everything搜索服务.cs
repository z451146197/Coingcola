using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coingcola.服务
{
    public enum Everything状态
    {
        未安装 = 1,
        未启动 = 2,
        启动中 = 3,
        可用 = 4,
        异常 = 5,
        查询超时 = 6,
        仅主程序可用 = 7
    }

    public sealed class Everything搜索服务
    {
        private string? _everythingExe;
        private string? _everythingCli;
        private DateTime _缓存时间 = DateTime.MinValue;

        public Everything状态 获取状态()
        {
            刷新路径缓存();

            if (!string.IsNullOrWhiteSpace(_everythingCli))
            {
                return Everything状态.可用;
            }

            if (!string.IsNullOrWhiteSpace(_everythingExe))
            {
                return Everything状态.仅主程序可用;
            }

            return Everything状态.未安装;
        }

        public string 获取状态提示()
        {
            return 获取状态() switch
            {
                Everything状态.可用 => "已接入 Everything，文件搜索优先由内置 Everything 引擎提供",
                Everything状态.仅主程序可用 => "已发现 Everything 主程序，当前可直接唤起 Everything 搜索",
                Everything状态.启动中 => "正在连接 Everything",
                Everything状态.查询超时 => "Everything 查询超时",
                Everything状态.异常 => "Everything 查询异常",
                _ => "未检测到 Everything，本地文件结果将仅保留应用、设置与快捷动作"
            };
        }

        public bool 可以查询()
        {
            return 获取状态() == Everything状态.可用;
        }

        public bool 可以唤起Everything()
        {
            var 状态 = 获取状态();
            return 状态 == Everything状态.可用 || 状态 == Everything状态.仅主程序可用;
        }

        public string? 获取建议内置目录()
        {
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Resources", "Everything"));
        }

        public bool 尝试启动()
        {
            刷新路径缓存();

            if (Process.GetProcessesByName("Everything").Any())
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(_everythingExe) || !File.Exists(_everythingExe))
            {
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _everythingExe,
                    Arguments = "-startup -minimized",
                    WorkingDirectory = Path.GetDirectoryName(_everythingExe) ?? AppContext.BaseDirectory,
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void 在Everything中继续搜索(string query)
        {
            刷新路径缓存();

            if (string.IsNullOrWhiteSpace(_everythingExe) || !File.Exists(_everythingExe))
            {
                throw new FileNotFoundException("未检测到 Everything.exe。");
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = _everythingExe,
                Arguments = $"-newwindow -s \"{EscapeArg(query)}\"",
                WorkingDirectory = Path.GetDirectoryName(_everythingExe) ?? AppContext.BaseDirectory,
                UseShellExecute = true
            });
        }

        public async Task<List<统一搜索结果项>> 查询文件与文件夹Async(string query, CancellationToken cancellationToken)
        {
            刷新路径缓存();

            if (string.IsNullOrWhiteSpace(_everythingCli) || !File.Exists(_everythingCli))
            {
                return new List<统一搜索结果项>();
            }

            bool 刚启动 = false;
            if (!Process.GetProcessesByName("Everything").Any())
            {
                刚启动 = 尝试启动();
                if (刚启动)
                {
                    await Task.Delay(220, cancellationToken);
                }
            }

            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _everythingCli,
                    Arguments = $"-n 24 -sort-path \"{EscapeArg(query)}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.Start();

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(TimeSpan.FromMilliseconds(650));
                await process.WaitForExitAsync(linkedCts.Token);

                string output = await process.StandardOutput.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(output))
                {
                    return new List<统一搜索结果项>();
                }

                var result = new List<统一搜索结果项>();
                foreach (string line in output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string fullPath = line.Trim();
                    if (string.IsNullOrWhiteSpace(fullPath))
                    {
                        continue;
                    }

                    bool isDirectory = Directory.Exists(fullPath);
                    bool isFile = File.Exists(fullPath);
                    if (!isDirectory && !isFile)
                    {
                        continue;
                    }

                    string title = Path.GetFileName(fullPath);
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = fullPath;
                    }

                    string subtitle = isDirectory
                        ? $"文件夹 · {Path.GetDirectoryName(fullPath) ?? fullPath}"
                        : $"Everything 文件结果 · {Path.GetDirectoryName(fullPath) ?? fullPath}";

                    result.Add(new 统一搜索结果项
                    {
                        Id = $"everything::{fullPath}",
                        标题 = title,
                        副标题 = subtitle,
                        来源 = "Everything",
                        目标 = fullPath,
                        命中说明 = "Everything 文件结果",
                        主动作文案 = isDirectory ? "打开文件夹" : "打开文件",
                        类型 = isDirectory ? 统一搜索结果类型.文件夹 : 统一搜索结果类型.文件,
                        次动作列表 = new List<统一搜索次动作>
                        {
                            new() { 类型 = 统一搜索次动作类型.打开位置, 文案 = "打开位置" },
                            new() { 类型 = 统一搜索次动作类型.固定到常用, 文案 = "固定到常用" },
                            new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                        }
                    });
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                return new List<统一搜索结果项>();
            }
            catch
            {
                return new List<统一搜索结果项>();
            }
        }

        private void 刷新路径缓存()
        {
            if (DateTime.Now - _缓存时间 < TimeSpan.FromMinutes(2))
            {
                return;
            }

            _缓存时间 = DateTime.Now;
            _everythingExe = 查找EverythingExe();
            _everythingCli = 查找EverythingCli();
        }

        private string? 查找EverythingExe()
        {
            string[] exeNames = Environment.Is64BitProcess
                ? new[] { "Everything64.exe", "Everything.exe" }
                : new[] { "Everything.exe", "Everything64.exe" };

            foreach (string root in 获取候选Everything根目录())
            {
                foreach (string exeName in exeNames)
                {
                    string path = Path.Combine(root, exeName);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        private string? 查找EverythingCli()
        {
            foreach (string root in 获取候选Everything根目录())
            {
                string path = Path.Combine(root, "es.exe");
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private IEnumerable<string> 获取候选Everything根目录()
        {
            string baseDir = AppContext.BaseDirectory;
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return new[]
            {
                Path.Combine(baseDir, "Resources", "Everything"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "Everything")),

                Path.Combine(baseDir, "third_party", "Everything"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "third_party", "Everything")),

                Path.Combine(programFiles, "Everything"),
                Path.Combine(programFilesX86, "Everything"),
                Path.Combine(localApp, "Programs", "Everything")
            }
            .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private string EscapeArg(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\\\"");
        }
    }
}
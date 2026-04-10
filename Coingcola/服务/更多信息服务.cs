using Coingcola.模型;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;

namespace Coingcola.服务
{
    /// <summary>
    /// 更多信息服务。
    /// 
    /// 当前职责：
    /// 1. 读取网络扩展信息
    /// 2. 读取固定磁盘概览
    /// 3. 读取软件环境信息
    /// 4. 读取系统目录与运行环境
    /// </summary>
    public class 更多信息服务
    {
        public 更多信息页面数据 获取页面数据()
        {
            var 网络信息 = 读取网络信息();
            List<string> 磁盘列表 = 读取固定磁盘列表();

            var data = new 更多信息页面数据
            {
                页面结论 = 生成页面结论(网络信息.活动网卡, 磁盘列表.Count),
                设备名称 = Environment.MachineName,
                当前用户 = 读取当前用户(),
                系统名称 = 读取系统名称(),
                系统版本 = 读取系统版本(),

                活动网卡 = 网络信息.活动网卡,
                IP地址 = 网络信息.IP地址,
                MAC地址 = 网络信息.MAC地址,
                DNS服务器 = 网络信息.DNS服务器,
                默认网关 = 网络信息.默认网关,

                固定磁盘数量 = 磁盘列表.Count.ToString(),
                固定磁盘概览 = 拼接多行文本(磁盘列表, "未读取到"),
                固定磁盘列表 = 磁盘列表,

                DotNet版本 = 读取DotNet版本(),
                PowerShell版本 = 读取PowerShell版本(),
                Winget状态 = 读取Winget状态(),

                Windows目录 = Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                系统目录 = Environment.SystemDirectory,
                临时目录 = Path.GetTempPath(),
                当前目录 = AppDomain.CurrentDomain.BaseDirectory,
                进程架构 = 读取进程架构()
            };

            data.分组列表 = new List<更多信息分组>
            {
                new 更多信息分组
                {
                    标题 = "网络扩展信息",
                    简介 = "用于快速确认当前网络连接和基础网络参数。",
                    项列表 = new List<更多信息项>
                    {
                        new() { 名称 = "活动网卡", 值 = data.活动网卡, 说明 = "当前启用且带 IP 的网卡。" },
                        new() { 名称 = "IP 地址", 值 = data.IP地址, 说明 = "当前优先识别到的 IPv4/IPv6 地址。" },
                        new() { 名称 = "MAC 地址", 值 = data.MAC地址, 说明 = "当前活动网卡的物理地址。" },
                        new() { 名称 = "DNS 服务器", 值 = data.DNS服务器, 说明 = "当前网络使用的 DNS 服务器。" },
                        new() { 名称 = "默认网关", 值 = data.默认网关, 说明 = "当前网络默认网关。" }
                    }
                },
                new 更多信息分组
                {
                    标题 = "存储扩展信息",
                    简介 = "用于快速查看本机固定磁盘数量和容量分布。",
                    项列表 = new List<更多信息项>
                    {
                        new() { 名称 = "固定磁盘数量", 值 = data.固定磁盘数量, 说明 = "当前已就绪的固定磁盘数量。" },
                        new() { 名称 = "固定磁盘概览", 值 = data.固定磁盘概览, 说明 = "当前固定磁盘容量与剩余空间概览。" }
                    }
                },
                new 更多信息分组
                {
                    标题 = "软件环境",
                    简介 = "用于快速查看当前机器的基础运行环境。",
                    项列表 = new List<更多信息项>
                    {
                        new() { 名称 = ".NET 版本", 值 = data.DotNet版本, 说明 = "当前进程环境识别到的 .NET 运行版本。" },
                        new() { 名称 = "PowerShell 版本", 值 = data.PowerShell版本, 说明 = "当前机器识别到的 PowerShell 版本。" },
                        new() { 名称 = "Winget 状态", 值 = data.Winget状态, 说明 = "用于软件安装和更新能力判断。" }
                    }
                },
                new 更多信息分组
                {
                    标题 = "系统目录与运行环境",
                    简介 = "用于快速定位系统目录和当前程序运行目录。",
                    项列表 = new List<更多信息项>
                    {
                        new() { 名称 = "Windows 目录", 值 = data.Windows目录, 说明 = "Windows 根目录。" },
                        new() { 名称 = "系统目录", 值 = data.系统目录, 说明 = "System32 等系统文件目录。" },
                        new() { 名称 = "临时目录", 值 = data.临时目录, 说明 = "当前用户临时文件目录。" },
                        new() { 名称 = "当前目录", 值 = data.当前目录, 说明 = "当前程序运行基础目录。" },
                        new() { 名称 = "进程架构", 值 = data.进程架构, 说明 = "当前进程与操作系统架构。" }
                    }
                }
            };

            return data;
        }

        private string 生成页面结论(string 活动网卡, int 磁盘数量)
        {
            if (string.IsNullOrWhiteSpace(活动网卡) || 活动网卡 == "未读取到")
            {
                return "当前已读取到系统扩展信息，但网络活动信息不完整，建议结合网络状态进一步确认。";
            }

            if (磁盘数量 <= 0)
            {
                return "当前已读取到系统扩展信息，但固定磁盘概览不完整，建议进一步检查存储状态。";
            }

            return "当前设备的网络、存储和软件环境信息已完成读取，可继续用于排查、留档或环境确认。";
        }

        private string 读取当前用户()
        {
            try
            {
                return $"{Environment.UserDomainName}\\{Environment.UserName}";
            }
            catch
            {
                return Environment.UserName;
            }
        }

        private string 读取系统名称()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string caption = obj["Caption"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(caption))
                    {
                        return caption.Trim();
                    }
                }
            }
            catch
            {
            }

            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                return key?.GetValue("ProductName")?.ToString() ?? "Windows";
            }
            catch
            {
                return "Windows";
            }
        }

        private string 读取系统版本()
        {
            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string displayVersion = key?.GetValue("DisplayVersion")?.ToString() ?? "";
                string build = key?.GetValue("CurrentBuildNumber")?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(displayVersion) && !string.IsNullOrWhiteSpace(build))
                {
                    return $"{displayVersion} · Build {build}";
                }

                if (!string.IsNullOrWhiteSpace(build))
                {
                    return $"Build {build}";
                }
            }
            catch
            {
            }

            return "未读取到";
        }

        private (string 活动网卡, string IP地址, string MAC地址, string DNS服务器, string 默认网关) 读取网络信息()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Description, IPAddress, MACAddress, DNSServerSearchOrder, DefaultIPGateway, IPEnabled FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string desc = obj["Description"]?.ToString() ?? "未读取到";
                    string mac = obj["MACAddress"]?.ToString() ?? "未读取到";

                    string ip = "未读取到";
                    if (obj["IPAddress"] is string[] ipArray && ipArray.Length > 0)
                    {
                        ip = string.Join(" / ", ipArray);
                    }

                    string dns = "未读取到";
                    if (obj["DNSServerSearchOrder"] is string[] dnsArray && dnsArray.Length > 0)
                    {
                        dns = string.Join(" / ", dnsArray);
                    }

                    string gateway = "未读取到";
                    if (obj["DefaultIPGateway"] is string[] gwArray && gwArray.Length > 0)
                    {
                        gateway = string.Join(" / ", gwArray);
                    }

                    return (desc.Trim(), ip, mac.Trim(), dns, gateway);
                }
            }
            catch
            {
            }

            return ("未读取到", "未读取到", "未读取到", "未读取到", "未读取到");
        }

        private List<string> 读取固定磁盘列表()
        {
            var 列表 = new List<string>();

            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (!drive.IsReady)
                    {
                        continue;
                    }

                    if (drive.DriveType != DriveType.Fixed)
                    {
                        continue;
                    }

                    string total = 格式化字节((ulong)drive.TotalSize);
                    string free = 格式化字节((ulong)drive.AvailableFreeSpace);
                    列表.Add($"{drive.Name} 总 {total} / 可用 {free}");
                }
            }
            catch
            {
            }

            return 列表;
        }

        private string 读取DotNet版本()
        {
            try
            {
                return Environment.Version.ToString();
            }
            catch
            {
                return "未读取到";
            }
        }

        private string 读取PowerShell版本()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-NoProfile -Command \"$PSVersionTable.PSVersion.ToString()\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (process == null)
                {
                    return "未读取到";
                }

                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(2000);

                return string.IsNullOrWhiteSpace(output) ? "未读取到" : output;
            }
            catch
            {
                return "未读取到";
            }
        }

        private string 读取Winget状态()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "winget",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (process == null)
                {
                    return "未检测到";
                }

                process.WaitForExit(2000);
                return process.ExitCode == 0 ? "已安装" : "未检测到";
            }
            catch
            {
                return "未检测到";
            }
        }

        private string 读取进程架构()
        {
            try
            {
                string processArch = RuntimeInformation.ProcessArchitecture.ToString();
                string osArch = RuntimeInformation.OSArchitecture.ToString();
                return $"进程 {processArch} / 系统 {osArch}";
            }
            catch
            {
                return "未读取到";
            }
        }

        private string 拼接多行文本(List<string> 列表, string 默认值)
        {
            if (列表 == null || 列表.Count == 0)
            {
                return 默认值;
            }

            return string.Join(Environment.NewLine, 列表);
        }

        private string 格式化字节(ulong bytes)
        {
            const double GB = 1024d * 1024d * 1024d;
            const double TB = 1024d * 1024d * 1024d * 1024d;

            if (bytes >= TB)
            {
                return (bytes / TB).ToString("0.##") + " TB";
            }

            return (bytes / GB).ToString("0.##") + " GB";
        }
    }
}
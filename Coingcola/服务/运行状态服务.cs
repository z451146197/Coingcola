using Coingcola.模型;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;

namespace Coingcola.服务
{
    /// <summary>
    /// 运行状态服务。
    /// 
    /// 当前策略：
    /// 1. 先做基础监控快照，不做高频实时轮询
    /// 2. 优先输出用户最关心的 CPU、内存、系统盘、运行时长
    /// 3. 后续可平滑扩展为实时监控版
    /// </summary>
    public class 运行状态服务
    {
        public 运行状态信息 获取运行状态()
        {
            DateTime? 启动时间 = 读取启动时间();
            TimeSpan? 运行时长 = 启动时间.HasValue ? DateTime.Now - 启动时间.Value : null;

            string cpu名称 = 读取CPU名称();
            string cpu负载 = 读取CPU负载();
            string cpu核心线程 = 读取CPU核心线程();

            (ulong totalMemory, ulong freeMemory) = 读取内存数据();
            ulong usedMemory = totalMemory > freeMemory ? totalMemory - freeMemory : 0;

            (string driveName, ulong totalDrive, ulong freeDrive) = 读取系统盘数据();
            ulong usedDrive = totalDrive > freeDrive ? totalDrive - freeDrive : 0;

            string totalMemoryText = totalMemory > 0 ? 格式化字节(totalMemory) : "未读取到";
            string usedMemoryText = totalMemory > 0 ? 格式化字节(usedMemory) : "未读取到";
            string freeMemoryText = totalMemory > 0 ? 格式化字节(freeMemory) : "未读取到";
            string memoryUsagePercent = totalMemory > 0
                ? $"{Math.Round((double)usedMemory / totalMemory * 100, 1)}%"
                : "未读取到";

            string totalDriveText = totalDrive > 0 ? 格式化字节(totalDrive) : "未读取到";
            string freeDriveText = totalDrive > 0 ? 格式化字节(freeDrive) : "未读取到";
            string driveUsagePercent = totalDrive > 0
                ? $"{Math.Round((double)usedDrive / totalDrive * 100, 1)}%"
                : "未读取到";

            string 显卡信息 = 读取显卡信息();
            string 系统名称 = 读取系统名称();
            string 系统版本 = 读取系统版本();

            var data = new 运行状态信息
            {
                页面结论 = 生成页面结论(cpu负载, memoryUsagePercent, driveUsagePercent),
                最近启动时间 = 启动时间.HasValue ? 启动时间.Value.ToString("yyyy-MM-dd HH:mm:ss") : "未读取到",
                运行时长 = 格式化运行时长(运行时长),

                CPU名称 = cpu名称,
                CPU当前负载 = cpu负载,
                CPU核心线程 = cpu核心线程,

                内存总量 = totalMemoryText,
                内存已用 = usedMemoryText,
                内存可用 = freeMemoryText,
                内存使用率 = memoryUsagePercent,

                系统盘名称 = driveName,
                系统盘总量 = totalDriveText,
                系统盘可用 = freeDriveText,
                系统盘使用率 = driveUsagePercent,

                显卡信息 = 显卡信息,
                系统名称 = 系统名称,
                系统版本 = 系统版本
            };

            data.分组列表 = new List<运行状态分组>
            {
                new 运行状态分组
                {
                    标题 = "系统运行",
                    简介 = "当前系统运行基础状态。",
                    项列表 = new List<运行状态项>
                    {
                        new() { 名称 = "系统", 值 = data.系统名称, 说明 = data.系统版本 },
                        new() { 名称 = "最近启动时间", 值 = data.最近启动时间, 说明 = "当前系统最近一次启动时间。" },
                        new() { 名称 = "运行时长", 值 = data.运行时长, 说明 = "从上次开机到现在的累计运行时间。" }
                    }
                },
                new 运行状态分组
                {
                    标题 = "CPU 与内存",
                    简介 = "当前设备核心算力资源快照。",
                    项列表 = new List<运行状态项>
                    {
                        new() { 名称 = "CPU", 值 = data.CPU名称, 说明 = data.CPU核心线程 },
                        new() { 名称 = "CPU 当前负载", 值 = data.CPU当前负载, 说明 = "当前 CPU 使用率快照。" },
                        new() { 名称 = "内存总量", 值 = data.内存总量, 说明 = "当前设备总物理内存。" },
                        new() { 名称 = "内存已用", 值 = data.内存已用, 说明 = $"可用内存：{data.内存可用}" },
                        new() { 名称 = "内存使用率", 值 = data.内存使用率, 说明 = "当前内存占用比例。" }
                    }
                },
                new 运行状态分组
                {
                    标题 = "存储状态",
                    简介 = "当前系统盘容量与占用情况。",
                    项列表 = new List<运行状态项>
                    {
                        new() { 名称 = "系统盘", 值 = string.IsNullOrWhiteSpace(data.系统盘名称) ? "未读取到" : data.系统盘名称, 说明 = $"总量：{data.系统盘总量}" },
                        new() { 名称 = "系统盘可用", 值 = data.系统盘可用, 说明 = "当前系统盘剩余空间。" },
                        new() { 名称 = "系统盘使用率", 值 = data.系统盘使用率, 说明 = "当前系统盘占用比例。" }
                    }
                },
                new 运行状态分组
                {
                    标题 = "图形与显示",
                    简介 = "当前识别到的显卡信息。",
                    项列表 = new List<运行状态项>
                    {
                        new() { 名称 = "显卡", 值 = data.显卡信息, 说明 = "当前识别到的显卡列表。" }
                    }
                }
            };

            return data;
        }

        private string 生成页面结论(string cpuUsage, string memoryUsage, string driveUsage)
        {
            double cpu = 解析百分比(cpuUsage);
            double memory = 解析百分比(memoryUsage);
            double drive = 解析百分比(driveUsage);

            if (cpu >= 90 || memory >= 90 || drive >= 95)
            {
                return "当前设备资源占用偏高，建议重点关注 CPU、内存或系统盘空间。";
            }

            if (cpu >= 70 || memory >= 75 || drive >= 85)
            {
                return "当前设备运行基本正常，但部分资源已进入较高占用区间。";
            }

            return "当前设备运行状态总体正常，可继续查看更细的系统与硬件信息。";
        }

        private DateTime? 读取启动时间()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string raw = obj["LastBootUpTime"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        return ManagementDateTimeConverter.ToDateTime(raw);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private string 读取CPU名称()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["Name"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }
            }
            catch
            {
            }

            return "未读取到";
        }

        private string 读取CPU核心线程()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string cores = obj["NumberOfCores"]?.ToString() ?? "";
                    string threads = obj["NumberOfLogicalProcessors"]?.ToString() ?? "";

                    if (!string.IsNullOrWhiteSpace(cores) && !string.IsNullOrWhiteSpace(threads))
                    {
                        return $"{cores} 核 / {threads} 线程";
                    }
                }
            }
            catch
            {
            }

            return "核心线程信息未读取到";
        }

        private string 读取CPU负载()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string load = obj["LoadPercentage"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(load))
                    {
                        return load.Trim() + "%";
                    }
                }
            }
            catch
            {
            }

            return "未读取到";
        }

        private (ulong 总量, ulong 可用) 读取内存数据()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    ulong totalKb = 0;
                    ulong freeKb = 0;

                    if (obj["TotalVisibleMemorySize"] != null)
                    {
                        ulong.TryParse(obj["TotalVisibleMemorySize"].ToString(), out totalKb);
                    }

                    if (obj["FreePhysicalMemory"] != null)
                    {
                        ulong.TryParse(obj["FreePhysicalMemory"].ToString(), out freeKb);
                    }

                    return (totalKb * 1024, freeKb * 1024);
                }
            }
            catch
            {
            }

            return (0, 0);
        }

        private (string 盘符, ulong 总量, ulong 可用) 读取系统盘数据()
        {
            try
            {
                string systemRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                var drive = new DriveInfo(systemRoot);

                if (!drive.IsReady)
                {
                    return (drive.Name, 0, 0);
                }

                return (drive.Name, (ulong)drive.TotalSize, (ulong)drive.AvailableFreeSpace);
            }
            catch
            {
                return ("", 0, 0);
            }
        }

        private string 读取显卡信息()
        {
            var 列表 = new List<string>();
            var 去重集合 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["Name"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    if (去重集合.Add(text))
                    {
                        列表.Add(text);
                    }
                }
            }
            catch
            {
            }

            if (列表.Count == 0)
            {
                return "未读取到";
            }

            return string.Join(Environment.NewLine, 列表);
        }

        private string 读取系统名称()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["Caption"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
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

        private string 格式化运行时长(TimeSpan? 时长)
        {
            if (!时长.HasValue)
            {
                return "未读取到";
            }

            TimeSpan value = 时长.Value;
            int days = value.Days;
            int hours = value.Hours;
            int minutes = value.Minutes;

            if (days > 0)
            {
                return $"{days}天 {hours}小时 {minutes}分钟";
            }

            if (hours > 0)
            {
                return $"{hours}小时 {minutes}分钟";
            }

            return $"{minutes}分钟";
        }

        private double 解析百分比(string 文本)
        {
            if (string.IsNullOrWhiteSpace(文本))
            {
                return 0;
            }

            string value = 文本.Replace("%", "").Trim();
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
                ? result
                : 0;
        }

        private string 格式化字节(ulong bytes)
        {
            const double GB = 1024d * 1024d * 1024d;
            const double TB = 1024d * 1024d * 1024d * 1024d;

            if (bytes >= TB)
            {
                return (bytes / TB).ToString("0.##", CultureInfo.InvariantCulture) + " TB";
            }

            return (bytes / GB).ToString("0.##", CultureInfo.InvariantCulture) + " GB";
        }
    }
}
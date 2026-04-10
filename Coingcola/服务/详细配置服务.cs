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
    /// 详细配置服务。
    /// 
    /// 当前职责：
    /// 1. 读取系统、硬件、存储、网络的详细配置
    /// 2. 组装为“详细配置”页面需要的数据结构
    /// 3. 保持展示友好，不做纯参数堆砌
    /// </summary>
    public class 详细配置服务
    {
        public 详细配置页面数据 获取页面数据()
        {
            List<string> 显卡列表 = 读取显卡列表();
            List<string> 磁盘列表 = 读取磁盘列表();

            var 数据 = new 详细配置页面数据
            {
                设备名称 = Environment.MachineName,
                系统名称 = 读取系统名称(),
                系统版本 = 读取系统版本(),
                系统类型 = 读取系统类型(),
                当前用户 = 读取当前用户(),

                CPU名称 = 读取CPU名称(),
                CPU核心信息 = 读取CPU核心信息(),
                内存总量 = 读取内存总量(),
                显卡名称 = 拼接多行文本(显卡列表, "未读取到"),
                主板信息 = 读取主板信息(),
                BIOS信息 = 读取BIOS信息(),

                设备厂商 = 读取设备厂商(),
                设备型号 = 读取设备型号(),
                系统盘信息 = 读取系统盘信息(),
                磁盘概览 = 拼接多行文本(磁盘列表, "未读取到"),

                活动网卡 = 读取活动网卡(),
                IP地址 = 读取IP地址(),
                MAC地址 = 读取MAC地址(),

                显卡列表 = 显卡列表,
                磁盘列表 = 磁盘列表
            };

            数据.分组列表 = new List<详细配置分组>
            {
                new 详细配置分组
                {
                    标题 = "系统信息",
                    简介 = "用于确认当前设备的系统基础信息。",
                    项列表 = new List<详细配置项>
                    {
                        new() { 名称 = "设备名称", 值 = 数据.设备名称, 说明 = "当前主机名称。" },
                        new() { 名称 = "系统名称", 值 = 数据.系统名称, 说明 = "当前操作系统名称。" },
                        new() { 名称 = "系统版本", 值 = 数据.系统版本, 说明 = "当前系统版本与构建号。" },
                        new() { 名称 = "系统类型", 值 = 数据.系统类型, 说明 = "例如 x64-based PC。" },
                        new() { 名称 = "当前用户", 值 = 数据.当前用户, 说明 = "当前登录用户。" }
                    }
                },
                new 详细配置分组
                {
                    标题 = "核心硬件",
                    简介 = "用于快速确认当前机器的核心硬件配置。",
                    项列表 = new List<详细配置项>
                    {
                        new() { 名称 = "CPU", 值 = 数据.CPU名称, 说明 = 数据.CPU核心信息 },
                        new() { 名称 = "内存", 值 = 数据.内存总量, 说明 = "当前设备总物理内存。" },
                        new() { 名称 = "显卡", 值 = 数据.显卡名称, 说明 = 数据.显卡列表.Count > 1 ? $"当前共识别到 {数据.显卡列表.Count} 个显卡设备。" : "当前识别到的显卡信息。" },
                        new() { 名称 = "主板", 值 = 数据.主板信息, 说明 = "当前主板厂商与型号。" },
                        new() { 名称 = "BIOS", 值 = 数据.BIOS信息, 说明 = "当前 BIOS 版本。" },
                        new() { 名称 = "设备厂商", 值 = 数据.设备厂商, 说明 = "整机厂商信息。" },
                        new() { 名称 = "设备型号", 值 = 数据.设备型号, 说明 = "整机型号信息。" }
                    }
                },
                new 详细配置分组
                {
                    标题 = "存储信息",
                    简介 = "用于确认系统盘和当前磁盘概览。",
                    项列表 = new List<详细配置项>
                    {
                        new() { 名称 = "系统盘", 值 = 数据.系统盘信息, 说明 = "当前系统所在磁盘容量概览。" },
                        new() { 名称 = "磁盘概览", 值 = 数据.磁盘概览, 说明 = 数据.磁盘列表.Count > 1 ? $"当前共识别到 {数据.磁盘列表.Count} 个固定磁盘分区概览项。" : "当前磁盘概览。" }
                    }
                },
                new 详细配置分组
                {
                    标题 = "网络信息",
                    简介 = "用于快速查看当前活动网络信息。",
                    项列表 = new List<详细配置项>
                    {
                        new() { 名称 = "活动网卡", 值 = 数据.活动网卡, 说明 = "当前启用且带 IP 的网卡。" },
                        new() { 名称 = "IP 地址", 值 = 数据.IP地址, 说明 = "当前优先识别到的 IPv4/IPv6 地址。" },
                        new() { 名称 = "MAC 地址", 值 = 数据.MAC地址, 说明 = "当前活动网卡 MAC 地址。" }
                    }
                }
            };

            return 数据;
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

        private string 读取系统类型()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SystemType FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["SystemType"]?.ToString() ?? "";
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

        private string 读取CPU核心信息()
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

            return "核心信息未读取到";
        }

        private string 读取内存总量()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["TotalPhysicalMemory"] != null &&
                        ulong.TryParse(obj["TotalPhysicalMemory"].ToString(), out ulong bytes))
                    {
                        return 格式化字节(bytes);
                    }
                }
            }
            catch
            {
            }

            return "未读取到";
        }

        private List<string> 读取显卡列表()
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

            return 列表;
        }

        private string 读取主板信息()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    string product = obj["Product"]?.ToString() ?? "";
                    string result = $"{manufacturer} {product}".Trim();

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch
            {
            }

            return "未读取到";
        }

        private string 读取BIOS信息()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["SMBIOSBIOSVersion"]?.ToString() ?? "";
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

        private string 读取设备厂商()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["Manufacturer"]?.ToString() ?? "";
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

        private string 读取设备型号()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["Model"]?.ToString() ?? "";
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

        private string 读取系统盘信息()
        {
            try
            {
                string systemRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                var drive = new DriveInfo(systemRoot);

                if (!drive.IsReady)
                {
                    return $"{drive.Name} 未就绪";
                }

                string 总量 = 格式化字节((ulong)drive.TotalSize);
                string 可用 = 格式化字节((ulong)drive.AvailableFreeSpace);

                return $"{drive.Name} · 总 {总量} · 可用 {可用}";
            }
            catch
            {
                return "未读取到";
            }
        }

        private List<string> 读取磁盘列表()
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

        private string 读取活动网卡()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Description, IPEnabled FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["Description"]?.ToString() ?? "";
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

        private string 读取IP地址()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT IPAddress, IPEnabled FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");

                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["IPAddress"] is string[] ipArray && ipArray.Length > 0)
                    {
                        return string.Join(" / ", ipArray);
                    }
                }
            }
            catch
            {
            }

            return "未读取到";
        }

        private string 读取MAC地址()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT MACAddress, IPEnabled FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string text = obj["MACAddress"]?.ToString() ?? "";
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
                return (bytes / TB).ToString("0.##", CultureInfo.InvariantCulture) + " TB";
            }

            return (bytes / GB).ToString("0.##", CultureInfo.InvariantCulture) + " GB";
        }
    }
}
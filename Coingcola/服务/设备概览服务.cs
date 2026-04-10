using Coingcola.模型;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;
using System.Security.Principal;

namespace Coingcola.服务
{
    /// <summary>
    /// 设备概览服务。
    /// 
    /// 当前职责：
    /// 1. 读取系统名称与版本
    /// 2. 读取 CPU / 内存 / 显卡 / 系统盘等关键概览信息
    /// 3. 读取主机名、当前用户、权限状态、运行时长
    /// 4. 提供“看看这台电脑”页面需要的摘要信息
    /// </summary>
    public class 设备概览服务
    {
        public 设备概览信息 获取设备概览()
        {
            DateTime? 启动时间 = 读取启动时间();
            TimeSpan? 运行时长 = 启动时间.HasValue ? DateTime.Now - 启动时间.Value : null;

            List<string> 显卡列表 = 读取显卡列表();
            List<string> 磁盘列表 = 读取磁盘列表();

            return new 设备概览信息
            {
                设备名称 = Environment.MachineName,
                系统名称 = 读取系统名称(),
                系统版本 = 读取系统版本(),
                当前用户 = 读取当前用户(),
                权限状态 = 是否为管理员运行() ? "管理员模式" : "普通模式",
                运行结论 = 生成运行结论(),

                CPU名称 = 读取CPU名称(),
                内存总量 = 读取内存总量(),
                显卡名称 = 拼接多行文本(显卡列表, "未读取到"),
                系统盘信息 = 读取系统盘信息(),
                主机名 = Environment.MachineName,
                运行时长 = 格式化运行时长(运行时长),

                主板信息 = 读取主板信息(),
                BIOS信息 = 读取BIOS信息(),
                系统类型 = 读取系统类型(),
                上次启动时间 = 启动时间.HasValue ? 启动时间.Value.ToString("yyyy-MM-dd HH:mm:ss") : "未读取到",
                设备厂商 = 读取设备厂商(),
                设备型号 = 读取设备型号(),

                显卡列表 = 显卡列表,
                磁盘列表 = 磁盘列表
            };
        }

        private string 生成运行结论()
        {
            string 权限 = 是否为管理员运行() ? "当前为管理员模式" : "当前为普通模式";
            string 系统 = 读取系统名称();
            return $"{系统} 已正常识别，{权限}。可继续查看硬件与系统概览。";
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

                return "版本信息未读取到";
            }
            catch
            {
                return "版本信息未读取到";
            }
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

        private bool 是否为管理员运行()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private string 读取CPU名称()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name.Trim();
                    }
                }
            }
            catch
            {
            }

            return "未读取到";
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
                    string name = obj["Name"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    if (去重集合.Add(name))
                    {
                        列表.Add(name);
                    }
                }
            }
            catch
            {
            }

            return 列表;
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

                    string 总量 = 格式化字节((ulong)drive.TotalSize);
                    string 可用 = 格式化字节((ulong)drive.AvailableFreeSpace);
                    列表.Add($"{drive.Name} 总 {总量} / 可用 {可用}");
                }
            }
            catch
            {
            }

            return 列表;
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

        private string 读取主板信息()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    string product = obj["Product"]?.ToString() ?? "";

                    string text = $"{manufacturer} {product}".Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
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
                    string bios = obj["SMBIOSBIOSVersion"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(bios))
                    {
                        return bios.Trim();
                    }
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
                    string type = obj["SystemType"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(type))
                    {
                        return type.Trim();
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
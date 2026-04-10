using Coingcola.模型;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace Coingcola.服务
{
    /// <summary>
    /// 驱动与激活服务。
    /// 
    /// 当前职责：
    /// 1. 读取 Windows 激活状态
    /// 2. 读取更准确的系统名称与版本信息
    /// 3. 扫描当前存在异常状态的 PnP 设备
    /// 4. 提供系统设置跳转入口
    /// 
    /// 本版优化重点：
    /// - 不再使用 slmgr，避免卡顿与乱码
    /// - 不再把“读不到产品密钥”当成未激活
    /// - 对激活状态采用更保守的判定策略，避免误报“未激活”
    /// - 激活状态与驱动扫描都做短时缓存，减少系统调用
    /// </summary>
    public class 驱动与激活服务
    {
        /// <summary>
        /// Windows 授权 ApplicationID。
        /// 用于从 SoftwareLicensingProduct 中筛出真正的 Windows 授权项。
        /// </summary>
        private const string WindowsApplicationId = "55c92734-d682-4d71-983e-d6ec3f16059f";

        private static readonly TimeSpan 激活缓存时长 = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan 驱动缓存时长 = TimeSpan.FromMinutes(1);

        private static 激活状态信息? _激活状态缓存;
        private static DateTime _激活状态缓存时间;

        private static List<驱动问题项>? _驱动问题缓存;
        private static DateTime _驱动问题缓存时间;

        /// <summary>
        /// 获取当前 Windows 激活状态。
        /// 
        /// 口径说明：
        /// - 只要拿到高置信度正证据（LicenseStatus=1），就显示“已激活”
        /// - 拿到宽限期证据，则显示“宽限期”
        /// - 其余冲突或不稳定场景，不再武断判为“未激活”，统一显示“待确认”
        /// </summary>
        public 激活状态信息 获取激活状态(bool 强制刷新 = false)
        {
            if (!强制刷新 &&
                _激活状态缓存 != null &&
                DateTime.Now - _激活状态缓存时间 < 激活缓存时长)
            {
                return _激活状态缓存;
            }

            var 结果 = new 激活状态信息
            {
                状态标题 = "激活状态读取中",
                状态说明 = "正在读取当前系统授权信息。",
                状态标签 = "读取中",
                系统名称 = 读取系统名称(),
                版本名称 = 读取系统版本信息(),
                授权名称 = "未读取到",
                部分产品密钥 = "数字许可证或未读取到"
            };

            try
            {
                var 候选列表 = 读取Windows授权候选项();

                if (候选列表.Count == 0)
                {
                    结果.是否已激活 = false;
                    结果.状态标题 = "激活状态待确认";
                    结果.状态说明 = "当前未能从系统接口读取到稳定的 Windows 授权记录。若系统设置中显示已激活，请以系统设置为准。";
                    结果.状态标签 = "待确认";

                    写入激活缓存(结果);
                    return 结果;
                }

                // 优先选择最可信的项
                授权候选项? 已激活项 = 候选列表.FirstOrDefault(x => x.LicenseStatus == 1);
                授权候选项? 宽限期项 = 候选列表.FirstOrDefault(x => x.LicenseStatus == 2 || x.LicenseStatus == 3 || x.LicenseStatus == 6);
                授权候选项? 其他项 = 候选列表.FirstOrDefault();

                授权候选项 最终展示项 = 已激活项 ?? 宽限期项 ?? 其他项!;

                结果.授权名称 = string.IsNullOrWhiteSpace(最终展示项.Name)
                    ? "未读取到"
                    : 最终展示项.Name;

                结果.部分产品密钥 = string.IsNullOrWhiteSpace(最终展示项.PartialProductKey)
                    ? "数字许可证或未读取到"
                    : $"*****-*****-*****-*****-{最终展示项.PartialProductKey}";

                // 主判定逻辑：尽量低误报
                if (已激活项 != null)
                {
                    结果.是否已激活 = true;
                    结果.状态标题 = "系统已激活";
                    结果.状态说明 = "当前 Windows 已完成激活，无需额外操作。";
                    结果.状态标签 = "已激活";
                }
                else if (宽限期项 != null)
                {
                    结果.是否已激活 = false;
                    结果.状态标题 = "当前处于宽限期";
                    结果.状态说明 = "系统目前处于授权宽限期，建议尽快确认激活状态。";
                    结果.状态标签 = "宽限期";
                }
                else
                {
                    // 关键修复点：
                    // 这里不再直接写“未激活”，避免和系统设置冲突。
                    结果.是否已激活 = false;
                    结果.状态标题 = "激活状态待确认";
                    结果.状态说明 = "当前未能从系统接口稳定确认激活状态。若“设置 > 系统 > 激活”显示已激活，请以系统设置中的结果为准。";
                    结果.状态标签 = "待确认";
                }

                if (!string.IsNullOrWhiteSpace(最终展示项.Description))
                {
                    string 清洗描述 = 清洗文本(最终展示项.Description);

                    if (!string.IsNullOrWhiteSpace(清洗描述))
                    {
                        结果.状态说明 += $" 授权描述：{清洗描述}";
                    }
                }

                写入激活缓存(结果);
                return 结果;
            }
            catch (Exception ex)
            {
                结果.状态标题 = "激活状态读取失败";
                结果.状态说明 = $"读取失败：{ex.Message}";
                结果.状态标签 = "异常";
                结果.是否已激活 = false;

                写入激活缓存(结果);
                return 结果;
            }
        }

        /// <summary>
        /// 获取当前存在异常的驱动 / 设备问题列表。
        /// 只保留真正异常项：ConfigManagerErrorCode > 0。
        /// </summary>
        public List<驱动问题项> 获取驱动问题列表(bool 强制刷新 = false)
        {
            if (!强制刷新 &&
                _驱动问题缓存 != null &&
                DateTime.Now - _驱动问题缓存时间 < 驱动缓存时长)
            {
                return _驱动问题缓存;
            }

            var 列表 = new List<驱动问题项>();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, DeviceID, ConfigManagerErrorCode FROM Win32_PnPEntity");

                foreach (ManagementObject obj in searcher.Get())
                {
                    int errorCode = 读取整型值(obj["ConfigManagerErrorCode"]);

                    // 关键修复点：错误码 0 代表正常，绝不进入问题列表
                    if (errorCode <= 0)
                    {
                        continue;
                    }

                    string 设备名称 = obj["Name"]?.ToString() ?? "未命名设备";
                    string 设备Id = obj["DeviceID"]?.ToString() ?? "";

                    列表.Add(new 驱动问题项
                    {
                        设备名称 = 清洗文本(设备名称),
                        设备Id = 清洗文本(设备Id),
                        错误代码 = errorCode,
                        错误说明 = 获取驱动错误说明(errorCode)
                    });
                }
            }
            catch (Exception ex)
            {
                列表.Add(new 驱动问题项
                {
                    设备名称 = "驱动扫描失败",
                    设备Id = "",
                    错误代码 = -1,
                    错误说明 = ex.Message
                });
            }

            列表 = 列表
                .OrderBy(x => x.错误代码)
                .ThenBy(x => x.设备名称)
                .ToList();

            写入驱动缓存(列表);
            return 列表;
        }

        /// <summary>
        /// 打开 Windows 激活设置页。
        /// </summary>
        public void 打开激活设置()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:activation",
                UseShellExecute = true
            });
        }

        /// <summary>
        /// 打开设备管理器。
        /// </summary>
        public void 打开设备管理器()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "devmgmt.msc",
                UseShellExecute = true
            });
        }

        /// <summary>
        /// 读取所有 Windows 授权候选项。
        /// 注意：这里不要求 PartialProductKey 非空，因为数字许可证场景可能没有可读密钥。
        /// </summary>
        private List<授权候选项> 读取Windows授权候选项()
        {
            var 列表 = new List<授权候选项>();

            string query =
                "SELECT LicenseStatus, Name, Description, PartialProductKey, ApplicationID " +
                "FROM SoftwareLicensingProduct";

            using var searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject obj in searcher.Get())
            {
                if (!是否是Windows授权项(obj))
                {
                    continue;
                }

                列表.Add(new 授权候选项
                {
                    LicenseStatus = 读取整型值(obj["LicenseStatus"]),
                    Name = 清洗文本(obj["Name"]?.ToString() ?? ""),
                    Description = 清洗文本(obj["Description"]?.ToString() ?? ""),
                    PartialProductKey = 清洗文本(obj["PartialProductKey"]?.ToString() ?? ""),
                    ApplicationID = 清洗文本(obj["ApplicationID"]?.ToString() ?? "")
                });
            }

            return 列表;
        }

        /// <summary>
        /// 判断某条 SoftwareLicensingProduct 记录是否属于 Windows 授权项。
        /// </summary>
        private bool 是否是Windows授权项(ManagementObject obj)
        {
            string applicationId = obj["ApplicationID"]?.ToString() ?? "";
            string name = obj["Name"]?.ToString() ?? "";

            if (string.Equals(applicationId, WindowsApplicationId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return name.Contains("Windows", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 读取系统名称。
        /// 优先使用 Win32_OperatingSystem.Caption，避免 Win11 被错误显示为 Win10。
        /// </summary>
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
                        return 清洗文本(caption.Trim());
                    }
                }
            }
            catch
            {
                // 忽略，走下面降级逻辑
            }

            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                return 清洗文本(key?.GetValue("ProductName")?.ToString() ?? "Windows");
            }
            catch
            {
                return "Windows";
            }
        }

        /// <summary>
        /// 读取显示版本和构建号。
        /// </summary>
        private string 读取系统版本信息()
        {
            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                string displayVersion = 清洗文本(key?.GetValue("DisplayVersion")?.ToString() ?? "");
                string build = 清洗文本(key?.GetValue("CurrentBuildNumber")?.ToString() ?? "");

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

        /// <summary>
        /// 将激活结果写入缓存。
        /// </summary>
        private void 写入激活缓存(激活状态信息 结果)
        {
            _激活状态缓存 = 结果;
            _激活状态缓存时间 = DateTime.Now;
        }

        /// <summary>
        /// 将驱动结果写入缓存。
        /// </summary>
        private void 写入驱动缓存(List<驱动问题项> 列表)
        {
            _驱动问题缓存 = 列表;
            _驱动问题缓存时间 = DateTime.Now;
        }

        /// <summary>
        /// 安全读取整型值。
        /// </summary>
        private int 读取整型值(object? value)
        {
            try
            {
                if (value == null)
                {
                    return 0;
                }

                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 清洗文本，去除不可见控制字符，降低乱码影响。
        /// </summary>
        private string 清洗文本(string 原文)
        {
            if (string.IsNullOrWhiteSpace(原文))
            {
                return "";
            }

            var sb = new StringBuilder();

            foreach (char c in 原文)
            {
                if (!char.IsControl(c) || c == '\r' || c == '\n' || c == '\t')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// 映射常见驱动 / 设备错误码说明。
        /// </summary>
        private string 获取驱动错误说明(int errorCode)
        {
            return errorCode switch
            {
                1 => "设备未正确配置。",
                10 => "设备无法启动。",
                12 => "设备资源不足，无法使用。",
                14 => "设备需要重新启动后才能工作。",
                18 => "需要重新安装驱动程序。",
                22 => "设备已被禁用。",
                24 => "设备不存在、未正确工作或未安装所有驱动。",
                28 => "未安装该设备的驱动程序。",
                31 => "Windows 无法加载该设备所需的驱动程序。",
                32 => "该设备驱动已被禁用。",
                37 => "驱动程序初始化失败。",
                39 => "Windows 无法加载设备驱动程序。",
                43 => "Windows 已停止该设备，因为它报告了问题。",
                _ => "存在设备或驱动异常，建议进入设备管理器进一步确认。"
            };
        }

        /// <summary>
        /// 内部授权候选项。
        /// </summary>
        private class 授权候选项
        {
            public int LicenseStatus { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string PartialProductKey { get; set; } = "";
            public string ApplicationID { get; set; } = "";
        }
    }
}
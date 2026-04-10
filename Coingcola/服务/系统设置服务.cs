using Coingcola.模型;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coingcola.服务
{
    /// <summary>
    /// 系统设置服务。
    /// 承担“常用设置”页面的真实系统项读写能力。
    /// </summary>
    public class 系统设置服务
    {
        private const string ExplorerAdvancedPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private const string HideDesktopIconsNewStartPanelPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel";
        private const string ThisPcDesktopIconGuid = "{20D04FE0-3AEA-1069-A2D8-08002B30309D}";

        private readonly Dictionary<string, 设置项定义> _设置项字典;

        public 系统设置服务()
        {
            _设置项字典 = new Dictionary<string, 设置项定义>(StringComparer.OrdinalIgnoreCase)
            {
                ["show_hidden_files"] = new 设置项定义
                {
                    Id = "show_hidden_files",
                    名称 = "显示隐藏文件",
                    说明 = "在资源管理器中显示隐藏文件和文件夹。",
                    推荐是否开启 = false,
                    推荐说明 = "普通用户建议关闭，避免误操作系统文件。",
                    生效说明 = "需重启资源管理器后生效",
                    读取函数 = 读取显示隐藏文件,
                    写入函数 = 写入显示隐藏文件,
                    需要重启资源管理器 = true
                },
                ["show_file_extensions"] = new 设置项定义
                {
                    Id = "show_file_extensions",
                    名称 = "显示文件扩展名",
                    说明 = "显示 .txt / .zip / .exe 等扩展名。",
                    推荐是否开启 = true,
                    推荐说明 = "推荐开启，便于识别文件类型并降低误打开风险。",
                    生效说明 = "需重启资源管理器后生效",
                    读取函数 = 读取显示文件扩展名,
                    写入函数 = 写入显示文件扩展名,
                    需要重启资源管理器 = true
                },
                ["desktop_this_pc"] = new 设置项定义
                {
                    Id = "desktop_this_pc",
                    名称 = "桌面显示“此电脑”图标",
                    说明 = "在桌面保留“此电脑”入口，方便快速进入资源管理器。",
                    推荐是否开启 = true,
                    推荐说明 = "推荐开启，适合把“我的电脑”作为高频入口。",
                    生效说明 = "需重启资源管理器后生效",
                    读取函数 = 读取桌面此电脑图标,
                    写入函数 = 写入桌面此电脑图标,
                    需要重启资源管理器 = true
                },
                ["explorer_open_my_computer"] = new 设置项定义
                {
                    Id = "explorer_open_my_computer",
                    名称 = "资源管理器默认打开到“我的电脑”",
                    说明 = "打开资源管理器时优先进入“我的电脑”，而不是首页。",
                    推荐是否开启 = true,
                    推荐说明 = "推荐开启，更符合当前产品的任务前置习惯。",
                    生效说明 = "需重启资源管理器后生效",
                    读取函数 = 读取资源管理器默认到我的电脑,
                    写入函数 = 写入资源管理器默认到我的电脑,
                    需要重启资源管理器 = true
                }
            };
        }

        public List<系统开关项> 获取设置项列表()
        {
            var result = new List<系统开关项>();

            foreach (var item in _设置项字典.Values)
            {
                bool 当前是否开启;
                try
                {
                    当前是否开启 = item.读取函数();
                }
                catch
                {
                    当前是否开启 = item.推荐是否开启;
                }

                result.Add(new 系统开关项
                {
                    Id = item.Id,
                    名称 = item.名称,
                    说明 = item.说明,
                    当前是否开启 = 当前是否开启,
                    推荐是否开启 = item.推荐是否开启,
                    推荐说明 = item.推荐说明,
                    生效说明 = item.生效说明
                });
            }

            return result;
        }

        public int 获取待调整项数量()
        {
            return 获取设置项列表().Count(x => !x.是否符合推荐);
        }

        public (bool 成功, string 提示, bool 需要重启资源管理器) 应用推荐设置(string id)
        {
            if (!_设置项字典.TryGetValue(id, out var item))
            {
                return (false, "未找到要处理的设置项。", false);
            }

            try
            {
                item.写入函数(item.推荐是否开启);
                return (true, $"已按推荐处理：{item.名称}", item.需要重启资源管理器);
            }
            catch (Exception ex)
            {
                return (false, $"应用失败：{ex.Message}", false);
            }
        }

        public (bool 成功, string 提示, int 已变更数, bool 需要重启资源管理器) 应用全部推荐设置()
        {
            var 列表 = 获取设置项列表();
            int 已变更数 = 0;
            bool 需要重启资源管理器 = false;

            try
            {
                foreach (var 项 in 列表)
                {
                    if (项.是否符合推荐)
                    {
                        continue;
                    }

                    if (!_设置项字典.TryGetValue(项.Id, out var 定义))
                    {
                        continue;
                    }

                    定义.写入函数(定义.推荐是否开启);
                    已变更数++;
                    需要重启资源管理器 = 需要重启资源管理器 || 定义.需要重启资源管理器;
                }

                if (已变更数 == 0)
                {
                    return (true, "当前所有设置均已符合推荐，无需处理。", 0, false);
                }

                return (true, $"已按推荐处理 {已变更数} 项设置。", 已变更数, 需要重启资源管理器);
            }
            catch (Exception ex)
            {
                return (false, $"批量应用失败：{ex.Message}", 已变更数, false);
            }
        }

        public (bool 成功, string 提示, bool 需要重启资源管理器) 切换设置(string id)
        {
            if (!_设置项字典.TryGetValue(id, out var item))
            {
                return (false, "未找到要切换的设置项。", false);
            }

            try
            {
                bool 当前值 = item.读取函数();
                bool 新值 = !当前值;
                item.写入函数(新值);
                return (true, $"已切换：{item.名称}", item.需要重启资源管理器);
            }
            catch (Exception ex)
            {
                return (false, $"切换失败：{ex.Message}", false);
            }
        }

        public (bool 成功, string 提示) 重启资源管理器()
        {
            try
            {
                using (var killProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM explorer.exe",
                    UseShellExecute = true,
                    CreateNoWindow = true
                }))
                {
                    killProcess?.WaitForExit();
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });

                return (true, "已重启资源管理器。");
            }
            catch (Exception ex)
            {
                return (false, $"重启资源管理器失败：{ex.Message}");
            }
        }

        private bool 读取显示隐藏文件()
        {
            object? value = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedPath)?.GetValue("Hidden");
            int hidden = value is int intValue ? intValue : 2;
            return hidden == 1;
        }

        private void 写入显示隐藏文件(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.CreateSubKey(ExplorerAdvancedPath);
            key?.SetValue("Hidden", enabled ? 1 : 2, RegistryValueKind.DWord);
        }

        private bool 读取显示文件扩展名()
        {
            object? value = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedPath)?.GetValue("HideFileExt");
            int hide = value is int intValue ? intValue : 0;
            return hide == 0;
        }

        private void 写入显示文件扩展名(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.CreateSubKey(ExplorerAdvancedPath);
            key?.SetValue("HideFileExt", enabled ? 0 : 1, RegistryValueKind.DWord);
        }

        private bool 读取桌面此电脑图标()
        {
            object? value = Registry.CurrentUser.OpenSubKey(HideDesktopIconsNewStartPanelPath)?.GetValue(ThisPcDesktopIconGuid);
            int hidden = value is int intValue ? intValue : 0;
            return hidden == 0;
        }

        private void 写入桌面此电脑图标(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.CreateSubKey(HideDesktopIconsNewStartPanelPath);
            key?.SetValue(ThisPcDesktopIconGuid, enabled ? 0 : 1, RegistryValueKind.DWord);
        }

        private bool 读取资源管理器默认到我的电脑()
        {
            object? value = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedPath)?.GetValue("LaunchTo");
            int launchTo = value is int intValue ? intValue : 1;
            return launchTo == 1;
        }

        private void 写入资源管理器默认到我的电脑(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.CreateSubKey(ExplorerAdvancedPath);
            key?.SetValue("LaunchTo", enabled ? 1 : 2, RegistryValueKind.DWord);
        }

        private sealed class 设置项定义
        {
            public string Id { get; set; } = string.Empty;
            public string 名称 { get; set; } = string.Empty;
            public string 说明 { get; set; } = string.Empty;
            public bool 推荐是否开启 { get; set; }
            public string 推荐说明 { get; set; } = string.Empty;
            public string 生效说明 { get; set; } = string.Empty;
            public Func<bool> 读取函数 { get; set; } = default!;
            public Action<bool> 写入函数 { get; set; } = default!;
            public bool 需要重启资源管理器 { get; set; }
        }
    }
}


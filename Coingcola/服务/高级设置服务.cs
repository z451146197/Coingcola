using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coingcola.服务
{
    /// <summary>
    /// 高级设置服务。
    /// 
    /// 当前策略：
    /// - 只做系统入口聚合，不直接篡改系统配置
    /// - 优先放置高频、高价值、通用性强的入口
    /// - 让“高级能力后置，但可快速到达”
    /// </summary>
    public class 高级设置服务
    {
        private readonly List<高级设置入口项> _入口列表 = new()
        {
            new 高级设置入口项
            {
                Id = "windows_update",
                名称 = "Windows 更新",
                分类 = "系统",
                说明 = "快速进入 Windows 更新设置页。",
                风险级别 = "低风险",
                生效说明 = "仅打开系统设置入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "advanced_system",
                名称 = "高级系统属性",
                分类 = "系统",
                说明 = "进入系统属性高级页，可继续查看性能、用户配置文件、启动和故障恢复等。",
                风险级别 = "需谨慎",
                生效说明 = "仅打开系统入口，不会自动修改设置。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "startup_apps",
                名称 = "启动应用",
                分类 = "系统",
                说明 = "管理开机自启动应用，减少开机负担。",
                风险级别 = "低风险",
                生效说明 = "仅打开系统设置入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "optional_features",
                名称 = "可选功能",
                分类 = "系统",
                说明 = "查看或安装系统可选功能。",
                风险级别 = "低风险",
                生效说明 = "仅打开系统设置入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "installed_apps",
                名称 = "已安装应用",
                分类 = "应用",
                说明 = "查看和卸载当前已安装的软件。",
                风险级别 = "低风险",
                生效说明 = "仅打开系统设置入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "default_apps",
                名称 = "默认应用",
                分类 = "应用",
                说明 = "设置浏览器、播放器、图片查看器等默认应用。",
                风险级别 = "低风险",
                生效说明 = "仅打开系统设置入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "device_manager",
                名称 = "设备管理器",
                分类 = "硬件",
                说明 = "查看设备状态、驱动情况与硬件异常。",
                风险级别 = "需谨慎",
                生效说明 = "仅打开系统工具入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "disk_management",
                名称 = "磁盘管理",
                分类 = "硬件",
                说明 = "查看磁盘、分区和卷信息。",
                风险级别 = "需谨慎",
                生效说明 = "仅打开系统工具入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "services",
                名称 = "服务",
                分类 = "工具",
                说明 = "查看和管理系统服务。",
                风险级别 = "需谨慎",
                生效说明 = "仅打开系统工具入口。",
                按钮文本 = "打开入口"
            },
            new 高级设置入口项
            {
                Id = "task_scheduler",
                名称 = "任务计划程序",
                分类 = "工具",
                说明 = "查看系统定时任务和触发器。",
                风险级别 = "需谨慎",
                生效说明 = "仅打开系统工具入口。",
                按钮文本 = "打开入口"
            }
        };

        public List<高级设置入口项> 获取入口列表()
        {
            return _入口列表
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.名称)
                .ToList();
        }

        public List<高级设置入口项> 搜索入口(string 关键词, string 分类 = "全部")
        {
            IEnumerable<高级设置入口项> 查询 = _入口列表;

            if (!string.IsNullOrWhiteSpace(分类) && 分类 != "全部")
            {
                查询 = 查询.Where(x => x.分类 == 分类);
            }

            if (!string.IsNullOrWhiteSpace(关键词))
            {
                string key = 关键词.Trim();

                查询 = 查询.Where(x =>
                    包含文本(x.名称, key) ||
                    包含文本(x.分类, key) ||
                    包含文本(x.说明, key));
            }

            return 查询
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.名称)
                .ToList();
        }

        public (bool 成功, string 提示) 打开入口(string id)
        {
            try
            {
                return id switch
                {
                    "windows_update" => 打开("ms-settings:windowsupdate", "", true, "已打开 Windows 更新。"),
                    "advanced_system" => 打开("SystemPropertiesAdvanced.exe", "", true, "已打开高级系统属性。"),
                    "startup_apps" => 打开("ms-settings:startupapps", "", true, "已打开启动应用。"),
                    "optional_features" => 打开("ms-settings:optionalfeatures", "", true, "已打开可选功能。"),
                    "installed_apps" => 打开("ms-settings:appsfeatures", "", true, "已打开已安装应用。"),
                    "default_apps" => 打开("ms-settings:defaultapps", "", true, "已打开默认应用。"),
                    "device_manager" => 打开("devmgmt.msc", "", true, "已打开设备管理器。"),
                    "disk_management" => 打开("diskmgmt.msc", "", true, "已打开磁盘管理。"),
                    "services" => 打开("services.msc", "", true, "已打开服务。"),
                    "task_scheduler" => 打开("taskschd.msc", "", true, "已打开任务计划程序。"),
                    _ => (false, "未识别的高级设置入口。")
                };
            }
            catch (Exception ex)
            {
                return (false, $"打开失败：{ex.Message}");
            }
        }

        public int 获取低风险数量()
        {
            return _入口列表.Count(x => x.风险级别 == "低风险");
        }

        public int 获取需谨慎数量()
        {
            return _入口列表.Count(x => x.风险级别 == "需谨慎");
        }

        private (bool 成功, string 提示) 打开(string fileName, string arguments, bool useShellExecute, string 成功提示)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = useShellExecute
            };

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                psi.Arguments = arguments;
            }

            Process.Start(psi);
            return (true, 成功提示);
        }

        private int 分类排序值(string 分类)
        {
            return 分类 switch
            {
                "系统" => 1,
                "应用" => 2,
                "硬件" => 3,
                "工具" => 4,
                _ => 99
            };
        }

        private bool 包含文本(string 原文, string 关键词)
        {
            return (原文 ?? string.Empty)
                .Contains(关键词, StringComparison.OrdinalIgnoreCase);
        }
    }
}